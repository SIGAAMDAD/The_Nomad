using Godot;

public partial class Console : Node {
	private static Node Instance;

	public override void _EnterTree() {
		base._EnterTree();

		Instance = GetTree().Root.GetNode( "/root/GDConsole" );
	}

	public static void AddCommand( string name, Callable fn, Godot.Collections.Array args, int required, string description = "" ) {
		Instance.CallDeferred( "add_command", name, fn, args, required, description );
	}
	public static void RemoveCommand( string name ) {
		Instance.CallDeferred( "remove_command", name );
	}
	public static void PrintLine( string text ) {
		Instance.CallDeferred( "print_line", text, true );
	}
	public static void PrintError( string text ) {
		Instance.CallDeferred( "print_error", text, true );
	}
	public static void PrintWarning( string text ) {
		Instance.CallDeferred( "print_warning", text, true );
	}
};

/*
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using Godot;
using Steamworks;

public partial class Console : Node {
	private bool Enabled = true;
	private bool PauseEnabled = false;

	[Signal]
	public delegate void ConsoleOpenedEventHandler();
	[Signal]
	public delegate void ConsoleClosedEventHandler();
	[Signal]
	public delegate void ConsoleUnknownCommandEventHandler();

	private class ConsoleCommand {
		public Callable Function;
		public string[] Arguments;
		public int RequiredArgs;
		public string Description;
		public bool Hidden = false;

		public ConsoleCommand( Callable fn, string[] arguments, int requiredArgs, string description = "" ) {
			Function = fn;
			Arguments = arguments;
			RequiredArgs = requiredArgs;
			Description = description;
		}
	};

	private Control Control = new Control();

	private static RichTextLabel RichLabel = new RichTextLabel();
	private static LineEdit LineEdit = new LineEdit();

	private static Dictionary<string, ConsoleCommand> ConsoleCommands = new Dictionary<string, ConsoleCommand>();
	private static Dictionary<string, string[]> CommandParameters = new Dictionary<string, string[]>();
	private static List<string> ConsoleHistory = new List<string>();
	private static int ConsoleHistoryIndex = 0;
	private static bool WasPausedAlready = false;

	public static void AddCommand( string name, Callable fn, string[] args, int requiredArgs, string description = "" ) {
		ConsoleCommands.TryAdd( name, new ConsoleCommand( fn, args, requiredArgs, description ) );
	}
	public static void RemoveCommand( string name ) {
		ConsoleCommands.Remove( name );
		CommandParameters.Remove( name );
	}
	public static void AddCommandAutocompleteList( string name, string[] paramList ) {
		CommandParameters.Add( name, paramList );
	}

	private void HistoryPrev() {
		if ( ConsoleHistoryIndex == 0 ) {
			return;
		}

		ConsoleHistoryIndex--;
		if ( ConsoleHistoryIndex >= 0 ) {
			LineEdit.Text = ConsoleHistory[ ConsoleHistoryIndex ];
			LineEdit.CaretColumn = LineEdit.Text.Length;
			ResetAutocomplete();
		}
	}
	private void HistoryNext() {
		if ( ConsoleHistoryIndex == ConsoleHistory.Count - 1 ) {
			LineEdit.Clear();
			return;
		}

		ConsoleHistoryIndex++;
		if ( ConsoleHistoryIndex < ConsoleHistory.Count ) {
			LineEdit.Text = ConsoleHistory[ ConsoleHistoryIndex ];
			LineEdit.CaretColumn = LineEdit.Text.Length;
		}
		ResetAutocomplete();
	}

	public override void _EnterTree() {
		base._EnterTree();

		FileAccess file = FileAccess.Open( "user://history.txt", FileAccess.ModeFlags.Read );
		if ( file != null ) {
			while ( !file.EofReached() ) {
				string line = file.GetLine();
				if ( line.Length > 0 ) {
					//AddInputHistory( line );
				}
			}
		}

		CanvasLayer canvasLayer = new CanvasLayer();
		canvasLayer.Layer = 3;
		AddChild( canvasLayer );
		Control.AnchorBottom = 1.0f;
		Control.AnchorRight = 1.0f;
		canvasLayer.AddChild( Control );
		
		StyleBoxFlat style = new StyleBoxFlat();
		style.BgColor = new Color( "000000d7" );

		RichLabel.SelectionEnabled = true;
		RichLabel.ContextMenuEnabled = true;
		RichLabel.BbcodeEnabled = true;
		RichLabel.ScrollFollowing = true;
		RichLabel.AnchorRight = 1.0f;
		RichLabel.AnchorBottom = 0.5f;
		RichLabel.AddThemeStyleboxOverride( "normal", style );
		RichLabel.AppendText( "DEVELOPER CONSOLE\n" );
		Control.AddChild( RichLabel );

		LineEdit.AnchorTop = 0.5f;
		LineEdit.AnchorRight = 1.0f;
		LineEdit.AnchorBottom = 0.5f;
		LineEdit.PlaceholderText = "> ";
		LineEdit.Connect( "text_submitted", Callable.From<string>( OnTextEntered ) );
		LineEdit.Connect( "text_changed", Callable.From<string>( OnLineEditTextChanged ) );
		Control.AddChild( LineEdit );

		Control.Visible = true;
		ProcessMode = ProcessModeEnum.Always;
	}
	public override void _ExitTree() {
		base._ExitTree();

		FileAccess file = FileAccess.Open( "user://history.txt", FileAccess.ModeFlags.Write );
		if ( file != null ) {
			int writeIndex = 0;
			int startWriteIndex = ConsoleHistory.Count;
			foreach ( var line in ConsoleHistory ) {
				if ( writeIndex >= startWriteIndex ) {
					file.StoreLine( line );
				}
				writeIndex++;
			}
		}
	}
	public override void _Ready() {
		base._Ready();
	}
	public override void _Input( InputEvent @event ) {
		base._Input( @event );

		if ( @event.IsActionPressed( "toggle_console" ) ) {
			ToggleConsole();
			GetTree().Root.SetInputAsHandled();
			return;
		}
		if ( !Control.Visible ) {
			return;
		}

		bool validInput = false;

		if ( @event.IsActionPressed( "console_history_prev" ) ) {
			HistoryPrev();
			validInput = true;
		} else if ( @event.IsActionPressed( "console_history_next" ) ) {
			HistoryNext();
			validInput = true;
		} else if ( @event.IsActionPressed( "console_bottom" ) ) {
			ScrollBar scroll = RichLabel.GetVScrollBar();
			scroll.Value -= scroll.Page - scroll.Page * 0.1f;
			validInput = true;
		} else if ( @event.IsActionPressed( "console_top" ) ) {
			ScrollBar scroll = RichLabel.GetVScrollBar();
			scroll.Value += scroll.Page + scroll.Page * 0.1f;
			validInput = true;
		} else if ( @event.IsActionPressed( "console_autocomplete" ) ) {
			Autocomplete();
			validInput = true;
		}

		if ( validInput ) {
			GetTree().Root.SetInputAsHandled();
		}
	}

	private List<string> Suggestions = new List<string>();
	private int CurrentSuggest = 0;
	private bool Suggesting = false;

	private void Autocomplete() {
		if ( Suggesting ) {
			for ( int i = 0; i < Suggestions.Count; i++ ) {
				if ( CurrentSuggest == i ) {
					LineEdit.Text = Suggestions[i];
					LineEdit.CaretColumn = LineEdit.Text.Length;
					if ( CurrentSuggest == Suggestions.Count - 1 ) {
						CurrentSuggest = 0;
					} else {
						CurrentSuggest++;
					}
				}
			}
		} else {
			Suggesting = false;

			if ( LineEdit.Text.Find( " " ) != -1 ) {
				List<string> splitText = ParseLineInput( LineEdit.Text );
				if ( splitText.Count > 1 ) {
					string command = splitText[0];
					string paramInput = splitText[1];
					if ( CommandParameters.ContainsKey( command ) ) {
						string[] parameters = CommandParameters[ command ];
						for ( int i = 0; i < parameters.Length; i++ ) {
							if ( paramInput.Find( parameters[i] ) != -1 ) {
								Suggestions.Add( string.Format( "{0} {1}", command, parameters[i] ) );
							}
						}
					}
				}
			}
			else {
				List<string> commands = new List<string>( ConsoleCommands.Count );
				foreach ( var command in ConsoleCommands ) {
					commands.Add( command.ToString() );
				}
				commands.Sort();
				commands.Reverse();

				int prevIndex = 0;
				for ( int i = 0; i < commands.Count; i++ ) {
					if ( LineEdit.Text.Length == 0 || commands[i].Contains( LineEdit.Text ) ) {
						int index = commands[i].Find( LineEdit.Text );
						if ( index <= prevIndex ) {
							Suggestions.Insert( 0, commands[i] );
						} else {
							Suggestions.Add( commands[i] );
						}
						prevIndex = index;
					}
				}
			}
		}
	}
	private List<string> ParseLineInput( string text ) {
		List<string> result = new List<string>();
		bool inQuotes = false;
		bool escaped = false;
		string token = "";
		char c;

		for ( int i = 0; i < text.Length; i++ ) {
			c = text[i];
			if ( text[i] == '\\' ) {
				escaped = true;
				break;
			}
			else if ( escaped ) {
				switch ( text[i] ) {
				case 'n':
					c = '\n';
					break;
				case 't':
					c = '\t';
					break;
				case 'r':
					c = '\r';
					break;
				case 'a':
					c = '\a';
					break;
				case 'b':
					c = '\b';
					break;
				case 'f':
					c = '\f';
					break;
				};
				escaped = false;
			}
			else if ( c == '\"' ) {
				inQuotes = !inQuotes;
				continue;
			}
			else if ( c == ' ' || c == '\t' ) {
				if ( !inQuotes ) {
					result.Add( token );
					token = "";
					continue;
				}
			}
			token += c;
		}
		result.Add( token );
		return result;
	}
	private void OnTextEntered( string text ) {
		ScrollToBottom();
		ResetAutocomplete();
		LineEdit.Clear();
		if ( LineEdit.HasMethod( "edit" ) ) {
			LineEdit.CallDeferred( "edit" );
		}

		if ( text.StripEdges().Length > 0 ) {
			AddInputHistory( text );
			PrintLine( string.Format( "[i]> {0}[/i]", text ) );
			List<string> textSplit = ParseLineInput( text );
			string command = textSplit[0];

			if ( ConsoleCommands.TryGetValue( command, out ConsoleCommand consoleCommand ) ) {
				List<string> args = textSplit[1..];
				if ( command.Match( "calc" ) ) {
					string expression = "";
					for ( int i = 0; i < args.Count; i++ ) {
						expression += args[i];
					}
					consoleCommand.Function.Call( new Godot.Collections.Array{ expression } );
					return;
				}
				if ( args.Count > consoleCommand.RequiredArgs ) {
					PrintError( string.Format( "usage: {0} {1}", command, consoleCommand.Arguments.ToString() ) );
					return;
				} else if ( args.Count > consoleCommand.Arguments.Length ) {
					args.RemoveRange( consoleCommand.Arguments.Length, args.Count - consoleCommand.Arguments.Length );
				}

				while ( args.Count < consoleCommand.Arguments.Length ) {
					args.Add( "" );
				}

				Godot.Collections.Array<string> arguments = new Godot.Collections.Array<string>();
				arguments.Resize( args.Count );
				for ( int i = 0; i < args.Count; i++ ) {
					arguments[i] = args[i];
				}
				consoleCommand.Function.Call( arguments );
			}
			else {
				EmitSignal( "ConsoleUnknownCommand", text );
			}
		}
	}


	public static void PrintError( string text ) {
		RichLabel.CallDeferred( "append_text", string.Format( "[color=red]\tERROR:[/color] {0}", text ) );
		GD.PushError( text );
	}
	public static void PrintWarning( string text ) {
		RichLabel.CallDeferred( "append_text", string.Format( "[color=gold]\tWARNING:[/color] {0}", text ) );
		GD.PushWarning( text );
	}
	public static void PrintLine( string text ) {
		RichLabel.CallDeferred( "append_text", string.Format( "{0}\n", text ) );
		GD.Print( text );
	}

	private void Disable() {
		Enabled = false;
		ToggleConsole();
	}
	private void Enable() {
		Enabled = true;
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
	private void ToggleConsole() {
		if ( Enabled ) {
			Control.Visible = !Control.Visible;
		} else {
			Control.Visible = false;
		}

		if ( Control.Visible ) {
			WasPausedAlready = GetTree().Paused;
			GetTree().Paused = WasPausedAlready || PauseEnabled;
			LineEdit.GrabFocus();
			EmitSignal( "ConsoleOpened" );
		} else {
			Control.AnchorBottom = 1.0f;
			ScrollToBottom();
			ResetAutocomplete();
			if ( PauseEnabled && !WasPausedAlready ) {
				GetTree().Paused = false;
			}
			EmitSignal( "ConsoleClosed" );
		}
	}
	public bool IsVisible() {
		return Control.Visible;
	}
	public static void ScrollToBottom() {
		ScrollBar scroll = RichLabel.GetVScrollBar();
		scroll.Value = scroll.MaxValue - scroll.Page;
	}

	private void OnLineEditTextChanged( string newText ) {
		ResetAutocomplete();
	}
	private void Quit() {
		GetTree().Quit();
	}
	private static void Clear() {
		RichLabel.Clear();
	}
	private static void DeleteHistory() {
		ConsoleHistory.Clear();
		ConsoleHistoryIndex = 0;
		DirAccess.RemoveAbsolute( "user://history.txt") ;
	}
	private static void PrintHelp() {
		RichLabel.AppendText(
		@"    builtin commands:
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
			[color=light_blue]Tab[/color] key to autocomplete, [color=light_blue]Tab[/color] again to cycle between matching suggestions
			
			"
		);
	}
	private static void Calculate( string command ) {
		Expression expression = new Expression();
		Error error = expression.Parse( command );
		if ( error != Error.Ok ) {
			PrintError( string.Format( expression.GetErrorText() ) );
			return;
		}
		Godot.Variant result = expression.Execute();
		if ( !expression.HasExecuteFailed() ) {
			PrintLine( result.ToString() );
		} else {
			PrintError( expression.GetErrorText() );
		}
	}
	private static void Commands() {
		List<string> commands = new List<string>( ConsoleCommands.Count );
		foreach ( var command in ConsoleCommands ) {
			commands.Add( command.ToString() );
		}
		commands.Sort();

		RichLabel.AppendText( string.Format( "\t{0}\n\n", commands.ToString() ) );
	}
	private void ListCommands() {
		List<string> commands = new List<string>( ConsoleCommands.Count );
		foreach ( var command in ConsoleCommands ) {
			commands.Add( command.ToString() );
		}
		commands.Sort();

		for ( int i = 0; i < commands.Count; i++ ) {
			string args = "";
			string description = ConsoleCommands[ commands[i] ].Description;
			for ( int a = 0; a < ConsoleCommands[ commands[i] ].Arguments.Length; i++ ) {
				if ( a < ConsoleCommands[ commands[i] ].RequiredArgs ) {
					args = "\t[color=cornflower_blue]<" + ConsoleCommands[ commands[i] ].Arguments[a] + ">[/color]";
				} else {
					args = "\t<" + ConsoleCommands[ commands[i] ].Arguments[a] + ">";
				}
			}
			RichLabel.AppendText( string.Format( "\t[color=light_green]{0}[/color][color=gray]{1}[/color]:   {2}\n", commands[i], args, description ) );
		}
		RichLabel.AppendText( "\n" );
	}
	private static void AddInputHistory( string text ) {
		if ( ConsoleHistory.Count == 0 || text != ConsoleHistory.Last() ) {
			ConsoleHistory.Add( text );
		}
		ConsoleHistoryIndex = ConsoleHistory.Count;
	}
	
	private void Pause() {
		GetTree().Paused = true;
	}
	private void Unpause() {
		GetTree().Paused = false;
	}
	private void Exec( string filename ) {
		string path = "user://" + filename;
		FileAccess script = FileAccess.Open( path, FileAccess.ModeFlags.Read );
		if ( script != null ) {
			while ( !script.EofReached() ) {
				OnTextEntered( script.GetLine() );
			}
		} else {
			PrintError( "Script " + filename + " not found." );
		}
	}
};
*/