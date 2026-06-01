/*
===========================================================================
The Nomad MPLv2 Source Code
Copyright (C) 2025-2026 Noah Van Til

This Source Code Form is subject to the terms of the Mozilla Public
License, v2. If a copy of the MPL was not distributed with this
file, You can obtain one at https://mozilla.org/MPL/2.0/.

This software is provided "as is", without warranty of any kind,
express or implied, including but not limited to the warranties
of merchantability, fitness for a particular purpose and noninfringement.
===========================================================================
*/

using Godot;

namespace Nomad.Game.Presentation.World
{
	/*
	===================================================================================

	WorldSunLightPrefab

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal sealed partial class WorldSunLightPrefab : Node2D
	{
		[ExportGroup( "Nodes" )]
		[Export]
		public CanvasModulate AmbientLight { get; private set; }

		[Export]
		public DirectionalLight2D SunLight { get; private set; }

		[ExportGroup( "Sun Arc" )]
		[Export]
		public Vector2 SunOrbitCenter { get; set; } = Vector2.Zero;

		[Export]
		public float SunOrbitRadius { get; set; } = 1536.0f;

		//
		// Visual arc used by the DirectionalLight2D.
		// These are screen/world-space angles, not astronomical values.
		//
		[Export]
		public float ArcStartDegrees { get; set; } = 160.0f;

		[Export]
		public float ArcEndDegrees { get; set; } = 380.0f;

		//
		// Use this if your occluder shadows point the wrong way.
		//
		[Export]
		public float LightRotationOffsetDegrees { get; set; } = 0.0f;

		[ExportGroup( "Minute Transition" )]
		[Export]
		public float MinuteTweenSeconds { get; set; } = 3.75f;

		[Export]
		public bool SnapOnFirstApply { get; set; } = true;

		[ExportGroup( "Light Energy" )]
		[Export]
		public float DayLightEnergy { get; set; } = 1.10f;

		[Export]
		public float TwilightLightEnergy { get; set; } = 0.45f;

		[Export]
		public float MoonLightEnergy { get; set; } = 0.08f;

		[ExportGroup( "Light Height" )]
		[Export]
		public float MinLightHeight { get; set; } = 0.08f;

		[Export]
		public float MaxLightHeight { get; set; } = 0.65f;

		[ExportGroup( "Light Colors" )]
		[Export]
		public Color DayLightColor { get; set; } = new Color( 1.0f, 0.94f, 0.78f );

		[Export]
		public Color DawnLightColor { get; set; } = new Color( 1.0f, 0.55f, 0.32f );

		[Export]
		public Color DuskLightColor { get; set; } = new Color( 0.95f, 0.38f, 0.22f );

		[Export]
		public Color MoonLightColor { get; set; } = new Color( 0.34f, 0.42f, 0.62f );

		[Export]
		public Color BaseShadowColor { get; set; } = new Color( 0.0f, 0.0f, 0.0f, 0.42f );
	};
};
