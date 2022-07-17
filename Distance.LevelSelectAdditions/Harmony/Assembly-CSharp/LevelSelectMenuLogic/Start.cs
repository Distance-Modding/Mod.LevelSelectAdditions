using Distance.LevelSelectAdditions.Scripts;
using HarmonyLib;

namespace Distance.LevelSelectAdditions.Harmony
{
	/// <summary>
	/// Patch to create the manager for the QUICK PLAYLIST rename button.
	/// </summary>
	[HarmonyPatch(typeof(LevelSelectMenuLogic), nameof(LevelSelectMenuLogic.Start))]
	internal static class LevelSelectMenuLogic__Start
	{
		[HarmonyPostfix]
		internal static void Postfix(LevelSelectMenuLogic __instance)
		{
			__instance.GetOrAddComponent<LevelPlaylistSelectRenameLogic>();
		}
	}
}