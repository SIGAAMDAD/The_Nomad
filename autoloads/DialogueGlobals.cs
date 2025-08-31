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
using Renown.Thinkers;

/*
===================================================================================

DialogueGlobals

===================================================================================
*/
/// <summary>
/// variables for global access for DialogueManager3
/// </summary>

public partial class DialogueGlobals : Node {
	/// <summary>
	/// name of the bot that's currently talking
	/// </summary>
	[Export]
	public StringName BotName;

	[Export]
	public MercenaryMaster MercMaster;

	/// <summary>
	/// the player's choice
	/// </summary>
	[Export]
	public int PlayerChoice = 0;

	[Export]
	public Thinker Entity;

	private static DialogueGlobals Instance;

	/*
	===============
	GetPlayer
	===============
	*/
	/// <summary>
	/// for access in .dialogue files
	/// </summary>
	/// <returns></returns>
	public static Player GetPlayer() {
		return LevelData.Instance.ThisPlayer;
	}

	/*
	===============
	Get
	===============
	*/
	/// <summary>
	/// for access in .dialogue files
	/// </summary>
	/// <returns></returns>
	public static DialogueGlobals Get() {
		return Instance;
	}

	/*
	===============
	_EnterTree
	===============
	*/
	public override void _EnterTree() {
		base._EnterTree();

		Instance = this;
	}
};