using Distance.LevelSelectAdditions.Extensions;
using Events;
using UnityEngine;

namespace Distance.LevelSelectAdditions.Events
{
	public class PlaylistColorChanged : StaticEvent<PlaylistColorChanged.Data>
	{
		public struct Data
		{
			public LevelPlaylist playlist;

			public string levelSetID;

			public Color? oldColor;

			public string oldNameColored;

			public Color? newColor;

			public string newNameColored;

			public Data(LevelPlaylist playlist, Color? oldColor, string oldNameColored, Color? newColor)
			{
				this.playlist = playlist;
				this.levelSetID = playlist.GetLevelSetID();
				this.oldColor = oldColor;
				this.oldNameColored = oldNameColored;
				this.newColor = newColor;
				this.newNameColored = playlist.Name_;
			}
		}
	}
}
