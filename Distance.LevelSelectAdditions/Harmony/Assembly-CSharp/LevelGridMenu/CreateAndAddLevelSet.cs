using Distance.LevelSelectAdditions.Sorting;
using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;

//using SortingMethod = LevelSelectMenuLogic.SortingMethod;

namespace Distance.LevelSelectAdditions.Harmony
{
	/// <summary>
	/// A patch applied to the method that handles preparing and adding LevelSets to the list.
	/// <para/>
	/// Here we need to add our own handling for Workshop LevelSets and remove the original <c>levelPlaylist.LimitCountTo(100)</c>.
	/// </summary>
	[HarmonyPatch(typeof(LevelGridMenu), nameof(LevelGridMenu.CreateAndAddLevelSet))]
	internal static class LevelGridMenu__CreateAndAddLevelSet
	{
		[HarmonyPrefix]
		internal static bool Prefix(LevelGridMenu __instance, LevelSet set, string name, LevelGridMenu.PlaylistEntry.Type type, LevelGroupFlags flags)
		{
			// Only perform special handling for Workshop LevelSets (since that's all that's currently supported).
			if (type == LevelGridMenu.PlaylistEntry.Type.Workshop)
			{
				LevelPlaylist levelPlaylist = LevelPlaylist.Create(set, name, flags);
				LevelGridMenu.PlaylistEntry.UnlockStyle unlockStyle = LevelGridMenu.PlaylistEntry.UnlockStyle.None;

				// Filter out levels in personal playlists, if the user has specified this.
				if (Mod.Instance.Config.HideWorkshopLevelsInPlaylists)
				{
					LevelFilter.ExcludeLevelsInPersonalPlaylists(levelPlaylist, __instance.modeID_);
				}

				// Apply custom sorting.
				{
					var sorter = new LevelSort(Mod.Instance.Config.GetWorkshopSortingMethods(),
											   Mod.Instance.Config.GetWorkshopReverseSortingMethods());
					sorter.SortPlaylist(__instance, levelPlaylist);
				}

				// Finally apply limit after sorting and potentially removing levels that were in playlists.
				if (Mod.Instance.Config.WorkshopLevelLimit != LevelFilter.Infinite)
				{
					LevelFilter.LimitLevels(levelPlaylist, Mod.Instance.Config.WorkshopLevelLimit);
				}


				// All done! Add the organized LevelPlaylist entry.
				__instance.CreateAndAddEntry(levelPlaylist, type, true, unlockStyle, string.Empty);


				// Skip the original method, since we've fully handled this level set.
				return false;
			}

			// Fallback to the original method for anything that's not a Workshop LevelSet.
			return true;
		}

	}
}
