using System;
using System.Collections.Generic;
using System.Globalization;


namespace Preferences.Ini
{
	/// <summary>
	///     Typed writer for a single INI section.
	/// </summary>
	public sealed class IniSectionWriter
	{
		// State

		private readonly Dictionary<string, string> m_Values = new(StringComparer.OrdinalIgnoreCase);

		/// <summary>
		///     Raw values collected for the section.
		/// </summary>
		public IReadOnlyDictionary<string, string> Values => m_Values;

		// Write

		/// <summary>
		///     Writes a string value.
		/// </summary>
		public void Set(string key, string value) => m_Values[key] = value ?? string.Empty;
		/// <summary>
		///     Writes an integer value.
		/// </summary>
		public void Set(string key, int value) => m_Values[key] = value.ToString(CultureInfo.InvariantCulture);
		/// <summary>
		///     Writes a floating-point value.
		/// </summary>
		public void Set(string key, float value) => m_Values[key] = value.ToString(CultureInfo.InvariantCulture);
		/// <summary>
		///     Writes a boolean value.
		/// </summary>
		public void Set(string key, bool value) => m_Values[key] = value ? "true" : "false";
		/// <summary>
		///     Writes an enum value.
		/// </summary>
		public void Set<T>(string key, T value) where T : struct, Enum => m_Values[key] = value.ToString();
	}
}