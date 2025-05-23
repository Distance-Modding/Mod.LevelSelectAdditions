using Distance.LevelSelectAdditions.Extensions;
using Distance.LevelSelectAdditions.Scripts;
using HarmonyLib;
using Serializers;
using System.IO;
using UnityEngine;

namespace Distance.LevelSelectAdditions.Patches
{
	/// <summary>
	/// Patch to handle the auto-assigning of a LevelPlaylist's file path when saving.
	/// <para/>
	/// Now the file path will respect <see cref="LevelPlaylistCompoundData.FilePath"/> if available.
	/// And otherwise will strip NGUI symbols from the playlist name before using it as a filename.
	/// </summary>
	/// <remarks>
	/// Required For: QUICK PLAYLIST Rename button, Level Set Options menu Rename/Recolor buttons.
	/// </remarks>
	[HarmonyPatch(typeof(LevelPlaylist), nameof(LevelPlaylist.Save))]
	internal static class LevelPlaylist__Save
	{
		[HarmonyPrefix]
		internal static bool Prefix(LevelPlaylist __instance)
		{
			string filePath = __instance.GenerateFilePath(true);

			/*string filePath;

			var playlistData = __instance.GetComponent<LevelPlaylistCompoundData>();
			if (playlistData && playlistData.FilePath != null)
			{
				filePath = playlistData.FilePath;
			}
			else
			{
				string validFileName = NGUIText.StripSymbols(__instance.playlistName_);
				validFileName = Resource.GetValidFileName(validFileName, string.Empty);
				validFileName = validFileName.Trim().TrimEnd('.');
				//validFileName = Path.ChangeExtension(validFileName, ".xml");
				filePath = Resource.PersonalLevelPlaylistsDirPath_ + validFileName + ".xml";

				// Store our filepath in the compound data for future use.
				playlistData = __instance.GetOrAddComponent<LevelPlaylistCompoundData>();
				if (playlistData)
				{
					playlistData.FilePath = filePath;
				}
			}*/

			XmlSerializer.SaveGameObjectFile(filePath, __instance.gameObject, __instance.playlistPrefab_);

			return false;
		}
	}
}