using System;
using System.Globalization;
using UnityEngine;


namespace Utilities
{
	public static class ColorUtilities
	{
		public static Color FromHex(string hex)
		{
			if (string.IsNullOrEmpty(hex))
				return Color.white;

			if (hex.StartsWith("#"))
				hex = hex.Substring(1);

			if (hex.Length == 6)
				hex += "FF"; // Add alpha if not provided

			if (hex.Length != 8)
				throw new ArgumentException("Hex string must be 6 or 8 characters long.");

			byte r = byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
			byte g = byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
			byte b = byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);
			byte a = byte.Parse(hex.Substring(6, 2), NumberStyles.HexNumber);

			return new Color32(r, g, b, a);
		}
	}
}