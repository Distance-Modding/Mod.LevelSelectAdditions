using Distance.LevelSelectAdditions.Extensions;
using Events;

namespace Distance.LevelSelectAdditions.Events
{
	public class PlaylistNameChanged : StaticEvent<PlaylistNameChanged.Data>
	{
		public struct Data
		{
			public LevelPlaylist playlist;

			public string levelSetID;

			public string oldNameColored;

			public string oldNameUncolored;

			public string newNameColored;

			public string newNameUncolored;

			public Data(LevelPlaylist playlist, string oldNameColored)
			{
				this.playlist = playlist;
				levelSetID = playlist.GetLevelSetID();
				this.oldNameColored = oldNameColored;
				this.oldNameColored.DecodeNGUIColor(out this.oldNameUncolored, out _, out _);
				newNameColored = playlist.Name_;
				newNameColored.DecodeNGUIColor(out this.newNameUncolored, out _, out _);
			}
		}
	}
}
