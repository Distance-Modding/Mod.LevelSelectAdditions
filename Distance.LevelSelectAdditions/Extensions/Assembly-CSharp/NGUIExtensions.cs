using System.Text.RegularExpressions;
using UnityEngine;

namespace Distance.LevelSelectAdditions.Extensions
{
	public static class NGUIExtensions
	{
		private static readonly Regex NGUIColorTagRegex = new Regex(@"^\[c\](?<body>.*)\[\/c\]$");
		private static readonly Regex NGUIColorHexRegex = new Regex(@"^\[(?<color>(?:[A-Fa-f0-9]){8}|(?:[A-Fa-f0-9]){6})\](?<body>.*)\[-\]$");


		public static bool DecodeNGUIColorTag(this string source, out string stripped)
		{
			Match tagMatch = NGUIColorTagRegex.Match(source);
			if (tagMatch.Success)
			{
				stripped = tagMatch.Groups["body"].Value;
				return true;
			}
			else
			{
				stripped = source;
				return false;
			}
		}


		public static bool DecodeNGUIColorHex(this string source, out string stripped, out Color color)
		{
			Match hexMatch = NGUIColorHexRegex.Match(source);
			if (hexMatch.Success)
			{
				stripped = hexMatch.Groups["body"].Value;
				color = hexMatch.Groups["color"].Value.ToColor();
				return true;
			}
			else
			{
				stripped = source;
				color = Color.white; // default to white (no colorization)
				return false;
			}
		}


		public static bool DecodeNGUIColor(this string source, out string stripped, out bool colorTag, out Color color)
		{
			stripped = source;

			colorTag = DecodeNGUIColorTag(stripped, out stripped);

			bool hasColorHex = DecodeNGUIColorHex(stripped, out stripped, out color);

			return hasColorHex;// || hasColorTag;
		}


		public static string EncodeNGUIColorTag(this string source)
		{
			return $"[c]{source}[/c]";
		}


		public static string EncodeNGUIColorHex24(this string source, Color color)
		{
			return $"[{NGUIText.EncodeColor24(color)}]{source}[-]";
		}

		public static string EncodeNGUIColorHex32(this string source, Color color)
		{
			return $"[{NGUIText.EncodeColor32(color)}]{source}[-]";
		}

		public static string EncodeNGUIColorHex(this string source, Color color)
		{
			if (color.a < 1f)
			{
				return source.EncodeNGUIColorHex32(color);
			}
			else
			{
				return source.EncodeNGUIColorHex24(color);
			}
		}


		public static string EncodeNGUIColor24(this string source, Color color)
		{
			return source.EncodeNGUIColorHex24(color).EncodeNGUIColorTag();
		}

		public static string EncodeNGUIColor32(this string source, Color color)
		{
			return source.EncodeNGUIColorHex32(color).EncodeNGUIColorTag();
		}

		public static string EncodeNGUIColor(this string source, Color color)
		{
			return source.EncodeNGUIColorHex(color).EncodeNGUIColorTag();
		}


		public static bool Color24Equals(this Color color, Color other)
		{
			color.a = 1f;
			other.a = 1f;
			return color.Color32Equals(other);
		}

		public static bool Color32Equals(this Color color, Color other)
		{
			return NGUIMath.ColorToInt(color) == NGUIMath.ColorToInt(other);
		}

		public static bool ColorSupportsMultiplier(this Color color, Color mult)
		{
			return (color.r <= mult.r && color.g <= mult.g && color.b <= mult.b && color.a <= mult.a);

			// 1 / 256 used here so that we maintain exact color channels (so just a small amount less than 1 unit).
			/*const float Thresh = 1f + (1f / 256f);
			color /= mult;
			return (color.r <= Thresh && color.g <= Thresh && color.b <= Thresh && color.a <= Thresh);*/
		}

		private static bool TryGetBaseChannelFromMultiplier(ref float channel, float mult)
		{
			if (channel > mult)
			{
				return false;
			}

			if (mult <= 0f)
			{
				channel = 0f;
			}
			else
			{
				channel /= mult;
			}
			return true;
		}

		public static bool TryGetBaseColorFromMultiplier(this Color color, Color mult, out Color baseColor)
		{
			baseColor = color;

			if (TryGetBaseChannelFromMultiplier(ref baseColor.r, mult.r) &&
				TryGetBaseChannelFromMultiplier(ref baseColor.g, mult.g) &&
				TryGetBaseChannelFromMultiplier(ref baseColor.b, mult.b) &&
				TryGetBaseChannelFromMultiplier(ref baseColor.a, mult.a))
			{
				// Before returning true, we need to confirm that we can obtain the exact original input color after multiplication.
				Color clampedColor = NGUIMath.IntToColor(NGUIMath.ColorToInt(baseColor));
				return color.Color32Equals(clampedColor * mult);
			}
			return false;
		}
	}
}
