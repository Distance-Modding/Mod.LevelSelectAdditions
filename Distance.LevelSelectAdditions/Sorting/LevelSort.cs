using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using SortingMethod = LevelSelectMenuLogic.SortingMethod;

namespace Distance.LevelSelectAdditions.Sorting
{
	public class LevelSort
	{
		#region Sorting Method Constants

		public const SortingMethod None = (SortingMethod)(-1);
		//public const SortingMethod Shuffle = (SortingMethod)(-2);

		public const SortingMethod Level_Name = SortingMethod.Level_Name;
		public const SortingMethod Author = SortingMethod.Author;
		public const SortingMethod Recently_Downloaded = SortingMethod.Recently_Downloaded;
		//public const SortingMethod Recently_Published = SortingMethod.Recently_Published;
		//public const SortingMethod Recently_Updated = SortingMethod.Recently_Updated;
		//public const SortingMethod Recently_Played = SortingMethod.Recently_Played;
		public const SortingMethod Difficulty = SortingMethod.Difficulty;
		public const SortingMethod Finish_Status = SortingMethod.Finish_Status;
		public const SortingMethod Medal_Earned = SortingMethod.Medal_Earned;
		//public const SortingMethod Workshop_Rating = SortingMethod.Workshop_Rating;
		//public const SortingMethod My_Rating = SortingMethod.My_Rating;
		//public const SortingMethod Positive_Votes = SortingMethod.Positive_Votes;

		public static SortingMethod[] GetSupportedMethodsList()
		{
			return new SortingMethod[]
			{
				None,
				//Shuffle,

				Level_Name,
				Author,
				Recently_Downloaded,
				//Recently_Played,
				//Recently_Published,
				//Recently_Updated,
				Difficulty,
				Finish_Status,
				Medal_Earned,
				//Workshop_Rating,
				//My_Rating,
				//Positive_Votes,
			};
		}

		public static string GetMethodName(SortingMethod sortingMethod)
		{
			switch (sortingMethod)
			{
			case LevelSort.None:
				return nameof(None);

			//case LevelSort.Shuffle:
			//	return nameof(Shuffle);

			default:
				return sortingMethod.ToString().Replace('_', ' ');
			}
		}

		#endregion

		#region Fields

		// Condensed array of comparisons determined by SortingMethod[] and bool[] constructor arguments.
		private readonly Comparison<LevelSortEntry>[] comparisonMethods;
		private readonly bool[] reverseMethods;

		// Additional level info needed by sorting methods, used by SortPlaylist() when in LevelGridMenu.
		private Dictionary<LevelPlaylist.ModeAndLevelInfo, LevelSortEntry> playlistEntries;

		#endregion

		/// <summary>
		/// Create a sorter with layers of sorting methods. Where if an earlier method returns 0, the next will be used to sort.
		/// </summary>
		public LevelSort(SortingMethod[] sortingMethods, bool[] reverseMethods)
		{
			Debug.Assert(sortingMethods != null);
			Debug.Assert(reverseMethods == null || sortingMethods.Length == reverseMethods.Length);

			List<Comparison<LevelSortEntry>> comparisonList = sortingMethods.Select(x => GetComparison(x)).ToList();
			List<bool> reverseList = (reverseMethods ?? new bool[sortingMethods.Length]).ToList();

			for (int i = 0; i < comparisonList.Count; i++)
			{
				if (comparisonList[i] == null)
				{
					// Remove None/unsupported comparisons
					comparisonList.RemoveAt(i);
					reverseList.RemoveAt(i);
					i--;
				}
				else if (i > 0 && comparisonList[i - 1] == comparisonList[i] && reverseList[i - 1] == reverseList[i])
				{
					// Condense repeated comparison types
					comparisonList.RemoveAt(i);
					reverseList.RemoveAt(i);
					i--;
				}
			}

			comparisonMethods = comparisonList.ToArray();
			this.reverseMethods = reverseList.ToArray();
		}

		/// <summary>
		/// Code duplication: We need YET ANOTHER LevelEntry type, so that we can perform sorting without creating UI elements.
		/// </summary>
		public class LevelSortEntry
		{
			public LevelNameAndPathPair LevelNameAndPath { get; }
			public GameModeID ModeID { get; }
			public LevelInfo LevelInfo { get; }
			public string AuthorName { get;  }
			public ProfileProgress.LevelProgress LevelProgress { get; }
			public MedalStatus MedalStatus { get; }

			/// <summary>
			/// Construct a sort entry using information from the <see cref="LevelSelectMenuLogic"/>.
			/// </summary>
			public LevelSortEntry(LevelSelectMenuLogic levelSelectMenu, LevelNameAndPathPair levelNameAndPath)
			{
				LevelNameAndPath = levelNameAndPath;
				ModeID = levelSelectMenu.modeID_;
				LevelInfo = levelSelectMenu.levelSets_.GetLevelInfo(LevelNameAndPath.levelPath_);
				AuthorName = GetAuthorName(LevelInfo);

				LevelProgress = levelSelectMenu.currentProfile_?.progress_?.GetLevelProgress(LevelNameAndPath.levelPath_);
				MedalStatus = MedalStatus.None;
				if (LevelProgress != null)
				{
					MedalStatus = LevelProgress.GetMedal(ModeID);
				}
			}

			/// <summary>
			/// Construct a sort entry using information from the <see cref="LevelGridMenu"/>.
			/// </summary>
			public LevelSortEntry(LevelGridMenu levelGridMenu, LevelPlaylist.ModeAndLevelInfo modeAndLevelInfo)
			{
				LevelNameAndPath = modeAndLevelInfo.levelNameAndPath_;
				ModeID = levelGridMenu.modeID_; // modeAndLevelInfo.mode_;
				LevelInfo = levelGridMenu.levelSets_.GetLevelInfo(LevelNameAndPath.levelPath_);
				AuthorName = GetAuthorName(LevelInfo);

				LevelProgress = levelGridMenu.profileProgress_?.GetLevelProgress(LevelNameAndPath.levelPath_);
				MedalStatus = MedalStatus.None;
				if (LevelProgress != null)
				{
					MedalStatus = LevelProgress.GetMedal(ModeID);
				}
			}
		}

		private static Comparison<LevelSortEntry> GetComparison(SortingMethod sortingMethod)
		{
			switch (sortingMethod)
			{
			case SortingMethod.Level_Name:
				return CompareLevelName;

			case SortingMethod.Author:
				return CompareAuthor;

			case SortingMethod.Difficulty:
				return CompareDifficulty;

			case SortingMethod.Finish_Status:
				return CompareFinishStatus;

			case SortingMethod.Medal_Earned:
				return CompareMedalEarned;

			case SortingMethod.Recently_Downloaded:
				return CompareMostRecentlyDownloaded;

			// Not supported yet
			//case LevelSort.Shuffle:
			//	return CompareShuffle;

			case LevelSort.None:
				return null;

			default:
				return null; // Not supported
			}
		}

		/// <summary>
		/// Code duplication: <see cref="LevelGridGrid.LevelEntry.GetAuthorName"/>
		/// </summary>
		internal static string GetAuthorName(LevelInfo levelInfo)
		{
			string authorName = SteamworksManager.defaultUserName_;
			if (levelInfo.levelType_ == LevelType.Official)
			{
				authorName = "Refract";
			}
			if (levelInfo.levelType_ == LevelType.Community)
			{
				authorName = (string.IsNullOrEmpty(levelInfo.levelCreatorName_) ? "Community Creator" : levelInfo.levelCreatorName_);
			}
			else if (levelInfo.levelType_ == LevelType.Workshop)
			{
				authorName = SteamworksManager.GetSteamName(levelInfo.workshopCreatorID_);
			}
			return authorName;
		}

		#region Sort

		/// <summary>
		/// Perform sorting for playlists by grabbing additional information from the <see cref="LevelGridMenu"/>.
		/// </summary>
		public void SortPlaylist(LevelGridMenu levelGridMenu, LevelPlaylist playlist)
		{
			// Map levels to entries with extra information needed for most sorting methods.
			// This info could also be grabbed on the fly, but it would be slower if you have
			//  thousands of workshop levels or something.
			playlistEntries = new Dictionary<LevelPlaylist.ModeAndLevelInfo, LevelSortEntry>();
			foreach (LevelPlaylist.ModeAndLevelInfo modeAndLevelInfo in playlist.playlist_)
			{
				playlistEntries.Add(modeAndLevelInfo, new LevelSortEntry(levelGridMenu, modeAndLevelInfo));
			}
			/*this.entries = new Dictionary<string, LevelSortEntry>();
			foreach (LevelPlaylist.ModeAndLevelInfo modeAndLevelInfo in playlist.playlist_)
			{
				this.entries.Add(modeAndLevelInfo.levelNameAndPath_.levelPath_, new LevelSortEntry(levelGridMenu, modeAndLevelInfo));
				//this.entries.Add(modeAndLevelInfo, new LevelSortEntry(levelGridMenu, modeAndLevelInfo));
			}*/

			playlist.Sort(ComparePlaylist);
		}

		/// <summary>
		/// Perform comparisons for <see cref="LevelGridMenu"/> playlists.
		/// </summary>
		private int ComparePlaylist(LevelPlaylist.ModeAndLevelInfo x, LevelPlaylist.ModeAndLevelInfo y)
		{
			// Get our entries containing needed additional information.
			LevelSortEntry x_entry = playlistEntries[x];
			LevelSortEntry y_entry = playlistEntries[y];
			//LevelSortEntry x_entry = this.entries[x.levelNameAndPath_.levelPath_];
			//LevelSortEntry y_entry = this.entries[y.levelNameAndPath_.levelPath_];

			// Keep applying comparisons down the list, until one returns a difference.
			int result = 0;
			for (int i = 0; result == 0 && i < comparisonMethods.Length; i++)
			{
				result = comparisonMethods[i](x_entry, y_entry);
				if (reverseMethods[i])
				{
					result = -result;
				}
			}

			return result;
		}

		#endregion

		#region Comparisons

		/// <summary>
		/// Code duplication: <see cref="LevelSelectMenuLogic.LevelNameSorter.SortVirtual"/>
		/// </summary>
		public static int CompareLevelName(LevelSortEntry x, LevelSortEntry y)
		{
			return LevelSelectMenuLogic.CompareAlphabeticalWithSpecialCharLast(x.LevelInfo.levelName_, y.LevelInfo.levelName_);
		}

		/// <summary>
		/// Code duplication: <see cref="LevelSelectMenuLogic.AuthorSorter.SortVirtual"/>
		/// </summary>
		/// <seealso cref="LevelGridGrid.LevelEntry.GetAuthorName"/>
		public static int CompareAuthor(LevelSortEntry x, LevelSortEntry y)
		{
			return LevelSelectMenuLogic.CompareAlphabeticalWithSpecialCharLast(x.AuthorName, y.AuthorName);
		}

		/// <summary>
		/// Code duplication: <see cref="LevelSelectMenuLogic.DifficultySorter.SortVirtual"/>
		/// </summary>
		public static int CompareDifficulty(LevelSortEntry x, LevelSortEntry y)
		{
			return x.LevelInfo.difficulty_.CompareTo(y.LevelInfo.difficulty_);
		}

		/// <summary>
		/// Code duplication: <see cref="LevelSelectMenuLogic.LevelEntry.ComparePlayedStatus"/>
		/// <para/>
		/// CHANGE: <see cref="MedalStatus.None"/> is treated as <see cref="MedalStatus.Did_Not_Finish"/>.
		/// </summary>
		public static int CompareFinishStatus(LevelSortEntry x, LevelSortEntry y)
		{
			// Treat MedalStatus.None as MedalStatus.Did_Not_Finish.
			bool x_notFinished = x.MedalStatus <= MedalStatus.Did_Not_Finish;
			bool y_notFinished = y.MedalStatus <= MedalStatus.Did_Not_Finish;

			if (x_notFinished == y_notFinished)
			{
				return 0;
			}
			else if (!x_notFinished)
			{
				return -1;
			}
			else //if (!y_notFinished)
			{
				return 1;
			}
		}

		/// <summary>
		/// Code duplication: <see cref="LevelSelectMenuLogic.MedalEarnedSorter.SortVirtual"/>
		/// <para/>
		/// CHANGE: <see cref="MedalStatus.Did_Not_Finish"/> is treated as the lower than <see cref="MedalStatus.Completed"/>.
		/// </summary>
		public static int CompareMedalEarned(LevelSortEntry x, LevelSortEntry y)
		{
			MedalStatus x_medalStatus = x.MedalStatus;
			MedalStatus y_medalStatus = y.MedalStatus;

			// Treat MedalStatus.None as MedalStatus.Did_Not_Finish.
			if (x_medalStatus < MedalStatus.Did_Not_Finish)
			{
				x_medalStatus = MedalStatus.Did_Not_Finish;
			}
			if (y_medalStatus < MedalStatus.Did_Not_Finish)
			{
				y_medalStatus = MedalStatus.Did_Not_Finish;
			}

			return y_medalStatus.CompareTo(x_medalStatus);
		}

		/// <summary>
		/// Code duplication: <see cref="LevelGridMenu.SortByMostRecentlyDownloaded"/>
		/// </summary>
		public static int CompareMostRecentlyDownloaded(LevelSortEntry x, LevelSortEntry y)
		{
			return y.LevelInfo.fileLastWriteDateTime_.CompareTo(x.LevelInfo.fileLastWriteDateTime_);
		}

		#endregion
	}
}
