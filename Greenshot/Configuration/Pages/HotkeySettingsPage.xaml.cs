﻿/*
 * Greenshot - a free and open source screenshot tool
 * Copyright (C) 2007-2013  Thomas Braun, Jens Klingen, Robin Krom
 * 
 * For more information see: http://getgreenshot.org/
 * The Greenshot project is hosted on Sourceforge: http://sourceforge.net/projects/greenshot/
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 1 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
using Greenshot.Helpers;
using Greenshot.IniFile;
using GreenshotPlugin.Core;
using GreenshotPlugin.Core.Settings;
using System;
using System.Collections.Generic;

namespace Greenshot.Configuration.Pages {
	/// <summary>
	/// Logic for the HotkeySettingsPage.xaml
	/// </summary>
	public partial class HotkeySettingsPage : SettingsPage {
		private static log4net.ILog LOG = log4net.LogManager.GetLogger(typeof(HotkeySettingsPage));

		public HotkeySettingsPage() : base() {
			InitializeComponent();
		}

		private void SettingsPage_GotFocus(object sender, System.Windows.RoutedEventArgs e) {
			SettingsHotkeyTextBox.UnregisterHotkeys();
		}

		private void SettingsPage_LostFocus(object sender, System.Windows.RoutedEventArgs e) {
			MainForm.RegisterHotkeys();
		}
	}
}
