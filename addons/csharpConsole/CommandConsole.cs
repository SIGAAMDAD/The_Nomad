using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

public partial class CommandConsole : Node
{
	public event Action console_opened;
	public event Action console_closed;
	public event Action console_unknown_command;

	static CommandConsole instance;

	class Command
	{
		public bool base_command = false;
		public Callable function;
		public int param_count;
		public Dictionary<string,string> Params = new Dictionary<string, string>();
		public string Description = "Description Not Definned";

		public Command(Callable in_function, int in_param_count)
		{
			this.function = in_function;
			this.param_count = in_param_count;
		}
	}

	Control control;
	RichTextLabel rich_label;
	RichTextLabel syntaxLabel;
    LineEdit line_edit;

	Dictionary<string, Command> Commands = new Dictionary<string, Command>();
	List<string> console_history = new List<string>();
	int console_history_index = 0;


	List<string> Suggestions = new List<string>();
	int CurrentSuggestion = 0;
	bool Suggesting = false;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        #region Create

        instance = this;
		this.control = new Control();
		this.rich_label = new RichTextLabel();
		this.syntaxLabel = new RichTextLabel();
        this.line_edit = new LineEdit();


		StyleBoxFlat style = new StyleBoxFlat();
		StyleBoxFlat style2 = new StyleBoxFlat();
        style.BgColor = new Color(0, 0, 1, 0.5f);
		style2.BgColor = new Color(0, 0, 0, 0.5f);

		CanvasLayer canvas = new CanvasLayer();
		canvas.Layer = 3;
		AddChild(canvas);
		control.AnchorBottom = 1;
		control.AnchorRight = 1;
		canvas.AddChild(control);

		rich_label.ScrollFollowing = true;
		rich_label.AnchorRight = 1.0f;
		rich_label.AnchorBottom = 0.5f;
		rich_label.AddThemeStyleboxOverride("normal", style);
		control.AddChild(rich_label);

		rich_label.Text = "Development Console. \n";
		line_edit.PlaceholderText = "Enter \"help\" for instructions";
		line_edit.AnchorTop = 0.54f;
		line_edit.AnchorRight = 1;
		line_edit.AnchorBottom = 0.59f;

		syntaxLabel.AnchorTop = 0.5f;
		syntaxLabel.AnchorRight = 1.0f;
		syntaxLabel.AnchorBottom = 0.54f;

        syntaxLabel.AddThemeStyleboxOverride("normal",style2);
		control.AddChild(syntaxLabel);


        control.AddChild(line_edit);
		line_edit.TextSubmitted += OnTextSubmited;
		line_edit.TextChanged += OnTextChanged;
		control.Visible = false;
		this.ProcessMode = ProcessModeEnum.Always;
        #endregion

        #region add commands
        AddCommand("quit", Quit);
		Commands["quit"].base_command = true;
		AddCommandDescription("quit", "Quits the game.");
		AddCommand("exit", Quit);
		Commands["exit"].base_command = true;
        AddCommandDescription("exit", "Quits the game.");
		AddCommand("help", Help);
		Commands["help"].base_command = true;
		AddCommandDescription("help", "Shows a list of all the currently registered commands.");
		AddCommand("clear", clear);
		Commands["clear"].base_command = true;
		AddCommandDescription("clear", "Clears the current registry view.");
		AddCommand("delete_history", DeleteHistory);
		Commands["delete_history"].base_command = true;
		AddCommandDescription("delete_history", "Deletes the commands history.");

		AddCommand("command_list", ShowExternalCommands);
		Commands["command_list"].base_command = true;
		AddCommandDescription("command_list", "Shows a list of all the currently registered commands.");

		GetCommandsWithAttribute();
        #endregion
    }
    #region base Command Console
    private void OnTextChanged(string newText)
	{
		ShowDescription(newText);
		ResetAutoComplete();

    }

	void OnTextSubmited(string text)
	{
		ScrollToBottom();
		ResetAutoComplete();
		line_edit.Clear();
		syntaxLabel.Clear();
		AddInputHistory(text);
		PrintLine(text);

		//string[] splitText = text.Split(' ');
		string[] splitText = SplitConsideringQuotes(text);
		
		if (splitText.Length > 0 )
		{
			string commandString = splitText[0].ToLower();
			
			if (Commands.ContainsKey(commandString))
			{
				Command commandEntry = Commands[commandString];
				
				switch (commandEntry.param_count)
				{
					case 0:
						commandEntry.function.Call();
						break;
					case > 0:
						List<Variant> InGameparams_ = new List<Variant>();

						for (int i = 1; i < splitText.Length; i++)
						{
                            InGameparams_.Add(splitText[i]);
						}

						//verify the ammount of params
						if(InGameparams_.Count < commandEntry.Params.Count)
						{
                            PrintLine("Not enough parameters.");
                            break;
                        }
						if(InGameparams_.Count > commandEntry.Params.Count)
						{
                            PrintLine("too much parameters.");
                            break;
                        }

						commandEntry.function.Call(InGameparams_.ToArray());
						break;
				}
			}
			else
			{
				console_unknown_command?.Invoke();
				PrintLine("Command not found.");

			}
		}
	}

    string[] SplitConsideringQuotes(string input)
    {
        var matches = Regex.Matches(input, @"""[^""]+""|\S+");
        var parts = new string[matches.Count];

        for (int i = 0; i < matches.Count; i++)
        {
            parts[i] = matches[i].Value.Trim('"'); 
        }

        return parts;
    }
   
    public override void _Input(InputEvent @event)
	{
		if (@event is InputEventKey eventKey)
		{
            // ~ key
            if (eventKey.Keycode == Key.Quoteleft)
			{
				if (eventKey.Pressed)
				{
					ToggleConsole();
				}
				GetTree().Root.SetInputAsHandled();
			}
			//toggle console size
			else if (eventKey.Keycode == Key.Quoteleft && eventKey.IsCommandOrControlPressed())
			{
				if (eventKey.Pressed)
				{
					if (control.Visible)
						ToggleSize();
					else
					{
						ToggleConsole();
						ToggleSize();
					}
					GetTree().Root.SetInputAsHandled();
				}
			}
			//disable control on ESC
			else if (eventKey.Keycode == Key.Escape && control.Visible)
			{
				if (eventKey.Pressed)
					ToggleConsole();
				GetTree().Root.SetInputAsHandled();
			}

			if (control.Visible && eventKey.Pressed)
			{
				if (eventKey.GetKeycodeWithModifiers() == Key.Up)
				{
                    GetTree().Root.SetInputAsHandled();
                    if (console_history_index > 0)
                    {
                        console_history_index--;
                        if (console_history_index >= 0)
                        {
                            line_edit.Text = console_history[console_history_index];
                            line_edit.CaretColumn = line_edit.Text.Length;
                            ResetAutoComplete();
                        }
                    }
                }

				if (eventKey.GetKeycodeWithModifiers() == Key.Down)
				{
                    GetTree().Root.SetInputAsHandled();
					if(console_history_index < console_history.Count)
					{
						console_history_index++;
						if(console_history_index < console_history.Count)
						{
                            line_edit.Text = console_history[console_history_index];
                            line_edit.CaretColumn = line_edit.Text.Length;
                            ResetAutoComplete();
                        }
                        else
						{
                            line_edit.Clear();
                            ResetAutoComplete();
                        }
					}
                }

                if (eventKey.Keycode == Key.Pageup)
				{
					VScrollBar scroll = rich_label.GetVScrollBar();
					scroll.Value -= scroll.Page - scroll.Page * 0.1;
					GetTree().Root.SetInputAsHandled();
				}
				if (eventKey.Keycode == Key.Pagedown)
				{
					VScrollBar scroll = rich_label.GetVScrollBar();
					scroll.Value += scroll.Page - scroll.Page * 0.1;
					GetTree().Root.SetInputAsHandled();
				}
				if (eventKey.Keycode == Key.Tab)
				{
					AutoComplete();
					GetTree().Root.SetInputAsHandled();
				}
			}
		}
	}

	void ToggleConsole()
	{
		control.Visible = !control.Visible;
		if (control.Visible)
		{
			//GetTree().Paused = true;
			line_edit.GrabFocus();
			console_opened?.Invoke();
		}
		else
		{
			control.AnchorBottom = 1f;
			//GetTree().Paused = false;
			ScrollToBottom();
			ResetAutoComplete();
			console_closed?.Invoke();
		}
	}
	void ToggleSize()
	{
		if (control.AnchorBottom == 1.0f)
		{
			control.AnchorBottom = 1.9f;
		}
		else
		{
			control.AnchorBottom = 1.0f;
		}
	}
	void AutoComplete()
	{
		if (Suggesting)
		{
			for (int i = 0; i < Suggestions.Count; i++)
			{
				if (CurrentSuggestion == i)
				{
					line_edit.Text = Suggestions[i];
					line_edit.CaretColumn = line_edit.Text.Length;
					if (CurrentSuggestion == Suggestions.Count -1)
					{
						CurrentSuggestion = 0;
					}
					else
					{
						CurrentSuggestion++;
					}
					return;
				}
			}
		}
		else
		{
			Suggesting = true;
			List<string> commands = new List<string>();
			foreach (var command in Commands)
			{
				commands.Add(command.Key);
			}
			commands.Sort();
			commands.Reverse();

			int PrevIndex = 0;
			foreach (var command in commands)
			{
				if (command.Contains(line_edit.Text))
				{
					int index = command.Find(line_edit.Text);
					if (index <= PrevIndex)
					{
						Suggestions.Insert(0, command);
					}
					else
					{
						Suggestions.Add(command);
					}
					PrevIndex = index;
				}
			}
			AutoComplete();

		}
	}

	void ScrollToBottom()
	{
		ScrollBar scroll = rich_label.GetVScrollBar();
		scroll.Value = scroll.MaxValue - scroll.Page;
	}
	void ResetAutoComplete()
	{
		this.Suggestions.Clear();
		CurrentSuggestion = 0;
		Suggesting = false;
	}

	void ShowDescription(string input)
	{
        syntaxLabel.Clear();
        var matches = Regex.Matches(input, @"""[^""]+""|\S+");
		string _function = "";
		string _params = "";

        foreach (var match in matches)
		{
			foreach (var command in Commands.Keys)
			{
				if(command == match.ToString())
				{
					_function = command;
					
					foreach(string _param in Commands[command].Params.Keys) 
					{
						_params += " [color=yellow]" + _param + "[/color]"; 

					}
                }
			}
		}
		
		syntaxLabel.AppendText(_function);
		syntaxLabel.AppendText(_params);
    }

	public static void PrintLine(string text)
	{
		if (instance.rich_label == null)
		{
			instance.CallDeferred("PrintLine", text, true);
		}
		else
		{
			instance.rich_label.AddText(text + "\n");
		}
	}

	void AddInputHistory(string text)
	{
		if (console_history.Count == 0 || text != console_history.Last())
		{
			console_history.Add(text);
		}
		console_history_index = console_history.Count;
	}

    void RemoveCommand(string CommandName)
	{
        Commands.Remove(CommandName);
    }

	public void Quit()
	{
		GetTree().Quit();
	}
	
	void clear()
	{
		rich_label.Clear();
	}

	void DeleteHistory()
	{
		console_history.Clear();
		console_history_index = 0;
		DirAccess.RemoveAbsolute(GetPath()+"/Commands.log");
	}
	void Help()
	{
		rich_label.AddText("Commands:\n");
        foreach (var command in Commands.OrderBy(d => d.Key))
        {
			if(command.Value.base_command)
				rich_label.AppendText(string.Format("\t[color=light_green]{0}[/color]: {1}\n", command.Key, command.Value.Description));
        }
        rich_label.AppendText("" +
            "\r\n\t\t[color=light_blue]Up[/color] and [color=light_blue]Down[/color] arrow keys to navigate commands history" +
            "\r\n\t\t[color=light_blue]PageUp[/color] and [color=light_blue]PageDown[/color] to navigate registry history" +
            "\r\n\t\t[color=light_blue]Ctr+Tilde[/color] to change console size between half screen and full creen" +
            "\r\n\t\t[color=light_blue]Tilde[/color] or [color=light_blue]Esc[/color] to close the console" +
            "\r\n\t\t[color=light_blue]Tab[/color] for basic autocomplete\n");
	}

	void ShowExternalCommands()
	{
        foreach (var command in Commands.OrderBy(d => d.Key))
        {
            if (!command.Value.base_command)
                rich_label.AppendText(string.Format("\t[color=light_green]{0}[/color]: {1}\n", command.Key, command.Value.Description));
        }
    }

    #endregion

    /// <summary>
    /// add a command to the console ingame, active console with ~ button.
    /// <code>
    /// CommandConsole.AddCommand("testing", test);
    /// </code>
    /// any opinion is accepted.
    /// </summary>
    /// <param name="CommandName">name of the function called in game console</param>
    /// <param name="function">reference to the method</param>
    public static void AddCommand(string CommandName, Delegate function)
    {
        try
        {
			
			instance.Commands.Add(CommandName, new Command(new Callable((GodotObject)function.Target, function.Method.Name), function.Method.GetParameters().Length));
			
			foreach (var param in function.Method.GetParameters())
            {
                instance.Commands[CommandName].Params.Add(param.Name, null);
            }
        }
        catch (Exception e)
        {
            GD.PrintErr(e);
        }
    }

    /// <summary>
	/// work in progress
	/// </summary>
	/// <param name="CommandName"></param>
	/// <param name="param"></param>
	/// <param name="description"></param>
    public static void AddParameterDescription(string CommandName, string param, string description)
    {
        try
        {
            instance.Commands[CommandName].Params[param] = description;
        }
        catch (Exception e)
        {
            GD.PrintErr(e);
        }
    }

	/// <summary>
	/// shows a description of the command in the console with commandlist
	/// </summary>
	/// <param name="CommandName"></param>
	/// <param name="description"></param>
    public static void AddCommandDescription(string CommandName, string description)
    {
        try
        {
            instance.Commands[CommandName].Description = description;
        }
        catch (Exception e)
        {
            GD.PrintErr(e);
        }
    }

    void GetCommandsWithAttribute()
    {
        var Methods =
            Assembly.GetExecutingAssembly()
            .GetTypes().
            SelectMany(x => x.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            ;


        foreach (var Method in Methods)
        {
            var commandAttributes = Method.GetCustomAttributes(typeof(AddCommandAttribute), false);
            AddCommandDescriptionAttribute commandDescriptionAttribute = (AddCommandDescriptionAttribute)Method.GetCustomAttribute(typeof(AddCommandDescriptionAttribute), false);

            foreach (AddCommandAttribute att in commandAttributes)
            {
                try
                {
                    Delegate delegateInstance = null;

                    if (Method.IsStatic)
                    {
                        
                        return;
                        /*
						delegateInstance = Delegate.CreateDelegate(
							System.Linq.Expressions.Expression.GetActionType(
								Method.GetParameters().Select(p => p.ParameterType).ToArray()),
							null,
							Method);*/
                    }

                    else
                    {
                        var targetType = Method.DeclaringType;
                        var targetInstance = Activator.CreateInstance(targetType); // This assumes the type has a parameterless constructor
                        delegateInstance = Delegate.CreateDelegate(
                            System.Linq.Expressions.Expression.GetActionType(
                                Method.GetParameters().Select(p => p.ParameterType).ToArray()),
                            targetInstance,
                            Method);
                    }

                    AddCommand(att.CommandName, delegateInstance);

                    if (commandDescriptionAttribute != null)
                    {
                        AddCommandDescription(att.CommandName, commandDescriptionAttribute.Description);
                    }
                }
                catch (Exception ex)
                {
                    GD.PrintErr(ex.ToString());
                }
            }
        }
    }
}
