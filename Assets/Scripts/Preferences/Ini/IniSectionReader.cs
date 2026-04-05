using System;
using System.Collections.Generic;
using System.Globalization;


namespace Preferences.Ini
{
	/// <summary>
	/// Typed reader for a single INI section.
	/// </summary>
	public readonly struct IniSectionReader
	{
		// State

		private readonly IReadOnlyDictionary<string, string> m_Values;

		// Construction

		/// <summary>
		/// Creates a reader over the provided key-value collection.
		/// </summary>
		public IniSectionReader(IReadOnlyDictionary<string, string> values) => m_Values = values;

		// Read

		/// <summary>
		/// Reads a string value from the section.
		/// </summary>
		public string GetString(string key, string fallback = "")
		{
			return m_Values != null && m_Values.TryGetValue(key, out string value) ? value : fallback;
		}

		/// <summary>
		/// Reads an integer value from the section.
		/// </summary>
		public int GetInt(string key, int fallback = 0)
		{
			return int.TryParse(GetString(key), NumberStyles.Integer, CultureInfo.InvariantCulture, out int value)
				       ? value
				       : fallback;
		}

		/// <summary>
		/// Reads a floating-point value from the section.
		/// </summary>
		public float GetFloat(string key, float fallback = 0f)
		{
			return float.TryParse(GetString(key), NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out float value)
				       ? value
				       : fallback;
		}

		/// <summary>
		/// Reads a boolean value from the section.
		/// </summary>
		public bool GetBool(string key, bool fallback = false)
		{
			string value = GetString(key);

			if (bool.TryParse(value, out bool parsed)) {
				return parsed;
			}

			return value switch {
				"1" => true,
				"0" => false,
				_   => fallback,
			};
		}

		/// <summary>
		/// Reads an enum value from the section.
		/// </summary>
		public T GetEnum<T>(string key, T fallback) where T : struct, Enum
		{
			return Enum.TryParse(GetString(key), true, out T value) ? value : fallback;
		}
	}
}