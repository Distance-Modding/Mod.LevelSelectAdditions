using Distance.LevelSelectAdditions.Extensions;
using HarmonyLib;

namespace Distance.LevelSelectAdditions.Harmony
{
	/// <summary>
	/// Patch to remember the playlist for the last-selected level.
	/// </summary>
	[HarmonyPatch(typeof(LevelGridMenu), nameof(LevelGridMenu.OnLevelEntrySelected))]
	internal static class LevelGridMenu__OnLevelEntrySelected
	{
		[HarmonyPrefix]
		internal static void Prefix(LevelGridMenu __instance)
		{
			Mod.Instance.Config.SetStateLastLevelSetID(__instance.displayType_, __instance.modeID_, __instance.levelGridGrid_.playlist_.GetLevelSetID());
		}
	}
}
