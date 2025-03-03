/*
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Console : Node {
	private static Node Instance;


	private bool Enabled = true;
	private bool PauseEnabled = false;

	[Signal]
	public delegate void ConsoleOpenedEventHandler();
	[Signal]
	public delegate void ConsoleClosedEventHandler();
	[Signal]
	public delegate void ConsoleUnknownCommandEventHandler();

	private class ConsoleCommand {
		public Action<List<string>> Function;
		public List<string> Arguments;
		public int Required;
		public string Description;
		public bool Hidden;

		public ConsoleCommand( Action<List<string>> fn, List<string> args, int nRequired, string description ) {
			Function = fn;
			Arguments = args;
			Required = nRequired;
			Description = description;
		}
	};

	private Control Control = new Control();

	public RichTextLabel RichLabel = new RichTextLabel();
	public LineEdit LineEdit = new LineEdit();

	private Dictionary<string, ConsoleCommand> ConsoleCommands = new Dictionary<string, ConsoleCommand>();
	private Dictionary<string, List<string>> CommandParameters = new Dictionary<string, List<string>>();
	private List<string> ConsoleHistory = new List<string>();
	private int ConsoleHistoryIndex = 0;
	private bool WasPausedAlready = false;

	public void AddCommand( string name, Action<List<string>> fn, List<Godot.Variant> args, int required, string description = "" ) {
		List<string> strArgs = new List<string>();
		for ( int i = 0; i < args.Count; i++ ) {
			strArgs.Add( args[i].ToString() );
		}
		ConsoleCommands.Add( name, new ConsoleCommand( fn, strArgs, required, description ) );
	}
	public void RemoveCommand( string name ) {
		ConsoleCommands.Remove( name );
		CommandParameters.Remove( name );
	}
	public void AddCommandAutocomplete( string name, List<string> paramList ) {
		CommandParameters.Add( name, paramList );
	}

	public override void _EnterTree() {
		base._EnterTree();

		System.IO.StreamReader file = new System.IO.StreamReader(
			ProjectSettings.GlobalizePath( "user://history.txt" )
		);
		
		if ( file != null ) {
			while ( !file.EndOfStream ) {
//				AddInputHistory( file.ReadLine() );
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
		RichLabel.AppendText( "[DEVELOPER CONSOLE]\n" );
		Control.AddChild( RichLabel );
		
		LineEdit.AnchorTop = 0.5f;
		LineEdit.AnchorRight = 1.0f;
		LineEdit.AnchorBottom = 0.5f;
		LineEdit.PlaceholderText = "> ";
		LineEdit.Connect( "text_submitted", Callable.From<string>( OnTextEntered ) );
		LineEdit.Connect( "text_changed", Callable.From( OnLineEditTextChanged ) );
		Control.AddChild( LineEdit );
	}

	public override void _ExitTree() {
		base._ExitTree();

		System.IO.StreamWriter file = new System.IO.StreamWriter(
			ProjectSettings.GlobalizePath( "user://history.txt" )
		);
		if ( file != null ) {
			for ( int i = 0; i < ConsoleHistory.Count; i++ ) {
				file.WriteLine( ConsoleHistory[i] );
			}
		}
	}

	public override void _Ready() {
		base._Ready();

		AddCommand( "quit", Callable.From( Quit ), null, 0, "Exit application." );
		AddCommand( "exit", Callable.From( Quit ), null, 0, "Exit application." );
	}

	public override void _Input( InputEvent @event ) {
		base._Input( @event );

		if ( @event is InputEventKey ) {
			InputEventKey keyEvent = (InputEventKey)@event;
			if ( keyEvent.GetPhysicalKeycodeWithModifiers() == Key.Quoteleft ) {
				if ( keyEvent.Pressed ) {
					Toggle();
				}
			}
		}
	}

	private List<string> Suggestions;
	private int CurrentSuggest = 0;
	private bool Suggesting = false;

	public void Autocomplete() {
		if ( Suggesting ) {
			for ( int i = 0; i < Suggestions.Count; i++ ) {
				if ( CurrentSuggest == i ) {
					LineEdit.Text = Suggestions[i].ToString();
					LineEdit.CaretColumn = LineEdit.Text.Length;
					if ( CurrentSuggest == Suggestions.Count - 1 ) {
						CurrentSuggest = 0;
					} else {
						CurrentSuggest++;
					}
					return;
				}
			}
		} else {
			Suggesting = true;

			if ( LineEdit.Text.Find( " " ) != -1 ) {
				string[] splitText = ParseLineInput( LineEdit.Text );
			}
		}
	}
	private string[] ParseLineInput( string text ) {
		string[] outArray = new string[ 64 ];
		bool inQuotes = false;
		bool escaped = false;
		string token = "";

		for ( int i = 0; i < text.Length; i++ ) {
			char tmp = text[i];
			if ( text[i] == '\\' ) {
				escaped = true;
				continue;
			} else if ( escaped ) {
				switch ( text[i] ) {
				case 'n':
					tmp = '\n';
					break;
				case 't':
					tmp = '\t';
					break;
				case 'r':
					tmp = '\r';
					break;
				case 'a':
					tmp = '\a';
					break;
				case 'b':
					tmp = '\b';
					break;
				case 'f':
					tmp = '\f';
					break;
				};
				escaped = false;
			} else if ( text[i] == '\"' ) {
				inQuotes = !inQuotes;
				continue;
			} else if ( text[i] == ' ' || text[i] == '\t' ) {
				if ( !inQuotes ) {
					outArray.Append( token );
					token = "";
				}
			}
			token += tmp;
		}
		outArray.Append( token );
		return outArray;
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
			PrintLine( "[i]> " + text + "[/i]" );
			
			string[] textSplit = ParseLineInput( text );
			string command = textSplit.First();

			if ( ConsoleCommands.Keys.Contains( command ) ) {
				List<string> arguments = (List<string>)textSplit.Skip( 1 ).Take( textSplit.Length - 2 );
				ConsoleCommand cmd = ConsoleCommands[ command ];

				if ( command.Match( "calc" ) ) {
					string expression = "";
					foreach ( var word in arguments ) {
						expression += word;
					}
					cmd.Function.Call( (Godot.Variant)expression );
					return;
				}

				if ( arguments.Count < cmd.Required ) {
					PrintLine( cmd.Description );
					return;
				}

				while ( arguments.Count < cmd.Arguments.Count ) {
					arguments.Append( "" );
				}

				cmd.Function.Call( (Godot.Variant)arguments );
			}
		}
	}

	public bool IsVisbile() {
		return Control.Visible;
	}
	public void ScrollToBottom() {
		ScrollBar scroll = RichLabel.GetVScrollBar();
		scroll.Value = scroll.MaxValue - scroll.Page;
	}
	public void ResetAutocomplete() {
		Suggestions.Clear();
		CurrentSuggest = 0;
		Suggesting = false;
	}
	public void ToggleSize() {
		if ( Control.AnchorBottom == 1.0f ) {
			Control.AnchorBottom = 1.9f;
		} else {
			Control.AnchorBottom = 1.0f;
		}
	}
	public void Disable() {
		Enabled = false;
		Toggle();
	}
	public void Enable() {
		Enabled = true;
	}
	public void Toggle() {
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

	private void AddInputHistory( string text ) {
		if ( ConsoleHistory.Count == 0 || text != ConsoleHistory.Last() ) {
			ConsoleHistory.Add( text );
		}
		ConsoleHistoryIndex = ConsoleHistory.Count;
	}
};
*/