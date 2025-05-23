using Distance.LevelSelectAdditions.Extensions;
using Distance.LevelSelectAdditions.Scripts;

using HarmonyLib;
using System;
using UnityEngine;

namespace Distance.LevelSelectAdditions.Patches
{
	/// <summary>
	/// Patch to strip NGUI color symbols (<c>[c][/c]</c>) from <c>labelText_</c>, so that PlaylistEntry button
	/// text still properly highlights to black when hovered over.
	/// <para/>
	/// Also assigns this PlaylistEntry to the compound data for the playlist, so that we can access the entry
	/// type and color.
	/// </summary>
	[HarmonyPatch(typeof(LevelGridMenu.PlaylistEntry), MethodType.Constructor, typeof(LevelGridMenu), typeof(string), typeof(LevelPlaylist), typeof(LevelGridMenu.PlaylistEntry.Type), typeof(bool), typeof(LevelGridMenu.PlaylistEntry.UnlockStyle), typeof(bool))]
	internal static class LevelGridMenu_PlaylistEntry__ctor
	{
		// Argument types (but no method name) is required for patching constructors with `HarmonyPatch`.

		[HarmonyPostfix]
		internal static void Postfix(LevelGridMenu.PlaylistEntry __instance)
		{
			if (__instance.type_ == LevelGridMenu.PlaylistEntry.Type.Personal)
			{
				// The label text with symbols is preserved by the playlist name.
				// TODO: We need to make sure not to rely on the playlist name after renaming/recoloring.
				//       Because that will change what was expected of it for the playlist entry buttons(?).
				if (__instance.labelText_.DecodeNGUIColorTag(out string stripped))
				{
					//Mod.Instance.Logger.Debug("labelText_ changed");
					__instance.labelText_ = stripped;
				}
			}

			var playlistData = __instance.Playlist_.GetComponent<LevelPlaylistCompoundData>();
			if (playlistData)
			{
				playlistData.PlaylistEntry = __instance;
			}
		}
	}
}
