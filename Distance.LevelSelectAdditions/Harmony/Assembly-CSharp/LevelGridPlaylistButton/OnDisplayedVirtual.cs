using Distance.LevelSelectAdditions.Scripts;
using HarmonyLib;

namespace Distance.LevelSelectAdditions.Harmony
{
	/// <summary>
	/// Patch to add an icon showing if this playlist button is the active main menu collection.
	/// </summary>
	[HarmonyPatch(typeof(LevelGridPlaylistButton), nameof(LevelGridPlaylistButton.OnDisplayedVirtual))]
	internal static class LevelGridPlaylistButton__OnDisplayedVirtual
	{
		[HarmonyPostfix]
		internal static void Postfix(LevelGridPlaylistButton __instance)
		{
			LevelGridMenu.PlaylistEntry entry = __instance.entry_ as LevelGridMenu.PlaylistEntry;

			var compoundData = LevelGridButtonCurrentMainMenuLogic.GetOrCreate(__instance);
			if (compoundData)
			{
				// Show camera icon when this is the current main menu level.
				compoundData.UpdateCurrentMainMenuIcon();
				if (compoundData.IsCoveringUnplayedCircle)
				{
					// We're hijacking the Unplayed circle, prevent it from appearing during UpdateOrangeDot().
					__instance.unplayed_.gameObject.SetActive(false);
					entry.isNew_ = false;
				}
			}
		}
	}
}
