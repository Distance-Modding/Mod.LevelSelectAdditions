using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;

//using SortingMethod = LevelSelectMenuLogic.SortingMethod;

namespace Distance.NoWorkshopGridLimit.Harmony
{
	[HarmonyPatch(typeof(LevelGridMenu), nameof(LevelGridMenu.CreateAndAddLevelSet))]
	internal static class LevelGridMenu__CreateAndAddLevelSet
	{
		internal static bool Prefix(LevelGridMenu __instance, LevelSet set, string name, LevelGridMenu.PlaylistEntry.Type type, LevelGroupFlags flags)
		{
			if (type == LevelGridMenu.PlaylistEntry.Type.Workshop)
			{
				Mod.Instance.Logger.Info($"No Workshop Grid Limit: LevelGridMenu__CreateAndAddLevelSet.Prefix(): type = {type}");//, __runOriginal = {__runOriginal}");

				try
				{
					LevelPlaylist levelPlaylist = LevelPlaylist.Create(set, name, flags);
					LevelGridMenu.PlaylistEntry.UnlockStyle unlockStyle = LevelGridMenu.PlaylistEntry.UnlockStyle.None;
					//levelPlaylist.Playlist_.Sort()

					// Filter out levels in personal playlists, if the user has specified this.
					if (Mod.Instance.Config.HideWorkshopLevelsInPlaylist)
					{
						Mod.Instance.Logger.Info("Filtering with HideWorkshopLevelsInPlaylist...");

						if (DirectoryEx.Exists(Resource.PersonalLevelPlaylistsDirPath_))
						{

							HashSet<string> levelsInPersonalPlaylists = new HashSet<string>();

							List<string> filePathsInDirWithPattern = Resource.GetFilePathsInDirWithPattern(Resource.PersonalLevelPlaylistsDirPath_, "*.xml", null);
							filePathsInDirWithPattern.RemoveAll((string s) => !Resource.FileExist(s));
							foreach (string absolutePath in filePathsInDirWithPattern)
							{
								LevelPlaylist personalPlaylist = LevelGridMenu.LoadPlaylist(absolutePath);
								if (!levelPlaylist)
								{
									Mod.Instance.Logger.Info("Failed to load: " + absolutePath);
									continue;
								}
								if (personalPlaylist.Count_ == 0)
								{
									continue;
								}
								if (personalPlaylist.FirstModeID_ != __instance.modeID_)
								{
									continue;
								}

								for (int i = 0; i < personalPlaylist.Count_; i++)
								{
									levelsInPersonalPlaylists.Add(personalPlaylist.GetLevelNameAndPathPairAtIndex(i).levelPath_);
								}
								/*foreach (LevelPlaylist.ModeAndLevelInfo modeAndLevelInfo in personalPlaylist.playlist_) {
									levelsInPersonalPlaylists.Add(modeAndLevelInfo.levelNameAndPath_.levelPath_);
								}*/
							}

							for (int i = 0; i < levelPlaylist.Count_; i++)
							{
								LevelNameAndPathPair levelNameAndPathPair = levelPlaylist.GetLevelNameAndPathPairAtIndex(i);

								// TODO: Is there any possibility that the Workshop playlist could mess up and have duplicates?
								//       If so, then we shouldn't remove items from the hashset.
								//if (levelsInPersonalPlaylists.Remove(levelNameAndPathPair.levelPath_)) {
								if (levelsInPersonalPlaylists.Contains(levelNameAndPathPair.levelPath_))
								{
									levelPlaylist.Remove(i);
									i--;
								}
							}
						}

					}

					// Apply limit after potentially removing levels that were in playlists.
					int limit = Mod.Instance.Config.WorkshopLevelLimit;
					if (limit != ConfigurationLogic.LevelLimit_Infinite && limit >= 0)
					{
						Mod.Instance.Logger.Info("Limiting with WorkshopLevelLimit...");
						levelPlaylist.LimitCountTo(limit); // original: 100
					}

					// Finally apply custom sorting.
					/*if (Mod.Instance.Config.WorkshopSortingMethod == SortingMethod.Recently_Downloaded) {
						Mod.Instance.Logger.Info("Sorting with with SortByMostRecentlyDownloaded...");
						levelPlaylist.Sort(new Comparison<LevelPlaylist.ModeAndLevelInfo>(__instance.SortByMostRecentlyDownloaded));
					}
					else
					{*/
					LevelSelectMenuLogic.SortingMethod defaultMethod = LevelSelectMenuLogic.SortingMethod.Level_Name;
					if (Mod.Instance.Config.WorkshopSortingMethod == LevelSelectMenuLogic.SortingMethod.Level_Name)
					{
						defaultMethod = LevelSelectMenuLogic.SortingMethod.Recently_Downloaded;
					}
					Mod.Instance.Logger.Info($"Sorting with with LevelSortHelper {Mod.Instance.Config.WorkshopSortingMethod}...");
					var helper = new LevelSortHelper(new LevelSelectMenuLogic.SortingMethod[]
						{
							Mod.Instance.Config.WorkshopSortingMethod,
							//defaultMethod,
							Mod.Instance.Config.WorkshopSortingMethod2,
							Mod.Instance.Config.WorkshopSortingMethod3,
						},
						new bool[]
						{
							//false, false,// false,
							Mod.Instance.Config.WorkshopReversedSortingMethod,
							Mod.Instance.Config.WorkshopReversedSortingMethod2,
							Mod.Instance.Config.WorkshopReversedSortingMethod3,
						});
					//var helper = new LevelSortHelper(Mod.Instance.Config.GetWorkshopSortingMethods(),
					//								 Mod.Instance.Config.GetWorkshopReversedSortingMethods());
					//								 // Mod.Instance.Config.WorkshopSortingMethod0, defaultMethod);
					helper.SortPlaylist(__instance, levelPlaylist);
					//}
					/*switch (Mod.Instance.Config.WorkshopSortingMethod) {
					case SortingMethod.Level_Name:
						levelPlaylist.Sort(new Comparison<LevelPlaylist.ModeAndLevelInfo>(helper.SortByLevelName));
						break;
					case SortingMethod.Author:
						levelPlaylist.Sort(new Comparison<LevelPlaylist.ModeAndLevelInfo>(helper.SortByAuthor));
						break;
					case SortingMethod.Difficulty:
						levelPlaylist.Sort(new Comparison<LevelPlaylist.ModeAndLevelInfo>(helper.SortByDifficulty));
						break;
					case SortingMethod.Finish_Status:
						levelPlaylist.Sort(new Comparison<LevelPlaylist.ModeAndLevelInfo>(helper.SortByFinishStatus));
						break;
					case SortingMethod.Medal_Earned:
						levelPlaylist.Sort(new Comparison<LevelPlaylist.ModeAndLevelInfo>(helper.SortByMedalEarned));
						break;

					case SortingMethod.Recently_Downloaded:
						levelPlaylist.Sort(new Comparison<LevelPlaylist.ModeAndLevelInfo>(__instance.SortByMostRecentlyDownloaded));
						break;

					default:
						goto case SortingMethod.Recently_Downloaded;
					}
					LevelSelectMenuLogic.LevelEntry.Create()
					__instance.levelSets_.GetLevelInfo
					LevelPlaylist.ModeAndLevelInfo m;
					m.
					if (sorting == SortingMethod.De)
						switch (Mod.Instance.Config.WorkshopSortingMethod) {
						case WorkshopSortMethod.Recent:
							levelPlaylist.Sort(new Comparison<LevelPlaylist.ModeAndLevelInfo>(__instance.SortByMostRecentlyDownloaded));
							break;
						}*/

					//levelPlaylist.Sort(new Comparison<LevelPlaylist.ModeAndLevelInfo>(__instance.SortByMostRecentlyDownloaded));

					Mod.Instance.Logger.Info("CreateAndAddEntry...");
					__instance.CreateAndAddEntry(levelPlaylist, type, true, unlockStyle, string.Empty);

					Mod.Instance.Logger.Info("Done!");
					return false;

				}
				catch (Exception ex)
				{
					Mod.Instance.Logger.Info("LevelGridMenu__CreateAndAddLevelSet.Prefix(): EXCEPTION!!");
					Mod.Instance.Logger.Exception(ex);
				}
			}
			return true;

			/*LevelPlaylist levelPlaylist = LevelPlaylist.Create(set, name, flags);
			LevelGridMenu.PlaylistEntry.UnlockStyle unlockStyle = LevelGridMenu.PlaylistEntry.UnlockStyle.None;
			if (type == LevelGridMenu.PlaylistEntry.Type.Official && __instance.displayType_ == LevelSelectMenuAbstract.DisplayType.ChooseMainMenuLevel) {
				unlockStyle = LevelGridMenu.PlaylistEntry.UnlockStyle.ChooseMainMenu;
			}
			else if (type == LevelGridMenu.PlaylistEntry.Type.Workshop) {
				levelPlaylist.Sort(new Comparison<LevelPlaylist.ModeAndLevelInfo>(__instance.SortByMostRecentlyDownloaded));
				//levelPlaylist.LimitCountTo(100);
			}
			__instance.CreateAndAddEntry(levelPlaylist, type, true, unlockStyle, string.Empty);
			return false;*/

		}

	}
}
