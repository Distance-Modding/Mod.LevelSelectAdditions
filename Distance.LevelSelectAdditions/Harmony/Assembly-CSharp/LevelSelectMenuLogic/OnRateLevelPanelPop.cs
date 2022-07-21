using HarmonyLib;

namespace Distance.LevelSelectAdditions.Harmony
{
	/// <summary>
	/// Patch to update the <see cref="LevelSelectMenuLogic.LevelEntry.myWorkshopVoteIndex_"/> field after rating a level.
	/// This is needed to ensure the level select list updates it's displayed vote, and so that we can grab the current vote
	/// for the level and display it on our rate button.
	/// </summary>
	[HarmonyPatch(typeof(LevelSelectMenuLogic), nameof(LevelSelectMenuLogic.OnRateLevelPanelPop))]
	internal static class LevelSelectMenuLogic__OnRateLevelPanelPop
	{
		[HarmonyPrefix]
		internal static void Prefix(LevelSelectMenuLogic __instance)
		{
			// This needs to be a prefix so that the displayed info is updated before `ReportEntryChanged` is called.

			var entry = __instance.selectedEntry_;
			if (entry != null && entry.ugcLevelData_ != null)
			{
				// IT'S REFLECTION TIME!
				// Performance isn't a concern for this usage, since this function is only called after user input from the rate level prompt.

				// Steamworks is located in `Assembly-CSharp-firstpass.dll`, so we don't have access to its types and methods/properties using those types.
				// So our goal is to update entry.myWorkshopVoteIndex_ to match the vote change performed by the Rate Level prompt.
				// TODO: In the future, the GSL may add `Assembly-CSharp-firstpass.dll` as a dependency. So eventually we could change this
				//        (assuming all other mods follow suit and update to the latest GSL version, which sounds like a nightmare to achieve).

				//public static int WorkshopVoteIndex(this EWorkshopVote vote);
				var method_WorkshopVoteIndex = typeof(EWorkshopVoteEx).GetMethod("WorkshopVoteIndex");
				//public EWorkshopVote WorkshopVote_ { get; set; }
				var getter_WorkshopVote_ = typeof(WorkshopLevelInfo).GetProperty("WorkshopVote_").GetGetMethod();

				int newVoteIndex = (int)method_WorkshopVoteIndex.Invoke(null, new object[] { getter_WorkshopVote_.Invoke(entry.ugcLevelData_, null) });

				if (newVoteIndex != entry.myWorkshopVoteIndex_)
				{
					entry.myWorkshopVoteIndex_ = newVoteIndex;
					// Any extra handling here if the vote was changed.
				}
			}
		}
	}
}
