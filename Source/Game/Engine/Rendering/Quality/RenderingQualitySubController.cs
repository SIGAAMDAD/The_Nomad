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

namespace Nomad.Game.Engine.Rendering.Quality
{
	internal abstract class RenderingQualitySubController
	{
		public abstract void Apply();

		protected static void SetIfPropertyExists( GodotObject obj, StringName propertyName, Variant value )
		{
			foreach ( var property in obj.GetPropertyList() ) {
				if ( property.ContainsKey( propertyName ) && property[ "name" ].AsStringName() == propertyName ) {
					obj.Set( propertyName, value );
					return;
				}
			}
		}

		protected static void SetVisibleForGroup( SceneTree tree, StringName group, bool visible )
		{
			foreach ( var node in tree.GetNodesInGroup( group ) ) {
				switch ( node ) {
					case CanvasItem canvasItem:
						canvasItem.Visible = visible;
						break;

					case Node3D node3D:
						node3D.Visible = visible;
						break;
				}
			}
		}
	};
};
