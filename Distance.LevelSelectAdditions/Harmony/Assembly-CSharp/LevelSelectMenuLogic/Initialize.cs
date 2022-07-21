using Distance.LevelSelectAdditions.Extensions;
using Distance.LevelSelectAdditions.Scripts;
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
	/// <para/>
	/// Also includes patch to hide unused Leaderboards (and optionally Playlist Mode) buttons when in the Choose Main Menu display type.
	/// <para/>
	/// Also includes patch to reset and update various states required when entering the Advanced level select menu.
	/// </summary>
	[HarmonyPatch(typeof(LevelSelectMenuLogic), nameof(LevelSelectMenuLogic.Initialize))]
	internal static class LevelSelectMenuLogic__Initialize
	{
		[HarmonyPrefix]
		internal static void Prefix(LevelSelectMenuLogic __instance)
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

			// Reflect the current `EnableRateWorkshopLevelButton` setting.
			var workshopRateButtonLogic = __instance.GetOrAddComponent<LevelSelectWorkshopRateButtonLogic>();
			if (workshopRateButtonLogic)
			{
				workshopRateButtonLogic.Initialize();
			}

			// Cleanup Playlist Mode states that aren't reset when leaving via MenuCancel.
			__instance.ResetTempPlaylistState();
		}

		[HarmonyPostfix]
		internal static void Postfix(LevelSelectMenuLogic __instance)
		{
			// This needs to be handled in the Postfix, because something inside `Initialize`
			//  indirectly changes the active state of these buttons.
			__instance.UpdateBottomLeftButtonVisibility();
		}
	}
}
