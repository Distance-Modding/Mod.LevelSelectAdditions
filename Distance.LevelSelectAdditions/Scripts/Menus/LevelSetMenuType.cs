using Distance.LevelSelectAdditions.Extensions;

namespace Distance.LevelSelectAdditions.Scripts.Menus
{
	public enum LevelSetMenuType
	{
		Basic, // Built-in playlists
		Special, // Workshop / Personal (generated at runtime)
		Playlist, // User-created playlists

		Count,
	}

	public static class LevelSetMenuTypeExtensions
	{
		public static LevelSetMenuType GetLevelSetMenuType(this LevelPlaylist playlist)
		{
			if (playlist == null || (playlist.IsResourcesPlaylist() && !playlist.IsSpecialLevelSet()))
			{
				return LevelSetMenuType.Basic;
			}
			else if (playlist.IsSpecialLevelSet())
			{
				return LevelSetMenuType.Special;
			}
			else
			{
				return LevelSetMenuType.Playlist;
			}
		}
	}
}
