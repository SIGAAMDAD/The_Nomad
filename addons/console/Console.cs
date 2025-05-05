using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public partial class Console : Control {
	[Export]
	public bool Enabled { get; set; } = true;

	[Export]
	public bool EnableOnReleaseBuild {
		get => _enableOnReleaseBuild;
		set
		{
			_enableOnReleaseBuild = value;
			if ( !_enableOnReleaseBuild && !OS.IsDebugBuild() ) {
				Disable();
			}
		}
	}
	private bool _enableOnReleaseBuild = false;

	private static Console Instance;

	[Export]
	public bool PauseEnabled { get; set; } = false;

	[Signal]
	public delegate void ConsoleOpenedEventHandler();
	[Signal]
	public delegate void ConsoleClosedEventHandler();
	[Signal]
	public delegate void ConsoleUnknownCommandEventHandler( string command );

	public class ConsoleCommand {
		public Callable Function { get; set; }
		public string[] Arguments { get; set; }
		public int Required { get; set; }
		public string Description { get; set; }
		public bool Hidden { get; set; }

		public ConsoleCommand( Callable function, string[] arguments, int required = 0, string description = "" ) {
			Function = function;
			Arguments = arguments;
			Required = required;
			Description = description;
		}
	}

	private Dictionary<string, ConsoleCommand> ConsoleCommands = new Dictionary<string, ConsoleCommand>();
	private Dictionary<string, string[]> CommandParameters = new Dictionary<string, string[]>();
	private List<string> ConsoleHistory = new List<string>();
	private int ConsoleHistoryIndex = 0;
	private bool _wasPausedAlready = false;

	private List<string> Suggestions = new List<string>();
	private int CurrentSuggest = 0;
	private bool Suggesting = false;

	private static RichTextLabel RichLabel;
	private LineEdit LineEdit;
	public static Control Control;
	private CanvasLayer CanvasLayer;

	public override void _EnterTree() {
		Instance = this;

		CanvasLayer = new CanvasLayer();
		CanvasLayer.Layer = 3;
		AddChild( CanvasLayer );

		Control = new Control();
		Control.AnchorBottom = 1.0f;
		Control.AnchorRight = 1.0f;
		CanvasLayer.AddChild( Control );

		var style = new StyleBoxFlat();
		style.BgColor = new Color( 0, 0, 0, 0.84f );

		RichLabel = new RichTextLabel();
		RichLabel.SelectionEnabled = true;
		RichLabel.ContextMenuEnabled = true;
		RichLabel.BbcodeEnabled = true;
		RichLabel.ScrollFollowing = true;
		RichLabel.AnchorRight = 1.0f;
		RichLabel.AnchorBottom = 0.5f;
		RichLabel.AddThemeStyleboxOverride( "normal", style );
		Control.AddChild( RichLabel );
		RichLabel.AppendText( "Development console.\n" );

		LineEdit = new LineEdit();
		LineEdit.AnchorTop = 0.5f;
		LineEdit.AnchorRight = 1.0f;
		LineEdit.AnchorBottom = 0.5f;
		LineEdit.PlaceholderText = "Enter \"help\" for instructions";
		Control.AddChild( LineEdit );
		LineEdit.TextSubmitted += OnTextEntered;
		LineEdit.TextChanged += OnLineEditTextChanged;
		Control.Visible = false;

		ProcessMode = ProcessModeEnum.Always;

		// Load history
		using var file = FileAccess.Open( "user://console_history.txt", FileAccess.ModeFlags.Read );
		if ( file != null ) {
			while ( !file.EofReached() ) {
				var line = file.GetLine();
				if ( line.Length > 0 ) {
					AddInputHistory( line );
				}
			}
		}

		InitializeCommands();
	}

	private void InitializeCommands() {
		AddCommand( "quit", new Callable( this, MethodName.Quit ), Array.Empty<string>(), 0, "Quits the game." );
		AddCommand( "exit", new Callable( this, MethodName.Quit ), Array.Empty<string>(), 0, "Quits the game." );
		AddCommand( "clear", new Callable( this, MethodName.Clear ), Array.Empty<string>(), 0, "Clears the text on the console." );
		AddCommand( "delete_history", new Callable( this, MethodName.DeleteHistory ), Array.Empty<string>(), 0, "Deletes the history of previously entered commands." );
		AddCommand( "help", new Callable( this, MethodName.Help ), Array.Empty<string>(), 0, "Displays instructions on how to use the console." );
		AddCommand( "commands_list", new Callable( this, MethodName.CommandsList ), Array.Empty<string>(), 0, "Lists all commands and their descriptions." );
		AddCommand( "commands", new Callable( this, MethodName.Commands ), Array.Empty<string>(), 0, "Lists commands with no descriptions." );
		AddCommand( "calc", new Callable( this, MethodName.Calculate ), new[] { "mathematical expression to evaluate" }, 0, "Evaluates the math passed in for quick arithmetic." );
		AddCommand( "echo", new Callable( this, MethodName.PrintLine ), new[] { "string" }, 1, "Prints given string to the console." );
		AddCommand( "echo_warning", new Callable( this, MethodName.PrintWarning ), new[] { "string" }, 1, "Prints given string as warning to the console." );
		AddCommand( "echo_debug", new Callable( this, MethodName.PrintDebug ), new[] { "string" }, 1, "Prints given string as debug to the console." );
		AddCommand( "echo_error", new Callable( this, MethodName.PrintError ), new[] { "string" }, 1, "Prints given string as an error to the console." );
		AddCommand( "pause", new Callable( this, MethodName.Pause ), Array.Empty<string>(), 0, "Pauses node processing." );
		AddCommand( "unpause", new Callable( this, MethodName.Unpause ), Array.Empty<string>(), 0, "Unpauses node processing." );
		AddCommand( "exec", new Callable( this, MethodName.Exec ), new[] { "filename" }, 1, "Execute a script." );
	}

	public static void AddCommand( string commandName, Callable function, string[] arguments = null, int required = 0, string description = "" ) {
		arguments ??= Array.Empty<string>();
		Instance.ConsoleCommands.TryAdd( commandName, new ConsoleCommand( function, arguments, required, description ) );
	}

	public static void RemoveCommand( string commandName ) {
		Instance.ConsoleCommands.Remove( commandName );
		Instance.CommandParameters.Remove( commandName );
	}

	public static void AddCommandAutocompleteList( string commandName, string[] paramList ) {
		Instance.CommandParameters.TryAdd( commandName, paramList );
	}

	private void SaveHistory() {
		using var file = FileAccess.Open( "user://console_history.txt", FileAccess.ModeFlags.Write );
		if ( file != null ) {
			int startIndex = Math.Max( 0, ConsoleHistory.Count - 100 ); // Max 100 lines
			for ( int i = startIndex; i < ConsoleHistory.Count; i++ ) {
				file.StoreLine( ConsoleHistory[i] );
			}
		}
	}

	public override void _Input( InputEvent @event ) {
		if ( @event is InputEventKey keyEvent && keyEvent.Pressed ) {
			// ~ key
			if ( keyEvent.GetPhysicalKeycodeWithModifiers() == Key.Quoteleft ) {
				ToggleConsole();
				GetViewport().SetInputAsHandled();
			} else if ( keyEvent.PhysicalKeycode == Key.Quoteleft && keyEvent.IsCommandOrControlPressed() ) {
				if ( Control.Visible ) {
					ToggleSize();
				} else {
					ToggleConsole();
					ToggleSize();
				}
				GetViewport().SetInputAsHandled();
			} else if ( keyEvent.GetPhysicalKeycodeWithModifiers() == Key.Escape && Control.Visible ) {
				ToggleConsole();
				GetViewport().SetInputAsHandled();
			}

			if ( Control.Visible ) {
				if ( keyEvent.GetPhysicalKeycodeWithModifiers() == Key.Up ) {
					GetViewport().SetInputAsHandled();
					if ( ConsoleHistoryIndex > 0 ) {
						ConsoleHistoryIndex--;
						if ( ConsoleHistoryIndex >= 0 ) {
							LineEdit.Text = ConsoleHistory[ ConsoleHistoryIndex ];
							LineEdit.CaretColumn = LineEdit.Text.Length;
							ResetAutocomplete();
						}
					}
				}
				else if ( keyEvent.GetPhysicalKeycodeWithModifiers() == Key.Down ) {
					GetViewport().SetInputAsHandled();
					if ( ConsoleHistoryIndex < ConsoleHistory.Count ) {
						ConsoleHistoryIndex++;
						if ( ConsoleHistoryIndex < ConsoleHistory.Count ) {
							LineEdit.Text = ConsoleHistory[ ConsoleHistoryIndex ];
							LineEdit.CaretColumn = LineEdit.Text.Length;
							ResetAutocomplete();
						}
						else {
							LineEdit.Text = "";
							ResetAutocomplete();
						}
					}
				}
				else if ( keyEvent.GetPhysicalKeycodeWithModifiers() == Key.Pageup ) {
					var scroll = RichLabel.GetVScrollBar();
					var tween = CreateTween();
					tween.TweenProperty( scroll, "value", scroll.Value - ( scroll.Page - scroll.Page * 0.1f ), 0.1f );
					GetViewport().SetInputAsHandled();
				}
				else if ( keyEvent.GetPhysicalKeycodeWithModifiers() == Key.Pagedown ) {
					var scroll = RichLabel.GetVScrollBar();
					var tween = CreateTween();
					tween.TweenProperty( scroll, "value", scroll.Value + ( scroll.Page - scroll.Page * 0.1f ), 0.1f );
					GetViewport().SetInputAsHandled();
				}
				else if ( keyEvent.GetPhysicalKeycodeWithModifiers() == Key.Tab ) {
					Autocomplete();
					GetViewport().SetInputAsHandled();
				}
			}
		}
	}

	private void Autocomplete() {
		if ( Suggesting ) {
			if ( CurrentSuggest < Suggestions.Count ) {
				LineEdit.Text = Suggestions[ CurrentSuggest ];
				LineEdit.CaretColumn = LineEdit.Text.Length;
				CurrentSuggest = ( CurrentSuggest + 1 ) % Suggestions.Count;
			}
			return;
		}

		Suggesting = true;

		if ( LineEdit.Text.Contains( ' ' ) ) {
			var splitText = ParseLineInput( LineEdit.Text );
			if ( splitText.Length > 1 ) {
				var command = splitText[0];
				var paramInput = splitText[1];
				if ( CommandParameters.TryGetValue( command, out var parameters ) ) {
					for ( int i = 0; i < parameters.Length; i++ ) {
						if ( parameters[i].Contains( paramInput ) ) {
							Suggestions.Add( $"{command} {parameters[i]}" );
						}
					}
				}
			}
		}
		else {
			var sortedCommands = ConsoleCommands
				.Where( c => !c.Value.Hidden )
				.Select( c => c.Key )
				.OrderBy( c => c )
				.ToList();
			
			for ( int i = 0; i < sortedCommands.Count; i++ ) {
				if ( string.IsNullOrEmpty( LineEdit.Text ) || sortedCommands[i].Contains( LineEdit.Text ) ) {
					Suggestions.Add( sortedCommands[i] );
				}
			}
		}

		if ( Suggestions.Count > 0 ) {
			Autocomplete();
		} else {
			ResetAutocomplete();
		}
	}

	private void ResetAutocomplete() {
		Suggestions.Clear();
		CurrentSuggest = 0;
		Suggesting = false;
	}

	private void ToggleSize() {
		if ( Control.AnchorBottom == 1.0f ) {
			Control.AnchorBottom = 1.9f;
		} else {
			Control.AnchorBottom = 1.0f;
		}
	}

	public void Disable() {
		Enabled = false;
		ToggleConsole(); // Ensure hidden if opened
	}

	public void Enable() {
		Enabled = true;
	}

	public void ToggleConsole() {
		if ( Enabled ) {
			Control.Visible = !Control.Visible;
		} else {
			Control.Visible = false;
		}

		if ( Control.Visible ) {
			_wasPausedAlready = GetTree().Paused;
			GetTree().Paused = _wasPausedAlready || PauseEnabled;
			LineEdit.GrabFocus();
			EmitSignal( SignalName.ConsoleOpened );
		}
		else {
			Control.AnchorBottom = 1.0f;
			ScrollToBottom();
			ResetAutocomplete();
			if ( PauseEnabled && !_wasPausedAlready ) {
				GetTree().Paused = false;
			}
			EmitSignal( SignalName.ConsoleClosed );
		}
	}

	private void ScrollToBottom() {
		var scroll = RichLabel.GetVScrollBar();
		scroll.Value = scroll.MaxValue - scroll.Page;
	}

	private void OnTextEntered( string newText ) {
		ScrollToBottom();
		ResetAutocomplete();
		LineEdit.Clear();
		LineEdit.CallDeferred( LineEdit.MethodName.Clear );

		if ( newText.Trim().Length > 0 ) {
			AddInputHistory( newText );
			PrintLine( $"[i]> {newText}[/i]" );
			var textSplit = ParseLineInput( newText );
			var textCommand = textSplit.Length > 0 ? textSplit[0] : "";

			if ( !string.IsNullOrEmpty( textCommand ) && ConsoleCommands.TryGetValue( textCommand, out var consoleCommand ) ) {
				var arguments = textSplit.Skip( 1 ).ToArray();

				// calc is a special command that needs special treatment
				if ( textCommand == "calc" ) {
					var expression = string.Join( "", arguments );
					consoleCommand.Function.Call( expression );
					SaveHistory();
					return;
				}

				if ( arguments.Length < consoleCommand.Required ) {
					PrintError( $"Too few arguments! Required < {consoleCommand.Required}>" );
					return;
				} else if ( arguments.Length > consoleCommand.Arguments.Length ) {
					arguments = arguments.Take( consoleCommand.Arguments.Length ).ToArray();
				}

				// Fill out with empty strings if needed
				var finalArgs = arguments.Concat( Enumerable.Repeat( "", consoleCommand.Arguments.Length - arguments.Length ) ).ToArray();

				try {
					consoleCommand.Function.Call( finalArgs );
					SaveHistory();
				} catch ( Exception ex ) {
					PrintError( $"Error executing command: {ex.Message}" );
				}
			}
			else if ( !string.IsNullOrEmpty( textCommand ) ) {
				EmitSignal( SignalName.ConsoleUnknownCommand, textCommand );
				PrintError( "Command not found." );
			}
		}
	}

	private void OnLineEditTextChanged( string newText ) {
		ResetAutocomplete();
	}

	private string[] ParseLineInput( string text ) {
		var result = new List<string>();
		var current = new StringBuilder();
		bool inQuotes = false;
		bool escaped = false;

		foreach ( char c in text ) {
			if ( escaped ) {
				current.Append( c );
				escaped = false;
				continue;
			}

			if ( c == '\\' ) {
				escaped = true;
				continue;
			}

			if ( c == '"' ) {
				inQuotes = !inQuotes;
				continue;
			}

			if ( char.IsWhiteSpace( c ) && !inQuotes ) {
				if ( current.Length > 0 ) {
					result.Add( current.ToString() );
					current.Clear();
				}
				continue;
			}

			current.Append( c );
		}

		if ( current.Length > 0 ) {
			result.Add( current.ToString() );
		}

		return [ .. result ];
	}

	private void AddInputHistory( string text ) {
		if ( ConsoleHistory.Count == 0 || text != ConsoleHistory.Last() ) {
			ConsoleHistory.Add( text );
		}
		ConsoleHistoryIndex = ConsoleHistory.Count;
	}

	// Command implementations
	private void Quit() {
		GetTree().Quit();
	}

	private void Clear() {
		RichLabel.Clear();
	}

	private void DeleteHistory() {
		ConsoleHistory.Clear();
		ConsoleHistoryIndex = 0;
		DirAccess.RemoveAbsolute( "user://console_history.txt" );
	}

	private void Help() {
		RichLabel.AppendText(
			@"	Built in commands:
			[color=light_green]calc[/color]: Calculates a given expresion
			[color=light_green]clear[/color]: Clears the registry view
			[color=light_green]commands[/color]: Shows a reduced list of all the currently registered commands
			[color=light_green]commands_list[/color]: Shows a detailed list of all the currently registered commands
			[color=light_green]delete_history[/color]: Deletes the commands history
			[color=light_green]echo[/color]: Prints a given string to the console
			[color=light_green]echo_error[/color]: Prints a given string as an error to the console
			[color=light_green]echo_info[/color]: Prints a given string as info to the console
			[color=light_green]echo_warning[/color]: Prints a given string as warning to the console
			[color=light_green]pause[/color]: Pauses node processing
			[color=light_green]unpause[/color]: Unpauses node processing
			[color=light_green]quit[/color]: Quits the game
		Controls:
			[color=light_blue]Up[/color] and [color=light_blue]Down[/color] arrow keys to navigate commands history
			[color=light_blue]PageUp[/color] and [color=light_blue]PageDown[/color] to scroll registry
			[[color=light_blue]Ctrl[/color] + [color=light_blue]~[/color]] to change console size between half screen and full screen
			[color=light_blue]~[/color] or [color=light_blue]Esc[/color] key to close the console
			[color=light_blue]Tab[/color] key to autocomplete, [color=light_blue]Tab[/color] again to cycle between matching suggestions\n\n"
		);
	}

	private void Calculate( string expression ) {
		var expr = new Expression();
		var error = expr.Parse( expression );
		if ( error != Error.Ok ) {
			PrintError( $"{expr.GetErrorText()}" );
			return;
		}
		var result = expr.Execute();
		if ( !expr.HasExecuteFailed() ) {
			PrintLine( result.ToString() );
		} else {
			PrintError( $"{expr.GetErrorText()}" );
		}
	}

	private void Commands() {
		var commands = ConsoleCommands
			.Where( c => !c.Value.Hidden )
			.Select( c => c.Key )
			.OrderBy( c => c )
			.ToList();

		RichLabel.AppendText( "	" );
		RichLabel.AppendText( $"{string.Join( ", ", commands )}\n\n" );
	}

	private void CommandsList() {
		var commands = ConsoleCommands
			.Where( c => !c.Value.Hidden )
			.OrderBy( c => c.Key )
			.ToList();

		foreach ( var ( command, cmdInfo ) in commands ) {
			var argumentsString = new StringBuilder();
			for ( int i = 0; i < cmdInfo.Arguments.Length; i++ ) {
				if ( i < cmdInfo.Required ) {
					argumentsString.Append( $"  [color=cornflower_blue]<{cmdInfo.Arguments[i]}>[/color]" );
				} else {
					argumentsString.Append( $"  <{cmdInfo.Arguments[i]}>" );
				}
			}
			RichLabel.AppendText( $"	[color=light_green]{command}[/color][color=gray]{argumentsString}[/color]:   {cmdInfo.Description}\n" );
		}
		RichLabel.AppendText( "\n" );
	}

	private void Pause() {
		GetTree().Paused = true;
	}

	private void Unpause() {
		GetTree().Paused = false;
	}

	private void Exec( string filename ) {
		string path = $"user://{filename}.txt";
		using var file = FileAccess.Open( path, FileAccess.ModeFlags.Read );
		if ( file != null ) {
			while ( !file.EofReached() ) {
				OnTextEntered( file.GetLine() );
			}
		}
		else {
			PrintError( $"File {path} not found." );
		}
	}

	public static void PrintLine( string text ) {
		if ( !IsInstanceValid( RichLabel ) ) {
			Instance.CallDeferred( MethodName.PrintLine, text, true );
		} else {
			RichLabel.CallDeferred( "append_text", text );
			RichLabel.CallDeferred( "append_text", "\n" );
			GD.Print( text );
		}
	}

	public static void PrintError( string text ) {
		PrintLine( $"[color=light_coral]   ERROR:[/color] {text}" );
	}

	public static void PrintDebug( string text ) {
		PrintLine( $"[color=light_blue]   DEBUG:[/color] {text}" );
	}

	public static void PrintWarning( string text ) {
		PrintLine( $"[color=gold]   WARNING:[/color] {text}" );
	}
};