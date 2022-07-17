using Distance.LevelSelectAdditions.Scripts;
using HarmonyLib;

namespace Distance.LevelSelectAdditions.Harmony
{
	/// <summary>
	/// Patch to add extra logic to playlist entry buttons, so that they update their name and color
	/// after leaving the Level Set options menu.
	/// </summary>
	[HarmonyPatch(typeof(LevelGridMenu), nameof(LevelGridMenu.Display))]
	internal static class LevelGridMenu__Display
	{
		[HarmonyPrefix]
		internal static void Prefix(LevelGridMenu __instance)
		{
			__instance.GetOrAddComponent<LevelPlaylistEntryUpdateLogic>();
		}
	}
}
