using Distance.LevelSelectAdditions.Helpers;
using HarmonyLib;

namespace Distance.LevelSelectAdditions.Harmony
{
	/// <summary>
	/// Patch to handle resetting the selected main menu (to remove level set selections).
	/// </summary>
	[HarmonyPatch(typeof(LevelGridMenu), nameof(LevelGridMenu.OnGridCellClicked))]
	internal static class LevelGridMenu__OnGridCellClicked
	{
		[HarmonyPrefix]
		internal static void Prefix(LevelGridMenu __instance)
		{
			if (__instance.levelSelectFinished_)
			{
				return;
			}
			if (__instance.displayType_ == LevelSelectMenuAbstract.DisplayType.ChooseMainMenuLevel)
			{
				// Clear level set used as main menu. The user has selected an individual level as their main menu.
				MainMenuLevelSetHelper.SetMainMenuLevelSet(null);
			}
		}
	}
}
