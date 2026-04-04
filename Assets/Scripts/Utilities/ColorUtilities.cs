namespace Utilities
{
	public static class ColorUtilities
	{
		public static UnityEngine.Color FromHex(string hex)
		{
			if (string.IsNullOrEmpty(hex))
				return UnityEngine.Color.white;

			if (hex.StartsWith("#"))
				hex = hex.Substring(1);

			if (hex.Length == 6)
				hex += "FF"; // Add alpha if not provided

			if (hex.Length != 8)
				throw new System.ArgumentException("Hex string must be 6 or 8 characters long.");

			byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
			byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
			byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
			byte a = byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);

			return new UnityEngine.Color32(r, g, b, a);
		}
	}
}