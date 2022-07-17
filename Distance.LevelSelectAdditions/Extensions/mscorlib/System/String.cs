using System.IO;

namespace Distance.LevelSelectAdditions.Extensions
{
	public static class StringExtensions
	{
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