using Godot;
using ImGuiNET;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public partial class Profiler : Node {
	private Dictionary<string, float> MethodTimes;
	private static Profiler Instance;

	public static void AddTimeData( float delta, [CallerMemberName] string callerName = null ) {
		if ( Instance.MethodTimes.TryGetValue( callerName, out float time ) ) {
			Instance.MethodTimes[ callerName ] = delta;
		} else {
			Instance.MethodTimes.Add( callerName, delta );
		}
	}

	public override void _Ready() {
		base._Ready();

		MethodTimes = new Dictionary<string, float>();

		Instance = this;
	}
	public override void _Process( double delta ) {
		base._Process( delta );
	}
};