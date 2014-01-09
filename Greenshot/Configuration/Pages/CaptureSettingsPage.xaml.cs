/*
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
using System;
using Greenshot.IniFile;
using GreenshotPlugin.Core;
using GreenshotPlugin.Core.Settings;

namespace Greenshot.Configuration.Pages {
	/// <summary>
	/// Logic for the CaptureSettingsPage.xaml
	/// </summary>
	public partial class CaptureSettingsPage : SettingsPage {

		public CaptureSettingsPage() : base() {
			InitializeComponent();
		}

		private void Button_Color(object sender, System.Windows.RoutedEventArgs e) {
			ColorDialog colorDialog = ColorDialog.GetInstance();
			colorDialog.Color = this.Config.DWMBackgroundColor;
			// Using the parent to make sure the dialog doesn't show on another window
			colorDialog.ShowDialog();
			if (colorDialog.DialogResult != System.Windows.Forms.DialogResult.Cancel) {
				if (!colorDialog.Color.Equals(this.Config.DWMBackgroundColor)) {
					this.Config.DWMBackgroundColor.Value = colorDialog.Color;
				}
			}
		}
	}
}
