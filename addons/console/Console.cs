/*
===========================================================================
The Nomad AGPL Source Code
Copyright (C) 2025 Noah Van Til

The Nomad Source Code is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published
by the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

The Nomad Source Code is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with The Nomad Source Code.  If not, see <http://www.gnu.org/licenses/>.

If you have questions concerning this license or the applicable additional
terms, you may contact me via email at nyvantil@gmail.com.
===========================================================================
*/

using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

/*
===================================================================================

Console

===================================================================================
*/
/// <summary>
/// The main logger for the game. Use to convey information both boring and important to the
/// terminal (tty), godot console, the logfile, and the in-game console
/// </summary>
/// <remarks>
/// This implementation is currently threadsafe
/// </remarks>

public partial class Console : Control {
	public struct ConsoleCommand {
		public readonly Callable Function;
		public readonly string[]? Arguments = null;
		public readonly int Required = 0;
		public readonly string? Description = null;
		public readonly bool Hidden = false;

		public ConsoleCommand( Callable function, string[]? arguments, int required = 0, string? description = "" ) {
			Function = function;
			Arguments = arguments;
			Required = required;
			Description = description;
		}
	};

	private static readonly string LOG_FILE = "user://debug.log";
	private static readonly string CONSOLE_HISTORY_FILE = "user://history.txt";

	[Export]
	public bool Enabled { get; private set; } = true;

	[Export]
	public bool EnableOnReleaseBuild { get; private set; } = true;

	[Export]
	public bool PauseEnabled { get; private set; } = false;

	private Dictionary<string, ConsoleCommand> ConsoleCommands = new Dictionary<string, ConsoleCommand>();
	private Dictionary<string, string[]> CommandParameters = new Dictionary<string, string[]>();
	private List<string> ConsoleHistory = new List<string>();
	private int ConsoleHistoryIndex = 0;
	private bool WasPausedAlready = false;
	private readonly object LockObject = new object();

	private List<string> Suggestions = new List<string>();
	private int CurrentSuggest = 0;
	private bool Suggesting = false;

	private static RichTextLabel RichLabel;
	private LineEdit LineEdit;
	public static Control Control;
	private CanvasLayer CanvasLayer;

	private System.IO.StreamWriter LogFileHandle;

	private static Console Instance;

	[Signal]
	public delegate void ConsoleOpenedEventHandler();
	[Signal]
	public delegate void ConsoleClosedEventHandler();
	[Signal]
	public delegate void ConsoleUnknownCommandEventHandler( string command );

	/*
	===============
	SetEnableOnReleaseBuild
	===============
	*/
	public static void SetEnableOnReleaseBuild( bool enable ) {
		Instance.EnableOnReleaseBuild = enable;
		if ( !Instance.EnableOnReleaseBuild && !OS.IsDebugBuild() ) {
			Instance.Disable();
		}
	}

	/*
	===============
	InitializeCommands
	===============
	*/
	/// <summary>
	/// Creates and initializes the default commands of the console
	/// </summary>
	private void InitializeCommands() {
		AddCommand( "quit", new Callable( this, MethodName.Quit ), [], 0, "Quits the game." );
		AddCommand( "exit", new Callable( this, MethodName.Quit ), [], 0, "Quits the game." );
		AddCommand( "clear", new Callable( this, MethodName.Clear ), [], 0, "Clears the text on the console." );
		AddCommand( "delete_history", new Callable( this, MethodName.DeleteHistory ), [], 0, "Deletes the history of previously entered commands." );
		AddCommand( "help", new Callable( this, MethodName.Help ), [], 0, "Displays instructions on how to use the console." );
		AddCommand( "commands_list", new Callable( this, MethodName.CommandsList ), [], 0, "Lists all commands and their descriptions." );
		AddCommand( "commands", new Callable( this, MethodName.Commands ), [], 0, "Lists commands with no descriptions." );
		AddCommand( "calc", new Callable( this, MethodName.Calculate ), [ "mathematical expression to evaluate" ], 0, "Evaluates the math passed in for quick arithmetic." );
		AddCommand( "echo", new Callable( this, MethodName.PrintLine ), [ "string" ], 1, "Prints given string to the console." );
		AddCommand( "echo_warning", new Callable( this, MethodName.PrintWarning ), [ "string" ], 1, "Prints given string as warning to the console." );
		AddCommand( "echo_debug", new Callable( this, MethodName.PrintDebug ), [ "string" ], 1, "Prints given string as debug to the console." );
		AddCommand( "echo_error", new Callable( this, MethodName.PrintError ), [ "string" ], 1, "Prints given string as an error to the console." );
		AddCommand( "pause", new Callable( this, MethodName.Pause ), [], 0, "Pauses node processing." );
		AddCommand( "unpause", new Callable( this, MethodName.Unpause ), [], 0, "Unpauses node processing." );
		AddCommand( "exec", new Callable( this, MethodName.Exec ), [ "filename" ], 1, "Execute a script." );
	}

	/*
	===============
	AddCommand
	===============
	*/
	/// <summary>
	/// Adds a command to the console command list
	/// </summary>
	/// <param name="commandName">Name of the command</param>
	/// <param name="function">The command function callback</param>
	/// <param name="arguments">The optional arguments that will be fed into function</param>
	/// <param name="required">The number of required arguments for the command</param>
	/// <param name="description">The description of the command and what it does</param>
	/// <exception cref="ArgumentException">Thrown if commandName is null or empty</exception>
	public static void AddCommand( string? commandName, Callable function, string[]? arguments = null, int required = 0, string? description = "" ) {
		ArgumentException.ThrowIfNullOrEmpty( commandName );

		arguments ??= [];
		Instance.ConsoleCommands.TryAdd( commandName, new ConsoleCommand( function, arguments, required, description ) );
	}

	public static void RemoveCommand( string commandName ) {
		Instance.ConsoleCommands.Remove( commandName );
		Instance.CommandParameters.Remove( commandName );
	}

	public static void AddCommandAutocompleteList( string commandName, string[] paramList ) {
		Instance.CommandParameters.TryAdd( commandName, paramList );
	}

	/*
	===============
	SaveHistory
	===============
	*/
	/// <summary>
	/// Saves the console history list to <see cref="CONSOLE_HISTORY_FILE"/>
	/// </summary>
	private void SaveHistory() {
		using var file = FileAccess.Open( CONSOLE_HISTORY_FILE, FileAccess.ModeFlags.Write );
		if ( file != null ) {
			int startIndex = Math.Max( 0, ConsoleHistory.Count - 100 ); // Max 100 lines
			for ( int i = startIndex; i < ConsoleHistory.Count; i++ ) {
				if ( !file.StoreLine( ConsoleHistory[ i ] ) ) {
					PrintError( $"Console.SaveHistory: error saving line at index {i} with data \"{ConsoleHistory[ i ]}\"" );

					// make sure we remove any corrupt data
					DirAccess.RemoveAbsolute( CONSOLE_HISTORY_FILE );
					break;
				}
			}
		} else {
			PrintError( $"Console.SaveHistory: couldn't save console history to {CONSOLE_HISTORY_FILE}" );
		}
	}

	/*
	===============
	HandleOpenConsole
	===============
	*/
	private void HandleToggleConsole( InputEventKey keyEvent ) {
		if ( keyEvent.GetPhysicalKeycodeWithModifiers() == Key.Quoteleft ) {
			if ( keyEvent.IsCommandOrControlPressed() ) {
				if ( Control.Visible ) {
					ToggleSize();
				} else {
					ToggleConsole();
					ToggleSize();
				}
				GetViewport().SetInputAsHandled();
			} else {
				ToggleConsole();
				GetViewport().SetInputAsHandled();
			}
		} else if ( keyEvent.GetPhysicalKeycodeWithModifiers() == Key.Escape && Control.Visible ) {
			ToggleConsole();
			GetViewport().SetInputAsHandled();
		}
	}

	/*
	===============
	OnConsoleHistoryPrev
	===============
	*/
	private void OnConsoleHistoryPrev() {
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

	/*
	===============
	OnConsoleHistoryNext
	===============
	*/
	private void OnConsoleHistoryNext() {
		GetViewport().SetInputAsHandled();
		if ( ConsoleHistoryIndex < ConsoleHistory.Count ) {
			ConsoleHistoryIndex++;
			if ( ConsoleHistoryIndex < ConsoleHistory.Count ) {
				LineEdit.Text = ConsoleHistory[ ConsoleHistoryIndex ];
				LineEdit.CaretColumn = LineEdit.Text.Length;
				ResetAutocomplete();
			} else {
				LineEdit.Text = "";
				ResetAutocomplete();
			}
		}
	}

	/*
	===============
	_Input
	===============
	*/
	/// <summary>
	/// Handles input for the console
	/// </summary>
	/// <param name="event"></param>
	public override void _Input( InputEvent @event ) {
		if ( @event is InputEventKey keyEvent && keyEvent != null && keyEvent.Pressed ) {
			// ~ or ESC key
			HandleToggleConsole( keyEvent );

			if ( Control.Visible ) {
				if ( keyEvent.GetPhysicalKeycodeWithModifiers() == Key.Up ) {
					OnConsoleHistoryPrev();
				} else if ( keyEvent.GetPhysicalKeycodeWithModifiers() == Key.Down ) {
					OnConsoleHistoryNext();
				} else if ( keyEvent.GetPhysicalKeycodeWithModifiers() == Key.Pageup ) {
					var scroll = RichLabel.GetVScrollBar();
					var tween = CreateTween();
					tween.TweenProperty( scroll, "value", scroll.Value - ( scroll.Page - scroll.Page * 0.1f ), 0.1f );
					GetViewport().SetInputAsHandled();
				} else if ( keyEvent.GetPhysicalKeycodeWithModifiers() == Key.Pagedown ) {
					var scroll = RichLabel.GetVScrollBar();
					var tween = CreateTween();
					tween.TweenProperty( scroll, "value", scroll.Value + ( scroll.Page - scroll.Page * 0.1f ), 0.1f );
					GetViewport().SetInputAsHandled();
				} else if ( keyEvent.GetPhysicalKeycodeWithModifiers() == Key.Tab ) {
					Autocomplete();
					GetViewport().SetInputAsHandled();
				}
			}
		}
	}

	/*
	===============
	Autocomplete
	===============
	*/
	/// <summary>
	/// I feel like this is self-explanatory
	/// </summary>
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
				var command = splitText[ 0 ];
				var paramInput = splitText[ 1 ];
				if ( CommandParameters.TryGetValue( command, out var parameters ) ) {
					for ( int i = 0; i < parameters.Length; i++ ) {
						if ( parameters[ i ].Contains( paramInput ) ) {
							Suggestions.Add( $"{command} {parameters[ i ]}" );
						}
					}
				}
			}
		} else {
			var sortedCommands = ConsoleCommands
				.Where( c => !c.Value.Hidden )
				.Select( c => c.Key )
				.OrderBy( c => c )
				.ToList();

			for ( int i = 0; i < sortedCommands.Count; i++ ) {
				if ( string.IsNullOrEmpty( LineEdit.Text ) || sortedCommands[ i ].Contains( LineEdit.Text ) ) {
					Suggestions.Add( sortedCommands[ i ] );
				}
			}
		}

		if ( Suggestions.Count > 0 ) {
			Autocomplete();
		} else {
			ResetAutocomplete();
		}
	}

	/*
	===============
	ResetAutocomplete
	===============
	*/
	/// <summary>
	/// Clears the autocomplete buffer
	/// </summary>
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	private void ResetAutocomplete() {
		Suggestions.Clear();
		CurrentSuggest = 0;
		Suggesting = false;
	}

	/*
	===============
	ToggleSize
	===============
	*/
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	private void ToggleSize() {
		if ( Control.AnchorBottom == 1.0f ) {
			Control.AnchorBottom = 1.9f;
		} else {
			Control.AnchorBottom = 1.0f;
		}
	}

	/*
	===============
	Disable
	===============
	*/
	/// <summary>
	/// Disables console processing
	/// </summary>
	public void Disable() {
		Enabled = false;
		ToggleConsole();
	}

	/*
	===============
	Enable
	===============
	*/
	/// <summary>
	/// Enables console processing
	/// </summary>
	public void Enable() {
		Enabled = true;
	}

	/*
	===============
	ToggleConsole
	===============
	*/
	/// <summary>
	/// Handles internal processing mode flip-flopping
	/// </summary>
	public void ToggleConsole() {
		if ( Enabled ) {
			Control.Visible = !Control.Visible;
		} else {
			Control.Visible = false;
		}

		if ( Control.Visible ) {
			WasPausedAlready = GetTree().Paused;
			GetTree().Paused = WasPausedAlready || PauseEnabled;
			LineEdit.GrabFocus();
			EmitSignalConsoleOpened();
		} else {
			Control.AnchorBottom = 1.0f;
			ScrollToBottom();
			ResetAutocomplete();
			if ( PauseEnabled && !WasPausedAlready ) {
				GetTree().Paused = false;
			}
			EmitSignalConsoleClosed();
		}
	}

	/*
	===============
	ScrollToBottom
	===============
	*/
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	private void ScrollToBottom() {
		var scroll = RichLabel.GetVScrollBar();
		scroll.Value = scroll.MaxValue - scroll.Page;
	}

	/*
	===============
	OnTextEntered
	===============
	*/
	private void OnTextEntered( string newText ) {
		ScrollToBottom();
		ResetAutocomplete();
		LineEdit.Clear();
		LineEdit.CallDeferred( LineEdit.MethodName.Clear );

		if ( newText.Trim().Length > 0 ) {
			AddInputHistory( newText );
			PrintLine( $"[i]> {newText}[/i]" );
			var textSplit = ParseLineInput( newText );
			var textCommand = textSplit.Length > 0 ? textSplit[ 0 ] : "";

			if ( !string.IsNullOrEmpty( textCommand ) && ConsoleCommands.TryGetValue( textCommand, out var consoleCommand ) && consoleCommand.Arguments != null ) {
				var arguments = textSplit.Skip( 1 ).ToArray();

				// calc is a special command that needs special treatment
				if ( textCommand == "calc" ) {
					var expression = string.Join( "", arguments );
					consoleCommand.Function.Delegate.DynamicInvoke( expression );
					SaveHistory();
					return;
				}

				if ( arguments.Length < consoleCommand.Required ) {
					PrintError( $"Too few arguments! Required < {consoleCommand.Required}>" );
					return;
				} else if ( arguments.Length > consoleCommand.Arguments.Length ) {
					arguments = arguments.Take( consoleCommand.Arguments.Length ).ToArray();
				}

				// fill out with empty strings if needed
				var finalArgs = arguments.Concat( Enumerable.Repeat( "", consoleCommand.Arguments.Length - arguments.Length ) ).ToArray();

				try {
					consoleCommand.Function.Delegate.DynamicInvoke( finalArgs );
					SaveHistory();
				} catch ( Exception ex ) {
					PrintError( $"Error executing command: {ex.Message}" );
				}
			} else if ( !string.IsNullOrEmpty( textCommand ) ) {
				EmitSignal( SignalName.ConsoleUnknownCommand, textCommand );
				PrintError( "Command not found." );
			}
		}
	}

	/*
	===============
	OnLineEditTextChanged
	===============
	*/
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	private void OnLineEditTextChanged( string newText ) {
		ResetAutocomplete();
	}

	/*
	===============
	ParseLineInput
	===============
	*/
	/// <summary>
	/// Parses console command line input string and returns a string list of individual commands
	/// </summary>
	/// <param name="text">The input string from the in-game console</param>
	/// <returns>The list of words/commands</returns>
	private string[] ParseLineInput( string text ) {
		List<string> result = new List<string>();
		StringBuilder current = new StringBuilder();
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

			if ( c == '\"' ) {
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

	/*
	===============
	AddInputHistory
	===============
	*/
	/// <summary>
	/// Adds a string of text to the existing console history buffer
	/// </summary>
	/// <param name="text">The string of text to add to the console history buffer</param>
	private void AddInputHistory( string text ) {
		ArgumentException.ThrowIfNullOrEmpty( text );

		if ( ConsoleHistory.Count == 0 || text != ConsoleHistory.Last() ) {
			ConsoleHistory.Add( text );
		}
		ConsoleHistoryIndex = ConsoleHistory.Count;
	}

	/*
	===============
	Quit
	===============
	*/
	/// <summary>
	/// Closes the game
	/// </summary>
	private void Quit() {
		GetTree().Quit();
	}

	/*
	===============
	Clear
	===============
	*/
	/// <summary>
	/// Clears the console text
	/// </summary>
	private void Clear() {
		RichLabel.Clear();
	}

	/*
	===============
	DeleteHistory
	===============
	*/
	/// <summary>
	/// Clears the console's history buffer, resets the index, and deletes the history file
	/// </summary>
	private void DeleteHistory() {
		ConsoleHistory.Clear();
		ConsoleHistoryIndex = 0;

		Error result = DirAccess.RemoveAbsolute( CONSOLE_HISTORY_FILE );
		if ( result != Error.Ok ) {
			PrintError( $"Error deleting console history file \"{CONSOLE_HISTORY_FILE}\" - godot error {result}" );
		}
	}

	/*
	===============
	Help
	===============
	*/
	/// <summary>
	/// Prints helper text about the builtin commands
	/// </summary>
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

	/*
	===============
	Calculate
	===============
	*/
	private void Calculate( string expression ) {
		Expression expr = new Expression();

		Error error = expr.Parse( expression );
		if ( error != Error.Ok ) {
			PrintError( $"{expr.GetErrorText()}" );
			return;
		}

		Variant result = expr.Execute();
		if ( !expr.HasExecuteFailed() ) {
			PrintLine( result.ToString() );
		} else {
			PrintError( $"{expr.GetErrorText()}" );
		}
	}

	/*
	===============
	Commands
	===============
	*/
	private void Commands() {
		var commands = ConsoleCommands
			.Where( c => !c.Value.Hidden )
			.Select( c => c.Key )
			.OrderBy( c => c )
			.ToList();

		RichLabel.AppendText( $" {string.Join( ", ", commands )}\n\n" );
	}

	/*
	===============
	CommandsList
	===============
	*/
	/// <summary>
	/// Lists all commands currently in the command cache
	/// </summary>
	private void CommandsList() {
		var commands = ConsoleCommands
			.Where( c => !c.Value.Hidden )
			.OrderBy( c => c.Key )
			.ToList();

		var argumentsString = new StringBuilder( 1024 );
		foreach ( var (command, cmdInfo) in commands ) {
			argumentsString.Clear();
			for ( int i = 0; i < cmdInfo.Arguments.Length; i++ ) {
				if ( i < cmdInfo.Required ) {
					argumentsString.Append( $"  [color=cornflower_blue]<{cmdInfo.Arguments[ i ]}>[/color]" );
				} else {
					argumentsString.Append( $"  <{cmdInfo.Arguments[ i ]}>" );
				}
			}
			RichLabel.AppendText( $"	[color=light_green]{command}[/color][color=gray]{argumentsString}[/color]:   {cmdInfo.Description}\n" );
		}
		RichLabel.AppendText( "\n" );
	}

	/*
	===============
	Pause
	===============
	*/
	/// <summary>
	/// Pauses the internal godot game loop
	/// </summary>
	private void Pause() {
		GetTree().Paused = true;
	}

	/*
	===============
	Unpause
	===============
	*/
	/// <summary>
	/// Unpauses the internal godot game loop
	/// </summary>
	private void Unpause() {
		GetTree().Paused = false;
	}

	/*
	===============
	Exec
	===============
	*/
	/// <summary>
	/// Executes the commands found in the given file
	/// </summary>
	/// <param name="filename">The path to the file</param>
	private void Exec( string filename ) {
		ArgumentException.ThrowIfNullOrEmpty( filename );

		string path = $"user://{filename}";

		using var file = FileAccess.Open( path, FileAccess.ModeFlags.Read );
		if ( file != null ) {
			while ( !file.EofReached() ) {
				OnTextEntered( file.GetLine() );
			}
		} else {
			PrintError( $"Error opening file at path {path}" );
		}
	}

	/*
	===============
	PrintLine
	===============
	*/
	/// <summary>
	/// Prints the provided text to the terminal, logfile, godot console, and the in-game console
	/// </summary>
	/// <param name="text">The text to output</param>
	public static void PrintLine( string text ) {
		if ( !IsInstanceValid( RichLabel ) ) {
			Instance.CallDeferred( MethodName.PrintLine, text, true );
		} else {
			RichLabel.CallDeferred( RichTextLabel.MethodName.AppendText, $"{text}\n" );
			GD.Print( text );
		}
		Instance.LogFileHandle?.WriteLine( text );
	}

	/*
	===============
	PrintError
	===============
	*/
	/// <summary>
	/// 
	/// </summary>
	/// <param name="text"></param>
	public static void PrintError( string text ) {
		if ( !IsInstanceValid( RichLabel ) ) {
			Instance.CallDeferred( MethodName.PrintLine, $"[color=light_coral]   ERROR:[/color] {text}", true );
		} else {
			RichLabel.CallDeferred( RichTextLabel.MethodName.AppendText, $"[color=light_coral]   ERROR:[/color] {text}\n" );
			GD.PushError( text );
		}
		Instance.LogFileHandle?.WriteLine( $"   ERROR: {text}" );
	}

	/*
	===============
	PrintDebug
	===============
	*/
	/// <summary>
	/// 
	/// </summary>
	/// <param name="text"></param>
	public static void PrintDebug( string text ) {
		if ( !IsInstanceValid( RichLabel ) ) {
			Instance.CallDeferred( MethodName.PrintLine, $"[color=light_blue]   DEBUG:[/color] {text}", true );
		} else {
			RichLabel.CallDeferred( RichTextLabel.MethodName.AppendText, $"[color=light_blue]   DEBUG:[/color] {text}" );
			GD.Print( text );
		}
		Instance.LogFileHandle?.WriteLine( $"   DEBUG: {text}" );
	}

	/*
	===============
	PrintWarning
	===============
	*/
	/// <summary>
	/// 
	/// </summary>
	/// <param name="text"></param>
	public static void PrintWarning( string text ) {
		if ( !IsInstanceValid( RichLabel ) ) {
			Instance.CallDeferred( MethodName.PrintLine, $"[color=gold]   WARNING:[/color] {text}", true );
		} else {
			RichLabel.CallDeferred( RichTextLabel.MethodName.AppendText, $"[color=gold]   WARNING:[/color] {text}\n" );
			GD.PushWarning( text );
		}
		Instance.LogFileHandle?.WriteLine( $"   WARNING: {text}" );
	}

	/*
	===============
	_EnterTree
	===============
	*/
	public override void _EnterTree() {
		Instance = this;

		CanvasLayer = new CanvasLayer();
		CanvasLayer.Layer = 3;
		AddChild( CanvasLayer );

		Control = new Control();
		Control.AnchorBottom = 1.0f;
		Control.AnchorRight = 1.0f;
		CanvasLayer.AddChild( Control );

		StyleBoxFlat style = new StyleBoxFlat();
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

		try {
			System.IO.FileStream stream = new System.IO.FileStream( ProjectSettings.GlobalizePath( LOG_FILE ), System.IO.FileMode.Create );
			LogFileHandle = new System.IO.StreamWriter( stream );
		} catch ( Exception e ) {
			GD.PushWarning( $"Couldn't create logfile {LOG_FILE}! Exception: {e.Message}" );
		}

		InitializeCommands();
	}

	/*
	===============
	_ExitTree
	===============
	*/
	public override void _ExitTree() {
		base._ExitTree();

		LogFileHandle.Close();
	}
};