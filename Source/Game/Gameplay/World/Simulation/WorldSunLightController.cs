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
using Nomad.Core.Compatibility.Guards;
using Nomad.Game.Presentation.World;
using Nomad.Game.Sdk.Events.World;
using Nomad.Game.Sdk.World;

namespace Nomad.Game.Gameplay.World.Simulation
{
	internal sealed class WorldSunLightController
	{
		private readonly WorldSunLightPrefab _prefab;
		private readonly DirectionalLight2D _directionalLight;
		private readonly CanvasModulate _canvasModulate;
		private readonly ICalendarService _calendar;
		private readonly SeasonService _seasonService;

		private readonly IDisposable _minuteChanged;
		private readonly IDisposable _seasonChanged;

		private Tween? _minuteTween;

		private readonly object _queuedTimeLock = new();

		private WorldTime _queuedTime;
		private bool _hasQueuedTime = false;
		private bool _deferredApplyQueued = false;

		private bool _hasAppliedOnce = false;

		public WorldSunLightController( ICalendarService calendarService, SeasonService seasonService, WorldSunLightPrefab prefab )
		{
			ArgumentGuard.ThrowIfNull( prefab, nameof( prefab ) );

			_prefab = prefab ?? throw new ArgumentNullException( nameof( prefab ) );
			_calendar = calendarService ?? throw new ArgumentNullException( nameof( calendarService ) );
			_seasonService = seasonService ?? throw new ArgumentNullException( nameof( seasonService ) );

			_directionalLight = _prefab.SunLight;
			_canvasModulate = _prefab.AmbientLight;

			_minuteChanged = _calendar.MinuteChanged.Subscribe( OnMinuteChanged );
			_seasonChanged = _seasonService.SeasonChanged.Subscribe( OnSeasonChanged );
		}

		/*
		================
		OnMinuteChanged
		================
		*/
		private void OnMinuteChanged( in MinuteChangedEventArgs args )
		{
			QueueTimeApply( args.Time );
		}

		/*
		===============
		OnSeasonChanged
		===============
		*/
		private void OnSeasonChanged( in SeasonChangedEventArgs args )
		{
			//
			// Season changes are still applied through the same minute-sample path.
			// This matters because season changes can alter ambient colors and daylight length.
			//
			QueueTimeApply( args.Time );
		}

		/*
		==============
		QueueTimeApply
		==============
		*/
		private void QueueTimeApply( in WorldTime time )
		{
			bool shouldCallDeferred = false;

			lock ( _queuedTimeLock ) {
				_queuedTime = time;
				_hasQueuedTime = true;

				if ( !_deferredApplyQueued ) {
					_deferredApplyQueued = true;
					shouldCallDeferred = true;
				}
			}

			if ( shouldCallDeferred ) {
				Callable.From( ApplyQueuedTimeDeferred ).CallDeferred();
			}
		}

		/*
		=======================
		ApplyQueuedTimeDeferred
		=======================
		*/
		private void ApplyQueuedTimeDeferred()
		{
			WorldTime time;

			lock ( _queuedTimeLock ) {
				if ( !_hasQueuedTime ) {
					_deferredApplyQueued = false;
					return;
				}

				time = _queuedTime;
				_hasQueuedTime = false;
				_deferredApplyQueued = false;
			}

			ApplyTimeTweened( time );
		}

		/*
		=================
		RefreshFromCurrent
		=================
		*/
		/// <summary>
		/// Useful after loading a save, using debug time commands, or entering a world scene.
		/// </summary>
		public void RefreshFromCurrent()
		{
			QueueTimeApply( _calendar.Current );
		}

		/*
		===================
		ApplyTimeImmediate
		===================
		*/
		private void ApplyTimeImmediate( in WorldTime time )
		{
			LightingSample sample = BuildLightingSample( time );

			_minuteTween?.Kill();
			_minuteTween = null;

			_canvasModulate.Color = sample.Ambient;

			_directionalLight.GlobalPosition = sample.LightPosition;
			_directionalLight.Rotation = sample.LightRotation;
			_directionalLight.Color = sample.LightColor;
			_directionalLight.Energy = sample.LightEnergy;
			_directionalLight.Height = sample.LightHeight;
			_directionalLight.ShadowColor = sample.ShadowColor;
			_directionalLight.Enabled = sample.LightEnergy > 0.01f;

			_hasAppliedOnce = true;
		}

		/*
		================
		ApplyTimeTweened
		================
		*/
		private void ApplyTimeTweened( in WorldTime time )
		{
			LightingSample sample = BuildLightingSample( time );

			if ( _prefab.SnapOnFirstApply && !_hasAppliedOnce ) {
				ApplyTimeImmediate( time );
				return;
			}

			_hasAppliedOnce = true;

			_minuteTween?.Kill();

			float duration = Math.Max( 0.0f, _prefab.MinuteTweenSeconds );

			if ( duration <= 0.0f ) {
				ApplyTimeImmediate( time );
				return;
			}

			float targetRotation = _directionalLight.Rotation
				+ Mathf.AngleDifference( _directionalLight.Rotation, sample.LightRotation );

			_directionalLight.Enabled = sample.LightEnergy > 0.01f || _directionalLight.Energy > 0.01f;

			_minuteTween = _prefab.CreateTween();
			_minuteTween.SetParallel( true );
			_minuteTween.SetTrans( Tween.TransitionType.Sine );
			_minuteTween.SetEase( Tween.EaseType.InOut );

			_minuteTween.TweenProperty(
				_canvasModulate,
				"color",
				sample.Ambient,
				duration
			);

			_minuteTween.TweenProperty(
				_directionalLight,
				"global_position",
				sample.LightPosition,
				duration
			);

			_minuteTween.TweenProperty(
				_directionalLight,
				"rotation",
				targetRotation,
				duration
			);

			_minuteTween.TweenProperty(
				_directionalLight,
				"color",
				sample.LightColor,
				duration
			);

			_minuteTween.TweenProperty(
				_directionalLight,
				"energy",
				sample.LightEnergy,
				duration
			);

			_minuteTween.TweenProperty(
				_directionalLight,
				"height",
				sample.LightHeight,
				duration
			);

			_minuteTween.TweenProperty(
				_directionalLight,
				"shadow_color",
				sample.ShadowColor,
				duration
			);

			_minuteTween.Finished += () => {
				_directionalLight.Rotation = sample.LightRotation;
				_directionalLight.Enabled = sample.LightEnergy > 0.01f;
			};
		}

		/*
		===================
		BuildLightingSample
		===================
		*/
		private LightingSample BuildLightingSample( in WorldTime time )
		{
			SeasonDefinition season = _seasonService.CurrentSeason;

			float hour = time.Hour + (time.Minute / 60.0f);

			SolarTimes solar = SolarTimes.FromSeason( season );

			float skyProgress01 = solar.GetSkyProgress01( hour );
			float dayProgress01 = solar.GetDayProgress01( hour );
			float sunElevation01 = solar.GetSunElevation01( hour );

			float arcDegrees = Mathf.Lerp( _prefab.ArcStartDegrees, _prefab.ArcEndDegrees, skyProgress01 );
			float arcRadians = Mathf.DegToRad( arcDegrees );

			Vector2 sunPositionDirection = new Vector2(
				Mathf.Cos( arcRadians ),
				Mathf.Sin( arcRadians )
			);

			Vector2 lightPosition = _directionalLight.GlobalPosition
				+ _prefab.SunOrbitCenter
				+ (sunPositionDirection * _prefab.SunOrbitRadius);

			Vector2 lightDirection = -sunPositionDirection;

			float lightRotation = lightDirection.Angle()
				+ Mathf.DegToRad( _prefab.LightRotationOffsetDegrees );

			float lightHeight = Mathf.Lerp(
				_prefab.MinLightHeight,
				_prefab.MaxLightHeight,
				Smooth01( sunElevation01 )
			);

			Color ambient;
			Color lightColor;
			float lightEnergy;

			if ( solar.IsNight( hour ) ) {
				ambient = season.NightAmbient;
				lightColor = _prefab.MoonLightColor;
				lightEnergy = _prefab.MoonLightEnergy;
			} else if ( solar.IsDawn( hour ) ) {
				float t = Smooth01( solar.GetDawnProgress01( hour ) );

				ambient = season.NightAmbient.Lerp( season.DawnAmbient, t );
				lightColor = _prefab.MoonLightColor.Lerp( _prefab.DawnLightColor, t );
				lightEnergy = Mathf.Lerp( _prefab.MoonLightEnergy, _prefab.TwilightLightEnergy, t );
			} else if ( solar.IsDusk( hour ) ) {
				float t = Smooth01( solar.GetDuskProgress01( hour ) );

				ambient = season.DuskAmbient.Lerp( season.NightAmbient, t );
				lightColor = _prefab.DuskLightColor.Lerp( _prefab.MoonLightColor, t );
				lightEnergy = Mathf.Lerp( _prefab.TwilightLightEnergy, _prefab.MoonLightEnergy, t );
			} else {
				float elevation = Smooth01( sunElevation01 );

				Color horizonAmbient = season.DawnAmbient.Lerp( season.DuskAmbient, dayProgress01 );
				Color horizonLight = _prefab.DawnLightColor.Lerp( _prefab.DuskLightColor, dayProgress01 );

				ambient = horizonAmbient.Lerp( season.DayAmbient, elevation );
				lightColor = horizonLight.Lerp( _prefab.DayLightColor, elevation );
				lightEnergy = Mathf.Lerp( _prefab.TwilightLightEnergy, _prefab.DayLightEnergy, elevation );
			}

			float shadowAlpha = Mathf.Lerp(
				0.62f,
				_prefab.BaseShadowColor.A,
				Smooth01( sunElevation01 )
			);

			Color shadowColor = new Color(
				_prefab.BaseShadowColor.R,
				_prefab.BaseShadowColor.G,
				_prefab.BaseShadowColor.B,
				shadowAlpha
			);

			return new LightingSample {
				Ambient = ambient,
				LightColor = lightColor,
				ShadowColor = shadowColor,
				LightPosition = lightPosition,
				LightRotation = lightRotation,
				LightEnergy = lightEnergy,
				LightHeight = lightHeight
			};
		}

		/*
		========
		Smooth01
		========
		*/
		private static float Smooth01( float t )
		{
			t = Math.Clamp( t, 0.0f, 1.0f );
			return t * t * (3.0f - (2.0f * t));
		}

		private readonly struct LightingSample
		{
			public Color Ambient { get; init; }
			public Color LightColor { get; init; }
			public Color ShadowColor { get; init; }

			public Vector2 LightPosition { get; init; }
			public float LightRotation { get; init; }
			public float LightEnergy { get; init; }
			public float LightHeight { get; init; }
		}

		private readonly struct SolarTimes
		{
			public readonly float DawnStart;
			public readonly float Sunrise;
			public readonly float SolarNoon;
			public readonly float Sunset;
			public readonly float DuskEnd;

			private SolarTimes(
				float dawnStart,
				float sunrise,
				float solarNoon,
				float sunset,
				float duskEnd
			)
			{
				DawnStart = dawnStart;
				Sunrise = sunrise;
				SolarNoon = solarNoon;
				Sunset = sunset;
				DuskEnd = duskEnd;
			}

			public static SolarTimes FromSeason( SeasonDefinition season )
			{
				float solarNoon = 12.0f;

				float daylightHours = Math.Clamp( season.DaylightHours, 1.0f, 23.0f );
				float twilightHours = Math.Clamp( season.TwilightHours, 0.0f, 4.0f );

				float sunrise = solarNoon - (daylightHours * 0.5f);
				float sunset = solarNoon + (daylightHours * 0.5f);

				float dawnStart = sunrise - twilightHours;
				float duskEnd = sunset + twilightHours;

				return new SolarTimes(
					dawnStart,
					sunrise,
					solarNoon,
					sunset,
					duskEnd
				);
			}

			public bool IsNight( float hour )
			{
				return hour < DawnStart || hour > DuskEnd;
			}

			public bool IsDawn( float hour )
			{
				return hour >= DawnStart && hour < Sunrise;
			}

			public bool IsDusk( float hour )
			{
				return hour > Sunset && hour <= DuskEnd;
			}

			public float GetDawnProgress01( float hour )
			{
				return InverseLerpClamped( DawnStart, Sunrise, hour );
			}

			public float GetDuskProgress01( float hour )
			{
				return InverseLerpClamped( Sunset, DuskEnd, hour );
			}

			public float GetSkyProgress01( float hour )
			{
				return InverseLerpClamped( DawnStart, DuskEnd, hour );
			}

			public float GetDayProgress01( float hour )
			{
				return InverseLerpClamped( Sunrise, Sunset, hour );
			}

			public float GetSunElevation01( float hour )
			{
				if ( hour < Sunrise || hour > Sunset ) {
					return 0.0f;
				}

				float dayProgress = GetDayProgress01( hour );

				return Math.Clamp(
					Mathf.Sin( dayProgress * Mathf.Pi ),
					0.0f,
					1.0f
				);
			}

			private static float InverseLerpClamped( float a, float b, float value )
			{
				if ( Math.Abs( b - a ) <= float.Epsilon ) {
					return 0.0f;
				}

				return Math.Clamp( (value - a) / (b - a), 0.0f, 1.0f );
			}
		};
	};
};
