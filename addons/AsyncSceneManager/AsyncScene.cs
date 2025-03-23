/*
using Godot;
using System.Collections.Generic;

public partial class AsyncScene : Node {
	[Signal]
	public delegate void OnCompleteEventHandler();

	public enum LoadingSceneOperation : uint {
		// Replaces the current scene immediately upon loading
		ReplaceImmediate,

		// Doesn't replace the scene immediately; call ChangeScene() to replace
		Replace,

		// Adds the new scene as a child to the root node immediately upon loading
		AdditiveImmediate,

		// Doesn't add the scene immediately; call ChangeScene() to add
		Additive
	};

	private static readonly Dictionary<ResourceLoader.ThreadLoadStatus, string> StatusNames = new Dictionary<ResourceLoader.ThreadLoadStatus, string>(){
		{ ResourceLoader.ThreadLoadStatus.InProgress, "THREAD_LOAD_IN_PROGRESS" },
		{ ResourceLoader.ThreadLoadStatus.Failed, "THREAD_LOAD_FAILED" },
		{ ResourceLoader.ThreadLoadStatus.InvalidResource, "THREAD_LOAD_INVALID_RESOURCE" },
		{ ResourceLoader.ThreadLoadStatus.Loaded, "THREAD_LOAD_LOADED" }
	};

	private Timer Timer = new Timer();
	private string PackedScenePath = "";
	private PackedScene MyResource = null;
	private Node CurrentSceneNode = null;
	private float Progress = 0.0f;
	private bool IsCompleted = false;
	private LoadingSceneOperation TypeOperation = LoadingSceneOperation.ReplaceImmediate;
	private bool Changed = false;

	public AsyncScene( string scenePath, LoadingSceneOperation setOperation = LoadingSceneOperation.ReplaceImmediate ) {
		PackedScenePath = scenePath;
		TypeOperation = setOperation;

		if ( !ResourceLoader.Exists( scenePath ) ) {
			GD.PushError( "Attempted to load non-existent resource: " + scenePath );
			return;
		}

		ResourceLoader.LoadThreadedRequest( scenePath, "", false, ResourceLoader.CacheMode.Replace );

		CallDeferred( "SetupUpdateSeconds" );
	}

	private void SetupUpdateSeconds() {
	}

	public void ChangeScene() {
		if ( !IsCompleted ) {
			GD.PushError( "Attempted instantiation of scene that hasn't loaded yet." );
			return;
		}

		if ( Changed ) {
			return;
		}

		if ( TypeOperation == LoadingSceneOperation.Replace ) {
			ChangeImmediate();
		} else if ( TypeOperation == LoadingSceneOperation.Additive ) {
			AdditiveScene();
		}

		Changed = true;
	}
	public string GetStatus() {
		return StatusNames[ ResourceLoader.LoadThreadedGetStatus( PackedScenePath ) ];
	}
	public void CheckStatus() {
		if ( IsCompleted ) {
			return;
		}

		ResourceLoader.ThreadLoadStatus status = ResourceLoader.LoadThreadedGetStatus( PackedScenePath );

		if ( status == ResourceLoader.ThreadLoadStatus.Loaded ) {
			MyResource = (PackedScene)ResourceLoader.LoadThreadedGet( PackedScenePath );
			if ( TypeOperation == LoadingSceneOperation.ReplaceImmediate ) {
				ChangeImmediate();
			} else if ( TypeOperation == LoadingSceneOperation.AdditiveImmediate ) {
				AdditiveScene();
			}
			Complete( false );
		}
		else if ( status == ResourceLoader.ThreadLoadStatus.InvalidResource ) {
			Complete( true );
		}
		else if ( status == ResourceLoader.ThreadLoadStatus.Failed ) {
			Complete( true );
		}
		else if ( status == ResourceLoader.ThreadLoadStatus.InProgress ) {
			Godot.Collections.Array progressArray = new Godot.Collections.Array();
			ResourceLoader.LoadThreadedGetStatus( PackedScenePath, progressArray );
			Progress = (float)progressArray[0] * 100.0f;
		}
	}

	private void Complete( bool bIsFailed ) {
		IsCompleted = !bIsFailed;
		Progress = 100.0f;
		Timer.QueueFree();

		EmitSignal( "OnComplete" );
	}
};
*/