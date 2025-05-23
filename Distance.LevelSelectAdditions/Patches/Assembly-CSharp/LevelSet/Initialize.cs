using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Distance.LevelSelectAdditions.Patches
{
	/// <summary>
	/// Patch to fix missing levels in the Advanced level select menu. The following levels are missing:
	/// <list type="bullet">
	/// <item>Sprint: Zenith</item>
	/// <item>ReverseTag: Stuntware 2051</item>
	/// </list>
	/// </summary>
	[HarmonyPatch(typeof(LevelSet), nameof(LevelSet.Initialize))]
	internal static class LevelSet__Initialize
	{
		[HarmonyPrefix]
		internal static void Prefix(LevelSet __instance)
		{
			if (__instance.resourcesLevelNameAndPathPairsInSet_.Count > 0)
			{
				return; // Already initialized
			}

			// Re-add levels that are missing in the Advanced level select menu (this setting requires a restart).
			// All official (but not community) levels are loaded from the `resourcesLevelFileNamesInSet_` field.
			// This is likely initialized by UnityEngine at some point outside of our view of the code.
			// This field stores file names without extension or relative path.
			List<string> knownMissingFileNames = new List<string>();
			switch (__instance.gameModeID_)
			{
			case GameModeID.Sprint:
				//Mod.Instance.Logger.Debug("GameModeID.Sprint");
				knownMissingFileNames.Add("Zenith");
				break;

			case GameModeID.ReverseTag:
				//Mod.Instance.Logger.Debug("GameModeID.ReverseTag");
				knownMissingFileNames.Add("Stuntware 2051");

				// The following are neither in the Advanced level select menu, or any level set grids.
				// However they still have ReverseTag as a supported mode - which is likely a mistake,
				//  as the Stunt map (Refraction) that was removed in the Radical Update had its mode properly removed.

				// Unsure when this was removed, but probably for the same reason as below.
				//knownMissingFileNames.Add("Quantum Core");

				// These were intentionally removed as noted in the Radical Update changelog.
				//knownMissingFileNames.Add("Space Skate");
				//knownMissingFileNames.Add("Stunt Playground");
				break;
			}

			if (knownMissingFileNames.Count > 0)
			{
				// Double-check to make sure these levels haven't already been added by another mod, or game update.
				List<string> newResourceLevelFileNamesInSet = __instance.resourcesLevelFileNamesInSet_.ToList();
				foreach (string fileName in knownMissingFileNames)
				{
					if (Array.IndexOf(__instance.resourcesLevelFileNamesInSet_, fileName) == -1)
					{
						newResourceLevelFileNamesInSet.Add(fileName);
					}
				}

				int added = newResourceLevelFileNamesInSet.Count - __instance.resourcesLevelFileNamesInSet_.Length;
				if (added > 0)
				{
					// Update the array with the added missing levels.
					__instance.resourcesLevelFileNamesInSet_ = newResourceLevelFileNamesInSet.ToArray();

					string header = $"{nameof(GameModeID)}.{__instance.gameModeID_}: ";
					Mod.Log.LogInfo($"{header}Added {added} missing {GUtils.GetPlural("level", added)} to the Advanced level select menu.");
				}
			}
		}
	}
}
