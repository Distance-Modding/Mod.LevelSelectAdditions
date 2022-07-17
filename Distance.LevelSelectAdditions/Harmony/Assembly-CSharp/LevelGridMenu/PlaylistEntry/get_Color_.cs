using Distance.LevelSelectAdditions.Extensions;
using HarmonyLib;
using System;
using UnityEngine;

namespace Distance.LevelSelectAdditions.Harmony
{
	/// <summary>
	/// Patch to determine if a PlaylistEntry name is intending to use full-color support (with <c>[c][/c]</c>
	/// symbols), and change the returned color to white so that the color isn't multiplied by the default
	/// <see cref="GConstants.myLevelColor_"/>.
	/// </summary>
	/// <remarks>
	/// Required For: Level Set Options menu Recolor button.
	/// </remarks>
	[HarmonyPatch(typeof(LevelGridMenu.PlaylistEntry), nameof(LevelGridMenu.PlaylistEntry.Color_), MethodType.Getter)]
	internal static class LevelGridMenu_PlaylistEntry__get_Color_
	{
		[HarmonyPostfix]
		internal static void Postfix(LevelGridMenu.PlaylistEntry __instance, ref Color __result)
		{
			if (__instance.type_ == LevelGridMenu.PlaylistEntry.Type.Personal)
			{
				if (__instance.playlist_.GetColor(out bool colorTag, out _, false))
				{
					if (colorTag)
					{
						// Otherwise we'll return the normal color, which is multiplied against the base color tags.
						// This will keep color tags used without this mod consistent.
						__result = Color.white;
					}
					else
					{
						// Color functions in vanilla by multiplying against the personal playlist color.
						__result = GConstants.myLevelColor_;
					}
				}
			}
		}
	}
}
