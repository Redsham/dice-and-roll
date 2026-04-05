using System;
using System.Collections.Generic;
using System.Text;


namespace Preferences.Ini
{
	/// <summary>
	/// Represents an INI document grouped by named sections.
	/// </summary>
	public sealed class IniDocument
	{
		// State

		private readonly Dictionary<string, Dictionary<string, string>> m_Sections = new(StringComparer.OrdinalIgnoreCase);

		// Factory

		/// <summary>
		/// Parses raw INI text into an <see cref="IniDocument"/>.
		/// </summary>
		public static IniDocument Parse(string content)
		{
			IniDocument document       = new();
			string      currentSection = string.Empty;

			if (string.IsNullOrWhiteSpace(content)) {
				return document;
			}

			string[] lines = content.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
			foreach (string rawLine in lines) {
				string line = rawLine.Trim();
				if (string.IsNullOrEmpty(line) || line.StartsWith(";") || line.StartsWith("#")) {
					continue;
				}

				if (line.StartsWith("[") && line.EndsWith("]")) {
					currentSection = line[1..^1].Trim();
					document.GetOrCreateSection(currentSection);
					continue;
				}

				int separatorIndex = line.IndexOf('=');
				if (separatorIndex <= 0) {
					continue;
				}

				string key   = line[..separatorIndex].Trim();
				string value = line[(separatorIndex + 1)..].Trim();
				document.GetOrCreateSection(currentSection)[key] = Unescape(value);
			}

			return document;
		}

		// Queries

		/// <summary>
		/// Tries to get a section by name.
		/// </summary>
		public bool TryGetSection(string section, out IReadOnlyDictionary<string, string> values)
		{
			if (m_Sections.TryGetValue(section, out Dictionary<string, string> rawValues)) {
				values = rawValues;
				return true;
			}

			values = null;
			return false;
		}

		// Mutations

		/// <summary>
		/// Replaces the values of a section.
		/// </summary>
		public void SetSection(string section, IReadOnlyDictionary<string, string> values)
		{
			Dictionary<string, string> copy = GetOrCreateSection(section);
			copy.Clear();

			foreach (KeyValuePair<string, string> pair in values) {
				copy[pair.Key] = pair.Value ?? string.Empty;
			}
		}

		/// <summary>
		/// Serializes the document back into INI text.
		/// </summary>
		public string Serialize()
		{
			StringBuilder builder      = new();
			bool          firstSection = true;

			foreach (string section in Sort(m_Sections.Keys)) {
				if (!firstSection) {
					builder.AppendLine();
				}

				firstSection = false;
				builder.Append('[').Append(section).AppendLine("]");

				foreach (string key in Sort(m_Sections[section].Keys)) {
					builder.Append(key)
					       .Append('=')
					       .AppendLine(Escape(m_Sections[section][key]));
				}
			}

			return builder.ToString();
		}

		// Helpers

		private Dictionary<string, string> GetOrCreateSection(string section)
		{
			if (!m_Sections.TryGetValue(section, out Dictionary<string, string> values)) {
				values              = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
				m_Sections[section] = values;
			}

			return values;
		}

		private static List<string> Sort(IEnumerable<string> values)
		{
			List<string> ordered = new(values);
			ordered.Sort(StringComparer.OrdinalIgnoreCase);
			return ordered;
		}

		private static string Escape(string value)
		{
			return (value ?? string.Empty)
			      .Replace("\\", "\\\\")
			      .Replace("\n", "\\n")
			      .Replace("\r", "\\r");
		}

		private static string Unescape(string value)
		{
			StringBuilder builder = new(value.Length);

			for (int index = 0; index < value.Length; index++) {
				char symbol = value[index];
				if (symbol != '\\' || index == value.Length - 1) {
					builder.Append(symbol);
					continue;
				}

				index++;
				builder.Append(value[index] switch {
					'n'  => '\n',
					'r'  => '\r',
					'\\' => '\\',
					_    => value[index],
				});
			}

			return builder.ToString();
		}
	}
}