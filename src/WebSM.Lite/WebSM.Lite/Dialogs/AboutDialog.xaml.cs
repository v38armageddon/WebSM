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
using Windows.System;

namespace WebSM.Lite.Dialogs
{
    public sealed partial class AboutDialog : ContentDialog
    {
        public AboutDialog()
        {
            InitializeComponent();
        }

        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            bool result = await Launcher.LaunchUriAsync(new Uri("ms-windows-store://review/?ProductId=9P5RZDH9SL6F"));
            result = true;
        }
    }
}
