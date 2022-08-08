using Distance.LevelSelectAdditions.Scripts;
using HarmonyLib;

namespace Distance.LevelSelectAdditions.Harmony
{
	/// <summary>
	/// Patch to add an icon showing if this level grid cell is the active main menu.
	/// </summary>
	[HarmonyPatch(typeof(LevelGridCell), nameof(LevelGridCell.OnDisplayedVirtual))]
	internal static class LevelGridCell__OnDisplayedVirtual
	{
		[HarmonyPostfix]
		internal static void Postfix(LevelGridCell __instance)
		{
			LevelGridGrid.LevelEntry entry = __instance.entry_ as LevelGridGrid.LevelEntry;
			bool isMainMenu = entry.levelGridMenu_.DisplayType_ == LevelSelectMenuAbstract.DisplayType.ChooseMainMenuLevel;

			// Hide medal logo when in Choose Main Menu level.
			__instance.medalLogo_.gameObject.SetActive(!isMainMenu);

			var compoundData = LevelGridButtonCurrentMainMenuLogic.GetOrCreate(__instance);
			if (compoundData)
			{
				// Show camera icon when this is the current main menu level.
				compoundData.UpdateCurrentMainMenuIcon();
				if (compoundData.IsCoveringUnplayedCircle)
				{
					// We're hijacking the Unplayed circle, prevent it from appearing during UpdateOrangeDot().
					__instance.unplayed_.gameObject.SetActive(false);
					__instance.isNew_ = false;
				}
			}
		}
	}
}
