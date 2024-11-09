/*
 * WebSM Lite - A simply minimalist web browser.
 * Copyright (C) 2022 - 2024 - v38armageddon
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
using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace WebSM.Lite
{
    public class SettingsTheme
    {
        public static void SetDefaultTheme()
        {
            string filePath = Path.Combine(ApplicationData.Current.RoamingFolder.Path, "settings.json");
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                Dictionary<string, object> settings = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                settings["Theme"] = 0;
                string newJson = JsonConvert.SerializeObject(settings, Formatting.Indented);
                File.WriteAllText(filePath, newJson);
            }
        }

        public static void SetLightTheme()
        {
            string filePath = Path.Combine(ApplicationData.Current.RoamingFolder.Path, "settings.json");
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                Dictionary<string, object> settings = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                settings["Theme"] = 1;
                string newJson = JsonConvert.SerializeObject(settings, Formatting.Indented);
                File.WriteAllText(filePath, newJson);
            }
        }
        public static void SetDarkTheme()
        {
            string filePath = Path.Combine(ApplicationData.Current.RoamingFolder.Path, "settings.json");
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                Dictionary<string, object> settings = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                settings["Theme"] = 2;
                string newJson = JsonConvert.SerializeObject(settings, Formatting.Indented);
                File.WriteAllText(filePath, newJson);
            }
        }
    }
}
