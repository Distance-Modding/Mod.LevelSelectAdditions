using System;
using System.Collections.Generic;
using System.Linq;

namespace Distance.LevelSelectAdditions
{
	public class LevelFilter
	{
		#region Limit Constants

		public const int Infinite = -1;

		public static int[] GetArbitraryLimitsList()
		{
			return new int[]
			{
				Infinite,
				25,
				50,
				100,
				250,
				500,
				750,
				1000,
				1500,
				2000,
				// Anything higher may as well be infinite...
			};
		}

		public static string GetLimitName(int limit)
		{
			switch (limit)
			{
			case Infinite:
				return nameof(Infinite);

			default:
				return limit.ToString();
			}
		}

		#endregion

		public static void LimitLevels(LevelPlaylist levelPlaylist, int limit)
		{
			if (limit != Infinite && limit >= 0)
			{
				levelPlaylist.LimitCountTo(limit);
			}
		}

		public static void ExcludeLevelsInPersonalPlaylists(LevelPlaylist levelPlaylist, GameModeID modeID)
		{
			if (DirectoryEx.Exists(Resource.PersonalLevelPlaylistsDirPath_))
			{
				// Create a list of all levels found in playlists.
				// NOTE: This does not perform any normalization. This is expected to already be handled by the LevelPlaylist xml file.
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
					if (personalPlaylist.FirstModeID_ != modeID)
					{
						continue;
					}

					for (int i = 0; i < personalPlaylist.Count_; i++)
					{
						levelsInPersonalPlaylists.Add(personalPlaylist.GetLevelNameAndPathPairAtIndex(i).levelPath_);
					}
				}

				for (int i = 0; i < levelPlaylist.Count_; i++)
				{
					LevelNameAndPathPair levelNameAndPathPair = levelPlaylist.GetLevelNameAndPathPairAtIndex(i);

					// TODO: Is there any possibility that the Workshop playlist could mess up and have duplicates?
					//       If so, then we shouldn't remove items from the hashset.
					if (levelsInPersonalPlaylists.Remove(levelNameAndPathPair.levelPath_))
					//if (levelsInPersonalPlaylists.Contains(levelNameAndPathPair.levelPath_))
					{
						levelPlaylist.Remove(i);
						i--;
					}
				}
			}
		}

	}
}
