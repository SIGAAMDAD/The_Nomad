/*
using Steamworks;
using Godot;
using System.Collections.Generic;

public partial class SteamInput : Node {
	private static int InputDeviceCount;
	private static InputHandle_t[] InputHandles;

	private static Node GUIDE;

	private static InputActionSetHandle_t MenuActionSet;
	private static InputAnalogActionHandle_t MenuUpAnalogAction;
	private static InputAnalogActionHandle_t MenuDownAnalogAction;
	private static InputAnalogActionHandle_t MoveAction;

	private static Callback<SteamInputDeviceConnected_t> SteamInputDeviceConnected;
	private static Callback<SteamInputDeviceDisconnected_t> SteamInputDeviceDisconnected;
	private static Callback<SteamInputConfigurationLoaded_t> SteamInputConfigurationLoaded;
	private static Callback<SteamInputGamepadSlotChange_t> SteamInputGamepadSlotChange;

	private static void OnSteamInputDeviceConnected( SteamInputDeviceConnected_t pCallback ) {
		InputHandles[ InputDeviceCount ] = pCallback.m_ulConnectedDeviceHandle;

		string deviceType = "";
		switch ( Steamworks.SteamInput.GetInputTypeForHandle( InputHandles[ InputDeviceCount ] ) ) {
		case ESteamInputType.k_ESteamInputType_Unknown:
			Console.PrintError( string.Format( "[STEAM] Unknown input device at handle index {0}!", InputDeviceCount ) );
			return;
		case ESteamInputType.k_ESteamInputType_GenericGamepad:
			deviceType = "Generic Gamepad";
			break;
		case ESteamInputType.k_ESteamInputType_PS3Controller:
			deviceType = "PS3 Controller";
			break;
		case ESteamInputType.k_ESteamInputType_PS4Controller:
			deviceType = "PS4 Controller";
			break;
		case ESteamInputType.k_ESteamInputType_PS5Controller:
			deviceType = "PS5 Controller";
			break;
		case ESteamInputType.k_ESteamInputType_XBoxOneController:
			deviceType = "XBox One Controller";
			break;
		case ESteamInputType.k_ESteamInputType_XBox360Controller:
			deviceType = "XBox 360 Controller";
			break;
		case ESteamInputType.k_ESteamInputType_SteamDeckController:
			deviceType = "SteamDeck Controller";
			break;
		}
		;

		Steamworks.SteamInput.ActivateActionSet( InputHandles[ InputDeviceCount ], MenuActionSet );

		Console.PrintLine( string.Format( "[STEAM] SteamInput device {0} connected.", deviceType ) );

		InputDeviceCount++;
	}
	private static void OnSteamInputDeviceDisconnected( SteamInputDeviceDisconnected_t pCallback ) {
		InputDeviceCount--;
	}
	private static void OnSteamInputConfigurationLoaded( SteamInputConfigurationLoaded_t pCallback ) {
	}
	private static void OnSteamInputGamepadSlotChange( SteamInputGamepadSlotChange_t pCallback ) {
	}

	public override void _Ready() {
		base._Ready();

		bool bInitialized = Steamworks.SteamInput.Init( false );
		InputHandles = new InputHandle_t[ 4 ];

		if ( bInitialized ) {
			GD.Print( "[STEAM] Initialized SteamInput API" );
			Steamworks.SteamInput.EnableDeviceCallbacks();
		} else {
			GD.PrintErr( "[STEAM] Error initializing SteamInput API" );
		}

		SteamInputDeviceConnected = Callback<SteamInputDeviceConnected_t>.Create( OnSteamInputDeviceConnected );
		SteamInputDeviceDisconnected = Callback<SteamInputDeviceDisconnected_t>.Create( OnSteamInputDeviceDisconnected );
		SteamInputConfigurationLoaded = Callback<SteamInputConfigurationLoaded_t>.Create( OnSteamInputConfigurationLoaded );
		SteamInputGamepadSlotChange = Callback<SteamInputGamepadSlotChange_t>.Create( OnSteamInputGamepadSlotChange );

		MenuActionSet = Steamworks.SteamInput.GetActionSetHandle( "Menu" );

		MenuUpAnalogAction = Steamworks.SteamInput.GetAnalogActionHandle( "MenuUp" );
		MenuDownAnalogAction = Steamworks.SteamInput.GetAnalogActionHandle( "MenuDown" );
		MoveAction = Steamworks.SteamInput.GetAnalogActionHandle( "Move" );

		GUIDE = GetNode( "/root/GUIDE" );
	}
	public override void _ExitTree() {
		base._ExitTree();

		Steamworks.SteamInput.Shutdown();
	}

	public override void _Process( double delta ) {
		base._Process( delta );

		InputAnalogActionData_t actionData = Steamworks.SteamInput.GetAnalogActionData( InputHandles[ 0 ], MenuUpAnalogAction );
		if ( actionData.bActive == 1 ) {
			GD.Print( "ACTIVE!" );
		}

		switch ( GetTree().CurrentScene.Name ) {
		case "MainMenu":
			break;
		default:
			break;
		}
		;
	}

	private InputEvent BuildInputEvent( Vector2 value ) {
		InputEventJoypadMotion joypadMotion = new InputEventJoypadMotion();
		joypadMotion.AxisValue ;
	}
	private void MapSteamInput( InputHandle_t controller ) {
		InputAnalogActionData_t moveData = Steamworks.SteamInput.GetAnalogActionData( controller, MoveAction );
		if ( moveData.bActive == 1 ) {
			Vector2 moveVector = new Vector2( moveData.x, moveData.y );
			moveData.eMode = EInputSourceMode.k_EInputSourceMode_Joystck
			GUIDE.Call( "inject_input",  );
		}
	}
};
*/