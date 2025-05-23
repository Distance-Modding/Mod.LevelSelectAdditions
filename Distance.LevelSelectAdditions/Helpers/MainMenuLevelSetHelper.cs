using Distance.LevelSelectAdditions.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace Distance.LevelSelectAdditions.Helpers
{
	public static class MainMenuLevelSetHelper
	{
		private static bool StartupLevelChosen { get; set; } = false;

		public static LevelPlaylist LoadFromRelativePathID(string relativePathID)
		{
			string pathOrName = relativePathID.RelativePathIDToAbsolutePath(out bool isName);
			if (isName)
			{
				var set = G.Sys.LevelSets_.GetSet(GameModeID.MainMenu);
				switch (pathOrName)
				{
				case "Official":
					return LevelPlaylist.Create(set, pathOrName, LevelGroupFlags.Resource | LevelGroupFlags.Community);
				case "Personal":
					return LevelPlaylist.Create(set, pathOrName, LevelGroupFlags.MyLevels | LevelGroupFlags.Levels);
				case "Workshop":
					return LevelPlaylist.Create(set, pathOrName, LevelGroupFlags.Workshop);
				}
			}
			else
			{
				var gameObject = LevelPlaylist.Load(pathOrName);
				return gameObject.GetComponent<LevelPlaylist>();
			}
			return null;
		}

		// Returns true if the main menu level set was changed (or removed).
		public static bool SetMainMenuLevelSet(LevelPlaylist playlist)
		{
			string relativePathID = null;
			if (playlist != null)
			{
				relativePathID = playlist.GetRelativePathID();
			}
			Profile profile = G.Sys.ProfileManager_.CurrentProfile_;
			if (Mod.Instance.SetProfileMainMenuRelativePathID(profile.Name_, relativePathID))
			{
				Mod.Instance.SetStateLastMainMenuLevelRelativePath(profile.Name_, null);
				profile.MainMenuLevelRelativePath_ = null; // Force main menu to reload by claiming it was changed.
				if (playlist != null)
				{
					ChooseNextMainMenu(force: true);
				}
				return true;
			}
			return false;
		}

		public static void ChooseNextMainMenu(bool force = false)
		{
			Profile profile = G.Sys.ProfileManager_.CurrentProfile_;
			if (profile == null)
			{
				return;
			}

			string lastLevel = Mod.Instance.GetStateLastMainMenuLevelRelativePath(profile.Name_);
			if (force || lastLevel == null || (!StartupLevelChosen || !Mod.RandomStartupMainMenu.Value))
			{
				string relativePathID = Mod.Instance.GetProfileMainMenuRelativePathID(profile.Name_);
				if (relativePathID != null)
				{
					LevelPlaylist levelPlaylist = LoadFromRelativePathID(relativePathID);
					if (levelPlaylist != null)
					{
						List<LevelInfo> levels = levelPlaylist.Playlist_.Select((x) => G.Sys.LevelSets_.GetLevelInfo(x.levelNameAndPath_.levelPath_))
																		.Where((x) => x.modes_.ContainsKey((int)GameModeID.MainMenu))
																		.ToList();
						if (levels.Count > 0)
						{
							StartupLevelChosen = true;
							// TODO: Avoid duplicate of last chosen main menu level?

							var rng = new System.Random();
							var level = levels[rng.Next(0, levels.Count)];

							profile.MainMenuLevelRelativePath_ = level.relativePath_;
							Mod.Instance.SetStateLastMainMenuLevelRelativePath(profile.Name_, level.relativePath_);
						}
					}
				}
			}
		}
	}
}
