using Distance.LevelSelectAdditions.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Distance.LevelSelectAdditions.Scripts
{
	public class LevelPlaylistCompoundData : MonoBehaviour
	{
		public LevelPlaylist Playlist { get; internal set; }

		/// <summary>
		/// Can be null.
		/// </summary>
		public LevelGridMenu.PlaylistEntry PlaylistEntry { get; internal set; }

		/// <summary>
		/// Absolute file path of the playlist when loaded, or assigned when saving.
		/// </summary>
		public string FilePath { get; internal set; }

		/// <summary>
		/// Game Mode assigned to official/community level sets during creation.
		/// </summary>
		public GameModeID CustomGameModeID { get; internal set; }

		/// <summary>
		/// Flags assigned during creation that determine special playlist types.
		/// </summary>
		public LevelGroupFlags LevelGroupFlags { get; internal set; }

		/// <summary>
		/// Type of playlist determined by the <see cref="LevelGridMenu.PlaylistEntry"/>.
		/// </summary>
		public LevelGridMenu.PlaylistEntry.Type PlaylistType => PlaylistEntry?.type_ ?? ((LevelGridMenu.PlaylistEntry.Type)(-1));

		/*/// <summary>
		/// Backup of the unsorted playlist order and entries, without limits being put in-place.
		/// </summary>
		public List<LevelPlaylist.ModeAndLevelInfo> OriginalList { get; internal set; }*/
	}
}
