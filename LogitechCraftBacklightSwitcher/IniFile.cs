using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

public class IniFile
{
    private object m_Lock = new object();
    public string FileName;
    private Dictionary<string, Dictionary<string, string>> m_Sections = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

    public IniFile(string FileName)
    {
        this.FileName = FileName;
        Refresh();
    }

    private string ParseSectionName(string Line)
    {
        if (!Line.StartsWith("[")) return null;
        if (!Line.EndsWith("]")) return null;
        if (Line.Length < 3) return null;
        return Line.Substring(1, Line.Length - 2);
    }

    private bool ParseKeyValuePair(string Line, ref string Key, ref string Value)
    {
        int i;
        if ((i = Line.IndexOf('=')) <= 0) return false;
        int j = Line.Length - i - 1;
        Key = Line.Substring(0, i).Trim();
        if (Key.Length <= 0) return false;
        Value = (j > 0) ? (Line.Substring(i + 1, j).Trim()) : ("");
        return true;
    }

    public void Refresh()
    {
        lock (m_Lock)
        {
            StreamReader sr = null;
            try
            {
                m_Sections.Clear();
                try
                {
                    sr = new StreamReader(FileName);
                }
                catch (FileNotFoundException)
                {
                    return;
                }

                Dictionary<string, string> CurrentSection = null;
                string s;
                string SectionName;
                string Key = null;
                string Value = null;
                while ((s = sr.ReadLine()) != null)
                {
                    s = s.Trim();
                    SectionName = ParseSectionName(s);
                    if (SectionName != null)
                    {
                        if (m_Sections.ContainsKey(SectionName))
                        {
                            CurrentSection = null;
                        }
                        else
                        {
                            CurrentSection = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                            m_Sections.Add(SectionName, CurrentSection);
                        }
                    }
                    else if (CurrentSection != null)
                    {
                        if (ParseKeyValuePair(s, ref Key, ref Value))
                        {
                            if (!CurrentSection.ContainsKey(Key))
                            {
                                CurrentSection.Add(Key, Value);
                            }
                        }
                    }
                }
            }
            finally
            {
                if (sr != null) sr.Close();
                sr = null;
            }
        }
    }

    public string GetValue(string SectionName, string Key, string DefaultValue)
    {
        lock (m_Lock)
        {
            Dictionary<string, string> Section;
            if (!m_Sections.TryGetValue(SectionName, out Section)) return DefaultValue;
            string Value;
            if (!Section.TryGetValue(Key, out Value)) return DefaultValue;
            return Value;
        }
    }

    public int GetValue(string SectionName, string Key, int DefaultValue)
    {
        string StringValue = GetValue(SectionName, Key, DefaultValue.ToString(CultureInfo.InvariantCulture));
        int Value;
        if (int.TryParse(StringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out Value)) return Value;
        return DefaultValue;
    }
}