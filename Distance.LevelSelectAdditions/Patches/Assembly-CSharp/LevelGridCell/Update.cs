using Distance.LevelSelectAdditions.Scripts;
using HarmonyLib;

namespace Distance.LevelSelectAdditions.Patches
{
	/// <summary>
	/// Patch to force level grid cells to stay enabled, in order to update the active main menu icon's highlight color.
	/// This is needed because the normal Update method disables the level grid cell after the image is loaded.
	/// </summary>
	[HarmonyPatch(typeof(LevelGridCell), nameof(LevelGridCell.Update))]
	internal static class LevelGridCell__Update
	{
		[HarmonyPostfix]
		internal static void Postfix(LevelGridCell __instance)
		{
			// We only need to stay enabled when Choosing Main Menu levels.
			LevelGridGrid.LevelEntry entry = __instance.entry_ as LevelGridGrid.LevelEntry;
			bool isMainMenu = entry.levelGridMenu_.DisplayType_ == LevelSelectMenuAbstract.DisplayType.ChooseMainMenuLevel;

			if (isMainMenu && LevelGridButtonCurrentMainMenuLogic.ShowIconHighlight)
			{
				__instance.enabled = true;
			}
		}
	}
}
