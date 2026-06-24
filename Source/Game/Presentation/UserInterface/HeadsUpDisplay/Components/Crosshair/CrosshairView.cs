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

using System;
using Godot;
using Nomad.Game.Sdk.HeadsUpDisplay;
using NumericsVector4 = System.Numerics.Vector4;

namespace Nomad.Game.Presentation.UserInterface.HeadsUpDisplay.Components.Crosshair
{
	/*
	===================================================================================

	CrosshairView

	===================================================================================
	*/
	/// <summary>
	/// Lightweight custom-drawn survival-action crosshair. The presenter owns all game
	/// state; this view only renders the currently requested reticle pose.
	/// </summary>

	internal sealed partial class CrosshairView : Control, IHudComponentView
	{
		private const float MIN_ALPHA = 0.0f;
		private const float MAX_ALPHA = 1.0f;

		[Export] public float BaseGapPixels { get; set; } = 7.0f;
		[Export] public float SegmentLengthPixels { get; set; } = 7.0f;
		[Export] public float SegmentWidthPixels { get; set; } = 1.6f;
		[Export] public float CenterDotRadiusPixels { get; set; } = 1.15f;
		[Export] public float SettledRingRadiusPixels { get; set; } = 2.5f;
		[Export] public float MaxVisualSpreadPixels { get; set; } = 34.0f;
		[Export] public Color ReticleColor { get; set; } = new Color( 0.92f, 0.92f, 0.86f, 0.92f );
		[Export] public Color ShadowColor { get; set; } = new Color( 0.02f, 0.02f, 0.018f, 0.52f );
		[Export] public Color ObstructedColor { get; set; } = new Color( 1.0f, 0.34f, 0.18f, 0.96f );

		[Export] public bool LineOfFireObstructionEnabled { get; set; } = true;
		[Export] public float LineOfFireRange { get; set; } = 120.0f;
		[Export] public uint LineOfFireCollisionMask { get; set; } = uint.MaxValue;
		[Export] public bool LineOfFireCollideWithBodies { get; set; } = true;
		[Export] public bool LineOfFireCollideWithAreas { get; set; } = false;
		[Export] public float LineOfFireMuzzleRightOffset { get; set; } = 0.32f;
		[Export] public float LineOfFireMuzzleUpOffset { get; set; } = -0.18f;
		[Export] public float LineOfFireMuzzleForwardOffset { get; set; } = 0.42f;
		[Export] public float LineOfFireEndpointTolerance { get; set; } = 0.28f;

		private float _spreadPixels = 10.0f;
		private float _settle = 1.0f;
		private float _alpha = 1.0f;
		private float _lineOfFireObstruction;
		private Vector2 _offsetPixels = Vector2.Zero;

		public override void _Ready()
		{
			base._Ready();

			MouseFilter = Control.MouseFilterEnum.Ignore;
			SetAnchorsPreset( Control.LayoutPreset.FullRect );
			SetOffsetsPreset( Control.LayoutPreset.FullRect );
		}

		public void SetColor( NumericsVector4 color )
		{
			ReticleColor = new Color( color.X, color.Y, color.Z, color.W );
			QueueRedraw();
		}

		public void SetReticle( float spreadPixels, float settle, float alpha, Vector2 offsetPixels )
		{
			_spreadPixels = Mathf.Clamp( spreadPixels, 0.0f, MaxVisualSpreadPixels );
			_settle = Mathf.Clamp( settle, 0.0f, 1.0f );
			_alpha = Mathf.Clamp( alpha, MIN_ALPHA, MAX_ALPHA );
			_offsetPixels = offsetPixels;
			QueueRedraw();
		}

		public void SetLineOfFireObstruction( float amount )
		{
			_lineOfFireObstruction = Mathf.Clamp( amount, 0.0f, 1.0f );
			QueueRedraw();
		}

		public override void _Draw()
		{
			base._Draw();

			if ( _alpha <= 0.001f ) {
				return;
			}

			Vector2 center = (Size * 0.5f) + _offsetPixels;
			float gap = BaseGapPixels + _spreadPixels;
			float lineLength = SegmentLengthPixels + (_spreadPixels * 0.08f);
			float halfWidth = SegmentWidthPixels * 0.5f;

			Color shadow = ShadowColor;
			shadow.A *= _alpha;

			Color reticle = ReticleColor.Lerp( ObstructedColor, _lineOfFireObstruction );
			reticle.A *= _alpha;

			DrawCrossSegments( center + new Vector2( 1.0f, 1.0f ), gap, lineLength, SegmentWidthPixels + 1.25f, shadow );
			DrawCrossSegments( center, gap, lineLength, SegmentWidthPixels, reticle );

			Color dotShadow = shadow;
			dotShadow.A *= 0.80f;
			DrawCircle( center + new Vector2( 1.0f, 1.0f ), CenterDotRadiusPixels + halfWidth, dotShadow );
			DrawCircle( center, CenterDotRadiusPixels + (_lineOfFireObstruction * 0.45f), reticle );

			DrawSettledRing( center, reticle, shadow );
			DrawObstructionMarker( center, reticle, shadow );
		}

		private void DrawCrossSegments( Vector2 center, float gap, float length, float width, Color color )
		{
			DrawLine(
				center + new Vector2( -gap - length, 0.0f ),
				center + new Vector2( -gap, 0.0f ),
				color,
				width,
				true
			);
			DrawLine(
				center + new Vector2( gap, 0.0f ),
				center + new Vector2( gap + length, 0.0f ),
				color,
				width,
				true
			);
			DrawLine(
				center + new Vector2( 0.0f, -gap - length ),
				center + new Vector2( 0.0f, -gap ),
				color,
				width,
				true
			);
			DrawLine(
				center + new Vector2( 0.0f, gap ),
				center + new Vector2( 0.0f, gap + length ),
				color,
				width,
				true
			);
		}

		private void DrawSettledRing( Vector2 center, Color reticle, Color shadow )
		{
			float ringAlpha = _alpha * _settle * 0.28f * (1.0f - (_lineOfFireObstruction * 0.65f));
			if ( ringAlpha <= 0.01f ) {
				return;
			}

			Color ringShadow = shadow;
			ringShadow.A = ringAlpha * 0.75f;

			Color ring = reticle;
			ring.A = ringAlpha;

			float radius = SettledRingRadiusPixels + (_settle * 1.5f);
			DrawArc( center + new Vector2( 1.0f, 1.0f ), radius, 0.0f, (MathF.PI * 2.0f), 48, ringShadow, 1.2f, true );
			DrawArc( center, radius, 0.0f, (MathF.PI * 2.0f), 48, ring, 0.8f, true );
		}

		private void DrawObstructionMarker( Vector2 center, Color reticle, Color shadow )
		{
			if ( _lineOfFireObstruction <= 0.01f ) {
				return;
			}

			Color markerShadow = shadow;
			markerShadow.A *= _lineOfFireObstruction * 0.9f;

			Color marker = reticle;
			marker.A *= _lineOfFireObstruction;

			float radius = 5.5f + (_lineOfFireObstruction * 1.5f);
			DrawLine( center + new Vector2( -radius, radius ) + new Vector2( 1.0f, 1.0f ), center + new Vector2( radius, -radius ) + new Vector2( 1.0f, 1.0f ), markerShadow, 2.25f, true );
			DrawLine( center + new Vector2( -radius, radius ), center + new Vector2( radius, -radius ), marker, 1.15f, true );
		}
	};
};
