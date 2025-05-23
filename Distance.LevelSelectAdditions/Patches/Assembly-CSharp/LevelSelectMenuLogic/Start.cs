using Distance.LevelSelectAdditions.Scripts;
using HarmonyLib;

namespace Distance.LevelSelectAdditions.Patches
{
	/// <summary>
	/// Patch to create the manager for the QUICK PLAYLIST rename button.
	/// <para/>
	/// And patch to create the manager for the Workshop rate this level button.
	/// </summary>
	[HarmonyPatch(typeof(LevelSelectMenuLogic), nameof(LevelSelectMenuLogic.Start))]
	internal static class LevelSelectMenuLogic__Start
	{
		[HarmonyPostfix]
		internal static void Postfix(LevelSelectMenuLogic __instance)
		{
			// Ensure our compound data component is attached.
			// `tempPlaylist_` is created in `LevelSelectMenuLogic.Start`, so this is the best point to ensure the data is added.
			__instance.tempPlaylist_.gameObject.GetOrAddComponent<LevelPlaylistCompoundData>();

			__instance.GetOrAddComponent<LevelPlaylistSelectRenameLogic>();
			__instance.GetOrAddComponent<LevelSelectWorkshopRateButtonLogic>();
		}
	}
}