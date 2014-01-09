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
using Greenshot.Helpers;
using Greenshot.IniFile;
using GreenshotPlugin.Core;
using GreenshotPlugin.Core.Settings;
using System;
using System.Collections.Generic;

namespace Greenshot.Configuration.Pages {
	/// <summary>
	/// Logic for the GeneralSettingsPage.xaml
	/// </summary>
	public partial class GeneralSettingsPage : SettingsPage {
		private static log4net.ILog LOG = log4net.LogManager.GetLogger(typeof(GeneralSettingsPage));

		public IList<LanguageFile> Languages {
			get {
				return GreenshotPlugin.Core.Language.SupportedLanguages;
			}
		}

		public GeneralSettingsPage() : base() {
			InitializeComponent();
		}

		private void SettingsPage_Loaded(object sender, System.Windows.RoutedEventArgs e) {
			if (IniConfig.IsPortable) {
				checkbox_autostartshortcut.Visibility = System.Windows.Visibility.Hidden;
				checkbox_autostartshortcut.IsChecked = false;
			} else {
				// Autostart checkbox logic.
				if (StartupHelper.hasRunAll()) {
					// Remove runUser if we already have a run under all
					StartupHelper.deleteRunUser();
					checkbox_autostartshortcut.IsEnabled = StartupHelper.canWriteRunAll();
					checkbox_autostartshortcut.IsChecked = true; // We already checked this
				} else if (StartupHelper.IsInStartupFolder()) {
					checkbox_autostartshortcut.IsEnabled = false;
					checkbox_autostartshortcut.IsChecked = true; // We already checked this
				} else {
					// No run for all, enable the checkbox and set it to true if the current user has a key
					checkbox_autostartshortcut.IsEnabled = StartupHelper.canWriteRunUser();
					checkbox_autostartshortcut.IsChecked = StartupHelper.hasRunUser();
				}
			}
		}

		public override void Commit() {
			base.Commit();
			// Reflect Language change
			GreenshotPlugin.Core.Language.CurrentLanguage = CoreConfig.Language;
			MainForm.Instance.UpdateUI();

			try {
				if (checkbox_autostartshortcut.IsChecked.HasValue && checkbox_autostartshortcut.IsChecked.Value) {
					// It's checked, so we set the RunUser if the RunAll isn't set.
					// Do this every time, so the executable is correct.
					if (!StartupHelper.hasRunAll()) {
						StartupHelper.setRunUser();
					}
				} else {
					// Delete both settings if it's unchecked
					if (StartupHelper.hasRunAll()) {
						StartupHelper.deleteRunAll();
					}
					if (StartupHelper.hasRunUser()) {
						StartupHelper.deleteRunUser();
					}
				}
			} catch (Exception e) {
				LOG.Warn("Problem checking registry, ignoring for now: ", e);
			}
		}
	}
}
