using Distance.LevelSelectAdditions.Extensions;
using Distance.LevelSelectAdditions.Scripts;
using Events;

namespace Distance.LevelSelectAdditions.Events
{
	public class PlaylistFileRenamed : StaticEvent<PlaylistFileRenamed.Data>
	{
		public struct Data
		{
			public LevelPlaylist playlist;

			public string oldFilePath;

			public string oldLevelSetID;

			public string newFilePath;

			public string newLevelSetID;

			public Data(LevelPlaylist playlist, string oldFilePath, string oldLevelSetID)
			{
				this.playlist = playlist;
				this.oldFilePath = oldFilePath;
				this.oldLevelSetID = oldLevelSetID;
				this.newFilePath = playlist.GetComponent<LevelPlaylistCompoundData>().FilePath;
				this.newLevelSetID = playlist.GetLevelSetID();
			}
		}
	}
}
