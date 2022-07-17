using Events;

namespace Distance.LevelSelectAdditions.Events
{
	public class PlaylistFileDeleted : StaticEvent<PlaylistFileDeleted.Data>
	{
		public struct Data
		{
			public string filePath;

			public string levelSetID;

			public string name;

			public Data(string oldFilePath, string oldLevelSetID, string name)
			{
				this.filePath = oldFilePath;
				this.levelSetID = oldLevelSetID;
				this.name = oldLevelSetID;
			}
		}
	}
}
