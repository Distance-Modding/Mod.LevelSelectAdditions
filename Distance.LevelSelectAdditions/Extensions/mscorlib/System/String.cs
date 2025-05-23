#pragma warning disable RCS1110
using System.Globalization;
using System.IO;
using UnityEngine;

namespace Distance.LevelSelectAdditions.Extensions
{
	public static class StringExtensions
	{

		public static Color ToColor(this string hexString)
		{
			var actualColorString = hexString.StartsWith("#") ? hexString.Substring(1, hexString.Length - 1) : hexString;

			if (actualColorString.Length % 2 != 0)
			{
				return Color.black;
			}

			if (actualColorString.Length < 6)
			{
				return Color.black;
			}

			if (actualColorString.Length > 8)
			{
				return Color.black;
			}

			return ParseHex(actualColorString);
		}

		private static Color32 ParseHex(string hexString)
		{
			if (!byte.TryParse(hexString.Substring(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte r))
			{
				return Color.black;
			}

			if (!byte.TryParse(hexString.Substring(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte g))
			{
				return Color.black;
			}

			if (!byte.TryParse(hexString.Substring(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte b))
			{
				return Color.black;
			}

			byte a = 255;

			if (hexString.Length == 8 && !byte.TryParse(hexString.Substring(6, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out a))
			{
				return Color.black;
			}

			return new Color32(r, g, b, a);
		}

		public static string UniformPathName(this string source)
		{
			return source.UniformPathSeparators().ToLowerInvariant();
		}

		public static string UniformPathSeparators(this string source)
		{
			return source.Replace(Path.DirectorySeparatorChar, '/')
						 .Replace(Path.AltDirectorySeparatorChar, '/')
						 .Replace('\\', '/');
		}

		public static string UniformPathSeparatorsTrimmed(this string source)
		{
			return source.UniformPathSeparators().TrimEnd('/');
		}


		public static bool IsEmptyPlaylistName(this string source)
		{
			return string.IsNullOrEmpty(source) || source == nameof(LevelPlaylist) ||
					NGUIText.StripSymbols(source).Trim().Length == 0;
		}
	}
}