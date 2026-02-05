/*
 * WebSM - A simply minimalist web browser.
 * Copyright (C) 2022 - 2025 - v38armageddon
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */
using System.Xml.Linq;

namespace WebSM;

public class SettingsHandler
{
    public static void SetDefaultTheme()
    {
        string filePath = Path.Combine(ApplicationData.Current.RoamingFolder.Path, "settings.xml");
        if (File.Exists(filePath))
        {
            try
            {
                XDocument doc = XDocument.Load(filePath);
                if (doc.Root != null)
                {
                    XElement theme = doc.Root.Element("Theme");
                    if (theme == null)
                    {
                        theme = new XElement("Theme");
                        doc.Root.Add(theme);
                    }
                    theme.Value = "0";
                    doc.Save(filePath);
                }
            }
            catch { }
        }
    }

    public static void SetLightTheme()
    {
        string filePath = Path.Combine(ApplicationData.Current.RoamingFolder.Path, "settings.xml");
        if (File.Exists(filePath))
        {
            try
            {
                XDocument doc = XDocument.Load(filePath);
                if (doc.Root != null)
                {
                    XElement theme = doc.Root.Element("Theme");
                    if (theme == null)
                    {
                        theme = new XElement("Theme");
                        doc.Root.Add(theme);
                    }
                    theme.Value = "1";
                    doc.Save(filePath);
                }
            }
            catch { }
        }
    }
    public static void SetDarkTheme()
    {
        string filePath = Path.Combine(ApplicationData.Current.RoamingFolder.Path, "settings.xml");
        if (File.Exists(filePath))
        {
            try
            {
                XDocument doc = XDocument.Load(filePath);
                if (doc.Root != null)
                {
                    XElement theme = doc.Root.Element("Theme");
                    if (theme == null)
                    {
                        theme = new XElement("Theme");
                        doc.Root.Add(theme);
                    }
                    theme.Value = "2";
                    doc.Save(filePath);
                }
            }
            catch { }
        }
    }
}
