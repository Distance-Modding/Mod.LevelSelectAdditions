using DistanceModConfigurationManager.Game;
using Distance.LevelSelectAdditions.Events;
using Distance.LevelSelectAdditions.Helpers;
using Distance.LevelSelectAdditions.Scripts;
using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Distance.LevelSelectAdditions.Extensions
{
	public static class LevelPlaylistExtensions
	{
		#region Constants

		public static readonly Color BasePersonalColor = GConstants.myLevelColor_;

		#endregion

		#region Playlist Attributes

		public static bool IsResourcesPlaylist(this LevelPlaylist playlist)
		{
			var playlistData = playlist.GetComponent<LevelPlaylistCompoundData>();
			if (!playlistData || playlistData.FilePath == null)
			{
				return true;
			}
			string path = new FileInfo(playlistData.FilePath).FullName.UniformPathSeparators();
			string resourcesPath = new DirectoryInfo(Path.Combine(Application.dataPath, "Resources")).FullName.UniformPathSeparatorsTrimmed() + "/";

			return path.StartsWith(resourcesPath, StringComparison.InvariantCultureIgnoreCase);
		}

		public static bool IsSpecialLevelSet(this LevelPlaylist playlist)
		{
			var playlistData = playlist.GetComponent<LevelPlaylistCompoundData>();
			if (!playlistData)
			{
				return false;
			}
			// NOTE: IsSet returns true if ANY bit of the passed flags is set.
			return playlistData.LevelGroupFlags.IsSet(LevelGroupFlags.MyLevels | LevelGroupFlags.Workshop);
		}

		public static bool IsWorkshopLevelSet(this LevelPlaylist playlist)
		{
			var playlistData = playlist.GetComponent<LevelPlaylistCompoundData>();
			if (!playlistData)
			{
				return false;
			}
			return playlistData.LevelGroupFlags.IsSet(LevelGroupFlags.Workshop);
		}

		public static bool IsPersonalLevelSet(this LevelPlaylist playlist)
		{
			var playlistData = playlist.GetComponent<LevelPlaylistCompoundData>();
			if (!playlistData)
			{
				return false;
			}
			return playlistData.LevelGroupFlags.IsSet(LevelGroupFlags.MyLevels);
		}

		#endregion

		#region Level Set ID

		public const string IDPrefixSeparator = "::";
		public const string IDPrefixResources = "resources" + IDPrefixSeparator;
		public const string IDPrefixPersonal  = "personal" + IDPrefixSeparator;
		public const string IDPrefixPath      = "path" + IDPrefixSeparator;
		public const string IDPrefixName      = "name" + IDPrefixSeparator;


		public static string GetLevelSetID(this LevelPlaylist playlist, bool relativePath = false)
		{
			string result;

			var playlistData = playlist.GetComponent<LevelPlaylistCompoundData>();
			if (playlistData && playlistData.FilePath != null)
			{
				string path = new FileInfo(playlistData.FilePath).FullName.UniformPathSeparators();
				string resourcesPath = new DirectoryInfo(Path.Combine(Application.dataPath, "Resources")).FullName.UniformPathSeparatorsTrimmed() + "/";
				string personalPath = new DirectoryInfo(Resource.personalDistanceDirPath_).FullName.UniformPathSeparatorsTrimmed() + "/";

				// Only assign if we have a FilePath to determine with,
				//  otherwise assume its a resource playlist until a FileName may be assigned.
				if (path.StartsWith(resourcesPath, StringComparison.InvariantCultureIgnoreCase))
				{
					result = IDPrefixResources + path.Substring(resourcesPath.Length/* + 1*/);
				}
				else if (path.StartsWith(personalPath, StringComparison.InvariantCultureIgnoreCase))
				{
					result = IDPrefixPersonal + path.Substring(personalPath.Length/* + 1*/);
				}
				else
				{
					result = IDPrefixPath + path;
				}
			}
			else
			{
				result = IDPrefixName + playlist.Name_;
			}

			if (!relativePath)
			{
				result = result.ToLowerInvariant();
			}
			return result;
		}

		public static string GetRelativePathID(this LevelPlaylist playlist) => playlist.GetLevelSetID(true);

		public static string RelativePathIDToAbsolutePath(this string relativePathID, out bool isName)
		{
			string[] parts = relativePathID.Split(new string[] { IDPrefixSeparator }, 2, StringSplitOptions.None);
			if (parts.Length == 2)
			{
				string prefix = parts[0] + IDPrefixSeparator;
				string relativePath = parts[parts.Length - 1];

				switch (prefix)
				{
				case IDPrefixResources:
					isName = false;
					return Path.Combine(new DirectoryInfo(Path.Combine(Application.dataPath, "Resources")).FullName, relativePath);

				case IDPrefixPersonal:
					isName = false;
					return Path.Combine(new DirectoryInfo(Resource.personalDistanceDirPath_).FullName, relativePath);

				case IDPrefixPath:
					isName = false;
					return relativePath;

				case IDPrefixName:
					isName = true;
					return relativePath;
				}
			}

			isName = false;
			return null;
		}

		/*public static bool IsLevelSetID(this LevelPlaylist playlist, string otherLevelSetID)
		{
			return playlist.GetLevelSetID() == otherLevelSetID;
			//string levelSetID = playlist.GetLevelSetID();
			//return levelSetID.Equals(otherLevelSetID, StringComparison.InvariantCultureIgnoreCase);

			#if false
			var playlistData = playlist.GetComponent<LevelPlaylistCompoundData>();
			if (!playlistData)
			{
				return playlist.Name_.Equals(otherLevelSetID, StringComparison.InvariantCultureIgnoreCase);
			}
			//return playlistData.IsLevelSetID(otherLevelSetID);
			string myPathName = this.GetLevelSetID();
			string myPrefix = GetLevelSetIDPrefix(myPathName);

			string otherPrefix = GetLevelSetIDPrefix(otherPathName);

			if (myPrefix != otherPrefix)
			{
				return false;
			}

			return myPathName.Substring(myPrefix.Length).Equals(otherPathName.Substring(otherPrefix.Length), StringComparison.InvariantCultureIgnoreCase);
			#endif
		}*/

		public static string GetLevelSetIDPrefix(string levelSetID)
		{
			string[] parts = levelSetID.Split(new string[] { IDPrefixSeparator }, 2, StringSplitOptions.None);
			if (parts.Length == 2)
			{
				return parts[0] + IDPrefixSeparator;
			}
			return parts[0];
		}

		#endregion

		#region Personal Display Name

		public static string GetUncoloredName(this LevelPlaylist playlist)
		{
			playlist.Name_.DecodeNGUIColor(out string name, out _, out _);
			return name; // Stripped is unmodified input source if no color tag or hex.
		}

		// Returns true if color has a value.
		public static bool GetColor(this LevelPlaylist playlist, out bool colorTag, out Color color, bool multiplyBaseColor)
		{
			bool hasColor = playlist.Name_.DecodeNGUIColor(out _, out colorTag, out color);
			if (multiplyBaseColor && hasColor && !colorTag)
			{
				color *= BasePersonalColor;
			}
			return hasColor;
		}

		// Returns true if color has a value.
		public static bool GetNameAndColor(this LevelPlaylist playlist, out string name, out bool colorTag, out Color color, bool multiplyBaseColor)
		{
			bool hasColor = playlist.Name_.DecodeNGUIColor(out name, out colorTag, out color);
			if (multiplyBaseColor && hasColor && !colorTag)
			{
				color *= BasePersonalColor;
			}
			return hasColor;
		}

		// Gets the base color to use for the label (this base color is used to preserve vanilla playlist colors that don't use the `[c][/c]` tag).
		// If alwaysUseBaseColor is true, then the base playlist color is always returned, even if the name is uncolored.
		//  (AKA `BasePersonalColor` will always be output if the `[c][/c]` tag is not used.)
		// Returns true if the playlist name is colored.
		public static bool GetBaseColor(this LevelPlaylist playlist, out Color baseColor, bool alwaysUseBaseColor)
		{
			bool hasColor = playlist.GetColor(out bool colorTag, out _, false);
			if ((hasColor || alwaysUseBaseColor) && !colorTag)
			{
				baseColor = BasePersonalColor;
			}
			else
			{
				baseColor = Color.white; // Default color with `[c][/c]` tag, or when no color is used.
			}
			return hasColor;
		}

		#endregion

		#region File Path

		public static string GetFilePath(this LevelPlaylist playlist)
		{
			var playlistData = playlist.GetComponent<LevelPlaylistCompoundData>();
			if (!playlistData)
			{
				return null;
			}
			return playlistData.FilePath;
		}

		public static string GenerateFilePath(this LevelPlaylist playlist, bool assignPath)
		{
			var playlistData = playlist.GetComponent<LevelPlaylistCompoundData>();
			if (playlistData && playlistData.FilePath != null)
			{
				return playlistData.FilePath;
			}
			else
			{
				string validFileName = NGUIText.StripSymbols(playlist.Name_);
				validFileName = Resource.GetValidFileName(validFileName, string.Empty);
				validFileName = validFileName.Trim().TrimEnd('.');
				validFileName = Sanitizer.SubstituteReservedName(validFileName, "{0}_");
				if (validFileName.Length == 0)
				{
					validFileName = "_";
				}
				//validFileName = Path.ChangeExtension(validFileName, ".xml");
				string filePath = Resource.PersonalLevelPlaylistsDirPath_ + validFileName + ".xml";

				// Store our filepath in the compound data for future use.
				playlistData = playlist.gameObject.GetOrAddComponent<LevelPlaylistCompoundData>();
				if (assignPath)
				{
					playlistData.FilePath = filePath;
					playlistData.Playlist = playlist;
				}
				return filePath;
			}
		}

		#endregion

		#region Change Information

		public static bool Rename(this LevelPlaylist playlist, string newName, bool autoSave)
		{
			try
			{
				string origName = playlist.Name_;

				bool hasColor = playlist.GetColor(out bool colorTag, out Color color, false);


				// If the user has put other formatting/color symbols inside the name,
				//  then we need to try and preserve the [c] color tag usage if possible.
				//bool hasSymbols = NGUIText.StripSymbols(newName).Length != newName.Length;

				if (hasColor && colorTag)
				{
					/*if (!hasSymbols && color.Color32Equals(BasePersonalColor))
					{
						playlist.Name_ = newName;//.EncodeNGUIColorHex(Color.white); // [FFFFFF]{newName}[-]
					}
					else if (color.TryGetBaseColorFromMultiplier(BasePersonalColor, out Color baseColor))
					{
						// We can lose the color tag, because the current name color naturally supports the myLevelColor_ multiplier.
						playlist.Name_ = newName.EncodeNGUIColorHex(baseColor); // [RRGGBB(AA)]{newName}[-]
					}
					else*/
					{
						playlist.Name_ = newName.EncodeNGUIColor(color); // [c][RRGGBB(AA)]{newName}[-][/c]
					}
				}
				else if (hasColor && !colorTag)
				{
					playlist.Name_ = newName.EncodeNGUIColorHex(color); // [RRGGBB(AA)]{newName}[-]
				}
				else if (colorTag)
				{
					playlist.Name_ = newName.EncodeNGUIColorTag(); // [c]{newName}[/c]
				}
				else
				{
					playlist.Name_ = newName;
				}

				//this.levelSelectMenu_.quickPlaylistLabel_.text = msg;

				if (playlist.Name_ != origName)
				{
					Mod.Log.LogDebug($"Old name: \"{origName}\"");
					Mod.Log.LogDebug($"New name: \"{playlist.Name_}\"");
					if (autoSave)
					{
						playlist.Save();

						PlaylistNameChanged.Broadcast(new PlaylistNameChanged.Data(playlist, origName));
					}

					return true;
				}
				else
				{
					Mod.Log.LogDebug("No playlist name change");
					return false;
				}
			}
			catch (Exception ex)
			{
				Mod.Log.LogError("Error in Rename()");
				Mod.Log.LogError(ex);
				return false;
				//throw;
			}
		}

		public static bool Recolor(this LevelPlaylist playlist, Color? optNewColor, bool autoSave)
		{
			try
			{
				string origName = playlist.Name_;

				bool hasColor = playlist.GetNameAndColor(out string name, out bool colorTag, out Color oldColor, false);

				Color newColor = (optNewColor ?? BasePersonalColor);


				// These bools state whether the new and old colors can support *not* using the [c] color tag.
				Color oldBaseColor = oldColor;
				bool useOldBase = !colorTag;
				if (colorTag) // If color tag is used, then oldColor is not the baseColor
				{
					useOldBase = oldColor.TryGetBaseColorFromMultiplier(BasePersonalColor, out oldBaseColor);
				}
				else
				{
					oldColor *= BasePersonalColor;
				}
				//bool useOldBase = oldColor.TryGetBaseColorFromMultiplier(BasePersonalColor, out Color oldBaseColor);
				bool useNewBase = newColor.TryGetBaseColorFromMultiplier(BasePersonalColor, out Color newBaseColor);


				// If the user has put other formatting/color symbols inside the name,
				//  then we need to try and preserve the [c] color tag usage if possible.
				bool hasSymbols = NGUIText.StripSymbols(name).Length != name.Length;


				if (optNewColor.HasValue && hasColor && useOldBase && useNewBase && newBaseColor.Color32Equals(oldBaseColor))
				{
					// Base color is the same, don't change anything.
				}
				else if (optNewColor.HasValue && hasColor && colorTag && newColor.Color32Equals(oldColor))
				{
					// The above comparison needs to check `colorTag`, because the extracted color would be a base color otherwise.
					// Normal color is the same, don't change anything.
				}
				else if (!optNewColor.HasValue || newColor.Color32Equals(BasePersonalColor))
				{
					if (hasSymbols && colorTag)
					{
						playlist.Name_ = name.EncodeNGUIColorTag(); // [c]{name}[/c]
					}
					else
					{
						// Color is the same as the multipier for base color. Remove the color tag and hex.
						playlist.Name_ = name;//.EncodeNGUIColorHex(Color.white); // [FFFFFF]{newName}[-]
					}
				}
				else if (useNewBase && (!hasSymbols || !colorTag))
				{
					// We can use the base color, meaning the color will have full support even without this mod.
					playlist.Name_ = name.EncodeNGUIColorHex(newBaseColor); // [RRGGBB(AA)]{name}[-]
				}
				else
				{
					// Don't attempt to honor hasSymbols because we're forced to use the color tag in this case.

					playlist.Name_ = name.EncodeNGUIColor(newColor); // [c][RRGGBB(AA)]{name}[-][/c]
				}


				if (playlist.Name_ != origName)
				{
					string dbgOldColor     = ((hasColor && colorTag)   ? "#"+NGUIText.EncodeColor32(oldColor)     : "N/A      ");
					string dbgOldBaseColor = ((hasColor && useOldBase) ? "#"+NGUIText.EncodeColor32(oldBaseColor) : "N/A      ");
					string dbgNewColor     =                             "#"+NGUIText.EncodeColor32(newColor);
					string dbgNewBaseColor = (useNewBase               ? "#"+NGUIText.EncodeColor32(newBaseColor) : "N/A      ");
					Mod.Log.LogDebug($"Old color: {dbgOldColor}, base: {dbgOldBaseColor}");
					Mod.Log.LogDebug($"New color: {dbgNewColor}, base: {dbgNewBaseColor}");
					Mod.Log.LogDebug($"Old name: \"{origName}\"");
					Mod.Log.LogDebug($"New name: \"{playlist.Name_}\"");
					if (autoSave)
					{
						playlist.Save();

						PlaylistColorChanged.Broadcast(new PlaylistColorChanged.Data(playlist, (hasColor ? (Color?)oldColor : null), origName, optNewColor));
					}

					return true;
				}
				else
				{
					Mod.Log.LogDebug("No playlist color change");
					return false;
				}
			}
			catch (Exception ex)
			{
				Mod.Log.LogError("Error in Recolor()");
				Mod.Log.LogError(ex);
				return false;
				//throw;
			}
		}

		public static bool RenameFile(this LevelPlaylist playlist, string newFileName, out bool invalidName, out bool notFound, out bool alreadyExists)
		{
			invalidName = false;
			notFound = false;
			alreadyExists = false;
			var playlistData = playlist.GetComponent<LevelPlaylistCompoundData>();
			try
			{
				if (!playlist.IsResourcesPlaylist())
				{
					//newFileName = Path.ChangeExtension(newFileName, ".xml");
					newFileName += ".xml";
					if (newFileName == Resource.GetFileName(playlist.GenerateFilePath(false)))
					{
						Mod.Log.LogDebug("No playlist filename change");
						return false;
					}

					string directory = Resource.GetDirectoryPathFromFilePath(playlistData.FilePath);
					string newFilePath = Path.Combine(directory, newFileName);
					if (!Sanitizer.IsInvalidName(newFileName, 128, out Sanitizer.InvalidReason reason))
					{
						if (FileEx.Exists(playlistData.FilePath))
						{
							if (!FileEx.Exists(newFilePath))
							{
								string oldFilePath = playlistData.FilePath;
								string oldLevelSetID = playlist.GetLevelSetID();

								Mod.Log.LogDebug($"Moving: {playlist.Name_} \"{playlistData.FilePath}\"");
								Mod.Log.LogDebug($"To:     {new string(' ', playlist.Name_.Length)} \"{newFilePath}\"");
								FileEx.Move(playlistData.FilePath, newFilePath);
								playlistData.FilePath = newFilePath;

								PlaylistFileRenamed.Broadcast(new PlaylistFileRenamed.Data(playlist, oldFilePath, oldLevelSetID));

								return true;
							}
							else
							{
								alreadyExists = true;
								return false;
							}
						}
						else
						{
							notFound = true;
							return false;
						}
					}
					else
					{
						invalidName = true;
						return false;
					}
				}
				else
				{
					Mod.Log.LogWarning("Attempting to call RenameFile() on resources playlist");
					return false;
				}
			}
			catch (Exception ex)
			{
				Mod.Log.LogError("Error in RenameFile()");
				Mod.Log.LogError(ex);
				return false;
				//throw;
			}
		}

		public static bool DeleteFile(this LevelPlaylist playlist, bool destroyObject, out bool notFound)
		{
			notFound = false;
			var playlistData = playlist.GetComponent<LevelPlaylistCompoundData>();
			try
			{
				if (playlistData && !playlist.IsResourcesPlaylist())
				{
					if (FileEx.Exists(playlistData.FilePath))
					{
						string oldFilePath = playlistData.FilePath;
						string oldLevelSetID = playlist.GetLevelSetID();
						string oldName = playlist.Name_;

						Mod.Log.LogDebug($"Deleting:   {playlist.Name_} \"{playlistData.FilePath}\"");
						FileEx.Delete(playlistData.FilePath);
						if (destroyObject)
						{
							Mod.Log.LogDebug($"Destroying: {playlist.Name_}");
							playlist.Destroy(); // Should we really call destroy here?
							UnityEngine.Object.DestroyImmediate(playlist.gameObject);
						}

						PlaylistFileDeleted.Broadcast(new PlaylistFileDeleted.Data(oldFilePath, oldLevelSetID, oldName));

						return true;
					}
					else
					{
						notFound = true;
						return false;
					}
				}
				else
				{
					Mod.Log.LogWarning("Attempting to call DeleteFile() on resources playlist");
					return false;
				}
			}
			catch (Exception ex)
			{
				Mod.Log.LogError("Error in DeleteFile()");
				Mod.Log.LogError(ex);
				return false;
				//throw;
			}
		}

		#endregion

		#region Prompt Change Information

		public static void PromptRename(this LevelPlaylist playlist, Action<bool> onSubmit, Action onPop, bool autoSave)
		{
			string name = playlist.GetUncoloredName();
			if (name.IsEmptyPlaylistName())
			{
				name = null;
			}

			bool OnRenameSubmit(out string error, string input)
			{
				if (!input.IsEmptyPlaylistName())
				{
					bool changed = playlist.Rename(input, autoSave);
					onSubmit?.Invoke(changed);

					error = "";
					return true;
				}
				else
				{
					error = "Playlist name is empty";
					return false;
				}
			}

			InputPromptPanel.Create(
				OnRenameSubmit,
				(onPop != null) ? new InputPromptPanel.OnPop(onPop) : null,
				"PLAYLIST DISPLAY NAME",
				name);
		}

		public static void PromptRecolor(this LevelPlaylist playlist, Action<bool> onSubmit, Action onPop, bool autoSave)
		{
			if (!playlist.GetColor(out bool colorTag, out Color color, true))
			{
				color = BasePersonalColor;
			}
			/*else if (!colorTag) // handled by true parameter
			{
				color *= BasePersonalColor;
			}*/

			string hex = (color.a < 1f) ? NGUIText.EncodeColor32(color) : NGUIText.EncodeColor24(color);
			string hexColor = $"#{hex.ToUpperInvariant()}";

			bool OnRecolorSubmit(out string error, string input)
			{
				Regex hexRegex = new Regex(@"^#?(?<color>([A-Fa-f0-9]){8}|([A-Fa-f0-9]){6})$");// InternalResources.Constants.REGEX_HEXADECIMAL_COLOR);
				Match hexMatch = hexRegex.Match(input);
				if (hexMatch.Success || input.Length == 0)
				{
					Color? newColor = null; // Empty input to remove color
					if (hexMatch.Success)
					{
						newColor = hexMatch.Groups["color"].Value.ToColor();
					}
					bool changed = playlist.Recolor(newColor, autoSave);
					onSubmit?.Invoke(changed);

					error = "";
					return true;
				}
				else
				{
					error = "Invalid hex code";
					return false;
				}
			}

			InputPromptPanel.Create(
				OnRecolorSubmit,
				(onPop != null) ? new InputPromptPanel.OnPop(onPop) : null,
				"PLAYLIST HEX COLOR",
				hexColor);
		}

		public static void PromptRenameFile(this LevelPlaylist playlist, Action<bool> onSubmit, Action onPop)
		{
			var playlistData = playlist.GetComponent<LevelPlaylistCompoundData>();
			if (playlistData && playlistData.FilePath != null)
			{
				string fileName = Resource.GetFileNameWithoutExtension(playlist.GenerateFilePath(false));
				if (fileName.IsEmptyPlaylistName())
				{
					fileName = null;
				}

				bool OnRenameFileSubmit(out string error, string input)
				{
					if (!input.IsEmptyPlaylistName())
					{
						bool changed = playlist.RenameFile(input, out bool invalidName, out bool notFound, out bool alreadyExists);
						if (!changed)
						{
							if (invalidName)
							{
								error = "Playlist file name is invalid";
								return false;
							}
							else if (notFound)
							{
								error = "Original playlist file name does not exist";
								return false;
							}
							else if (alreadyExists)
							{
								error = "Playlist with new file name already exists";
								return false;
							}
						}
						onSubmit?.Invoke(changed);

						error = "";
						return true;
					}
					else
					{
						error = "Playlist name is empty";
						return false;
					}
				}

				InputPromptPanel.Create(
					OnRenameFileSubmit,
					(onPop != null) ? new InputPromptPanel.OnPop(onPop) : null,
					"RENAME PLAYLIST FILE",
					fileName);
			}
			else
			{
				Mod.Log.LogError($"Cannot rename playlist \"{playlist.Name_}\" because it does not have a FilePath attached");
			}
		}


		public static void PromptDeleteFile(this LevelPlaylist playlist, Action<bool> onSubmit, bool destroyObject)
		{
			var playlistData = playlist.GetComponent<LevelPlaylistCompoundData>();
			if (playlistData && playlistData.FilePath != null)
			{
				void ActuallyDeleteFile()
				{
					bool changed = playlist.DeleteFile(destroyObject, out bool notFound);
					if (!changed)
					{
						if (notFound)
						{
							MessageBox.Create("Could not delete playlist, file not found", "ERROR")
								.SetButtons(MessagePanelLogic.ButtonType.Ok)
								.Show();
							return;
						}
					}
					onSubmit?.Invoke(changed);
				}

				void OnDeleteFileConfirm()
				{
					// Double-nest confirmation, since we're placing this in a *supposedly* more commonly-used menu.
					MessageBox.Create("Are you [FF2B19]really[-] sure that you want to permanently delete this playlist!?", "ARE YOU SURE?")
						.SetButtons(MessagePanelLogic.ButtonType.YesNo)
						.OnConfirm(ActuallyDeleteFile)
						.Show();
				}

				// TODO: Do we need to double-nest confirmation?
				MessageBox.Create($"Are you sure you want to remove this playlist: [u]{Resource.GetFileName(playlistData.FilePath)}[/u]?", "DELETE PLAYLIST")
					.SetButtons(MessagePanelLogic.ButtonType.YesNo)
					.OnConfirm(OnDeleteFileConfirm)
					.Show();
			}
			else
			{
				Mod.Log.LogError($"Cannot delete playlist \"{playlist.Name_}\" because it does not have a FilePath attached");
			}
		}

		#endregion
	}
}