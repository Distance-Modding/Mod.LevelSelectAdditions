﻿using Centrifuge.Distance.Data;
using Centrifuge.Distance.Game;
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


		public static string GetLevelSetID(this LevelPlaylist playlist)
		{
			var playlistData = playlist.GetComponent<LevelPlaylistCompoundData>();
			if (playlistData && playlistData.FilePath != null)
			{
				string path = new FileInfo(playlistData.FilePath).FullName.UniformPathSeparators();
				string resourcesPath = new DirectoryInfo(Path.Combine(Application.dataPath, "Resources")).FullName.UniformPathSeparatorsTrimmed() + "/";

				// Only assign if we have a FilePath to determine with,
				//  otherwise assume its a resource playlist until a FileName may be assigned.
				if (path.StartsWith(resourcesPath, StringComparison.InvariantCultureIgnoreCase))
				{
					return IDPrefixResources + path.Substring(resourcesPath.Length/* + 1*/).ToLowerInvariant();
				}

				string personalPath = new DirectoryInfo(Resource.personalDistanceDirPath_).FullName.UniformPathSeparatorsTrimmed() + "/";

				if (path.StartsWith(personalPath, StringComparison.InvariantCultureIgnoreCase))
				{
					return IDPrefixPersonal + path.Substring(personalPath.Length/* + 1*/).ToLowerInvariant();
				}

				return IDPrefixPath + path.ToLowerInvariant();
			}

			return IDPrefixName + playlist.Name_.ToLowerInvariant();
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

		#region Display Name

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
				color *= GConstants.myLevelColor_;
			}
			return hasColor;
		}

		// Returns true if color has a value.
		public static bool GetNameAndColor(this LevelPlaylist playlist, out string name, out bool colorTag, out Color color, bool multiplyBaseColor)
		{
			bool hasColor = playlist.Name_.DecodeNGUIColor(out name, out colorTag, out color);
			if (multiplyBaseColor && hasColor && !colorTag)
			{
				color *= GConstants.myLevelColor_;
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
					/*if (!hasSymbols && color.Color32Equals(GConstants.myLevelColor_))
					{
						playlist.Name_ = newName;//.EncodeNGUIColorHex(Color.white); // [FFFFFF]{newName}[-]
					}
					else if (color.TryGetBaseColorFromMultiplier(GConstants.myLevelColor_, out Color baseColor))
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
					Mod.Instance.Logger.Debug($"Old name: \"{origName}\"");
					Mod.Instance.Logger.Debug($"New name: \"{playlist.Name_}\"");
					if (autoSave)
					{
						playlist.Save();

						PlaylistNameChanged.Broadcast(new PlaylistNameChanged.Data(playlist, origName));
					}

					return true;
				}
				else
				{
					Mod.Instance.Logger.Debug("No playlist name change");
					return false;
				}
			}
			catch (Exception ex)
			{
				Mod.Instance.Logger.Error("Error in Rename()");
				Mod.Instance.Logger.Exception(ex);
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

				Color newColor = (optNewColor ?? GConstants.myLevelColor_);


				// These bools state whether the new and old colors can support *not* using the [c] color tag.
				Color oldBaseColor = oldColor;
				bool useOldBase = !colorTag;
				if (colorTag) // If color tag is used, then oldColor is not the baseColor
				{
					useOldBase = oldColor.TryGetBaseColorFromMultiplier(GConstants.myLevelColor_, out oldBaseColor);
				}
				else
				{
					oldColor *= GConstants.myLevelColor_;
				}
				//bool useOldBase = oldColor.TryGetBaseColorFromMultiplier(GConstants.myLevelColor_, out Color oldBaseColor);
				bool useNewBase = newColor.TryGetBaseColorFromMultiplier(GConstants.myLevelColor_, out Color newBaseColor);


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
				else if (!optNewColor.HasValue || newColor.Color32Equals(GConstants.myLevelColor_))
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
					Mod.Instance.Logger.Debug($"Old color: {dbgOldColor}, base: {dbgOldBaseColor}");
					Mod.Instance.Logger.Debug($"New color: {dbgNewColor}, base: {dbgNewBaseColor}");
					Mod.Instance.Logger.Debug($"Old name: \"{origName}\"");
					Mod.Instance.Logger.Debug($"New name: \"{playlist.Name_}\"");
					if (autoSave)
					{
						playlist.Save();

						PlaylistColorChanged.Broadcast(new PlaylistColorChanged.Data(playlist, (hasColor ? (Color?)oldColor : null), origName, optNewColor));
					}

					return true;
				}
				else
				{
					Mod.Instance.Logger.Debug("No playlist color change");
					return false;
				}
			}
			catch (Exception ex)
			{
				Mod.Instance.Logger.Error("Error in Recolor()");
				Mod.Instance.Logger.Exception(ex);
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
						Mod.Instance.Logger.Debug("No playlist filename change");
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

								Mod.Instance.Logger.Debug($"Moving: {playlist.Name_} \"{playlistData.FilePath}\"");
								Mod.Instance.Logger.Debug($"To:     {new string(' ', playlist.Name_.Length)} \"{newFilePath}\"");
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
					Mod.Instance.Logger.Warning("Attempting to call RenameFile() on resources playlist");
					return false;
				}
			}
			catch (Exception ex)
			{
				Mod.Instance.Logger.Error("Error in RenameFile()");
				Mod.Instance.Logger.Exception(ex);
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

						Mod.Instance.Logger.Debug($"Deleting:   {playlist.Name_} \"{playlistData.FilePath}\"");
						FileEx.Delete(playlistData.FilePath);
						if (destroyObject)
						{
							Mod.Instance.Logger.Debug($"Destroying: {playlist.Name_}");
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
					Mod.Instance.Logger.Warning("Attempting to call DeleteFile() on resources playlist");
					return false;
				}
			}
			catch (Exception ex)
			{
				Mod.Instance.Logger.Error("Error in DeleteFile()");
				Mod.Instance.Logger.Exception(ex);
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
				color = GConstants.myLevelColor_;
			}
			/*else if (!colorTag) // handled by true parameter
			{
				color *= GConstants.myLevelColor_;
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
				Mod.Instance.Logger.Error($"Cannot rename playlist \"{playlist.Name_}\" because it does not have a FilePath attached");
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
								.SetButtons(MessageButtons.Ok)
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
						.SetButtons(MessageButtons.YesNo)
						.OnConfirm(ActuallyDeleteFile)
						.Show();
				}

				// TODO: Do we need to double-nest confirmation?
				MessageBox.Create($"Are you sure you want to remove this playlist: [u]{Resource.GetFileName(playlistData.FilePath)}[/u]?", "DELETE PLAYLIST")
					.SetButtons(MessageButtons.YesNo)
					.OnConfirm(OnDeleteFileConfirm)
					.Show();
			}
			else
			{
				Mod.Instance.Logger.Error($"Cannot delete playlist \"{playlist.Name_}\" because it does not have a FilePath attached");
			}
		}

		#endregion
	}
}