using HarmonyLib;

namespace Distance.LevelSelectAdditions.Harmony
{
	/// <summary>
	/// Patch to fix scrolling but in level select menu, where pressing up or down will only scroll to
	/// the top or bottom entries in the list.
	/// <para/>
	/// This bug can be reproduced with the following steps:
	/// <list type="number">
	/// <item>Enter the level select menu.</item>
	/// <item>Scroll up and select the search bar.</item>
	/// <item>Leave then re-enter the level select menu.</item>
	/// <item>Now that the menu has initialized with the search bar selected, you can only scroll to the top and bottom entries.</item>
	/// </list>
	/// The in-game fix for this bug is to manually select the search bar with your mouse, then scroll off of the search bar.
	/// </summary>
	[HarmonyPatch(typeof(LevelSelectMenuLogic), nameof(LevelSelectMenuLogic.Initialize))]
	internal static class LevelSelectMenuLogic__Initialize
	{
		[HarmonyPrefix]
		internal static void Prefix(LevelSelectMenuLogic __instance)
		{
			if (Mod.Instance.Config.FixLevelSelectScrollBug)
			{
				// Reset search bar selected state, if needed.
				if (__instance.searchInput_.isSelected)
				{
					// NOTE: isSelected alone won't be enough to deselect the input, because UIInput.selection is still
					//       assigned to the control (and determines the state of `isSelected`).
					//       So use the 'built-in' protected method `OnSelect` to change selection instead.
					//__instance.searchInput_.isSelected = false;
					//UIInput.selection = null;
					__instance.searchInput_.OnSelect(false);
				}
			}
		}
	}
}
