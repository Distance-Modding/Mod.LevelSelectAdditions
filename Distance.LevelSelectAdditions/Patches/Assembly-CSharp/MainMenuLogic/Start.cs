using Distance.LevelSelectAdditions.Helpers;
using HarmonyLib;

namespace Distance.LevelSelectAdditions.Patches
{
	/// <summary>
	/// Patch to trigger changing the level set main menu level.
	/// </summary>
	[HarmonyPatch(typeof(MainMenuLogic), nameof(MainMenuLogic.Start))]
	internal static class MainMenuLogic__Start
	{
		[HarmonyPrefix]
		internal static void Prefix(MainMenuLogic __instance)
		{
			MainMenuLevelSetHelper.ChooseNextMainMenu();
		}
	}
}
