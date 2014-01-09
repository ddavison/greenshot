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
	public class DestinationSelectionContainer {
		public IDestination Destination {
			get;
			set;
		}
		public bool IsSelected {
			get;
			set;
		}
	}
	/// <summary>
	/// Logic for the DestinationsSettingsPage.xaml
	/// </summary>
	public partial class DestinationsSettingsPage : SettingsPage {
		private static readonly CoreConfiguration coreConfiguration = IniConfig.GetIniSection <CoreConfiguration>();

		public List<DestinationSelectionContainer> Destinations {
			get;
			set;
		}

		protected override void Initialize() {
			base.Initialize();
			if (coreConfiguration.OutputDestinations == null) {
				coreConfiguration.OutputDestinations = new List<string>();
			}
			foreach (IDestination destination in DestinationHelper.GetAllDestinations()) {
				Destinations.Add(new DestinationSelectionContainer {
					Destination = destination,
					IsSelected = coreConfiguration.OutputDestinations.Contains(destination.Designation)
				});
			}
		}
		public DestinationsSettingsPage() : base() {
			Destinations = new List<DestinationSelectionContainer>();
			InitializeComponent();
		}

		public override void Commit() {
			base.Commit();
			coreConfiguration.OutputDestinations.Clear();
			foreach (DestinationSelectionContainer destinationSelectionContainer in Destinations) {
				if (destinationSelectionContainer.IsSelected) {
					coreConfiguration.OutputDestinations.Add(destinationSelectionContainer.Destination.Designation);
				}
			}
		}
	}
}
