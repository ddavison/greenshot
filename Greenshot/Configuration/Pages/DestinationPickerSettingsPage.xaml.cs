using Greenshot.Helpers;
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
using Greenshot.IniFile;
using Greenshot.Plugin;
using GreenshotPlugin.Core;
using GreenshotPlugin.Core.Settings;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Greenshot.Configuration.Pages {
	/// <summary>
	/// Logic for the DestinationPickerSettingsPage.xaml
	/// </summary>
	public partial class DestinationPickerSettingsPage : SettingsPage {
		private static readonly CoreConfiguration coreConfiguration = IniConfig.GetIniSection <CoreConfiguration>();
		private ObservableCollection<IDestination> availableDestinations = new ObservableCollection<IDestination>();
		private ObservableCollection<IDestination> selectedDestinations = new ObservableCollection<IDestination>();
		public ObservableCollection<IDestination> AvailableDestinations {
			get {
				return availableDestinations;
			}
		}

		public ObservableCollection<IDestination> SelectedDestinations {
			get {
				return selectedDestinations;
			}
		}

		protected override void Initialize() {
			base.Initialize();
			if (coreConfiguration.PickerDestinations == null) {
				coreConfiguration.PickerDestinations = new List<string>();
			}

			if (coreConfiguration.PickerDestinations.Count > 0) {
				// Show selected (and active) destinations
				foreach (string designation in coreConfiguration.PickerDestinations) {
					IDestination destination = DestinationHelper.GetDestination(designation);
					if ("Picker".Equals(destination.Designation)) {
						continue;
					}
					selectedDestinations.Add(destination);
				}
				foreach (IDestination destination in DestinationHelper.GetAllDestinations()) {
					// Skip picker
					if ("Picker".Equals(destination.Designation)) {
						continue;
					}
					if (!coreConfiguration.PickerDestinations.Contains(destination.Designation)) {
						availableDestinations.Add(destination);
					}
				}
			} else {
				foreach (IDestination destination in DestinationHelper.GetAllDestinations()) {
					// Skip picker
					if ("Picker".Equals(destination.Designation)) {
						continue;
					}
					selectedDestinations.Add(destination);
				}
			}

		}
		public DestinationPickerSettingsPage() : base() {
			InitializeComponent();
		}

		public override void Commit() {
			base.Commit();
			coreConfiguration.PickerDestinations.Clear();
			// Are any unselected, only than we need to specify the list
			if (availableDestinations.Count > 0) {
				foreach (IDestination destination in selectedDestinations) {
					if (!coreConfiguration.PickerDestinations.Contains(destination.Designation)) {
						coreConfiguration.PickerDestinations.Add(destination.Designation);
					}
				}
			}
		}
	}
}
