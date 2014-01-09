
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
using GreenshotPlugin.Core.Settings;
using Greenshot.IniFile;
using GreenshotPlugin.Core;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace ExternalCommand {
	/// <summary>
	/// Interaction logic for ExternalCommandSettingsPage.xaml
	/// </summary>
	public partial class ExternalCommandSettingsPage : SettingsPage {
		private readonly static ExternalCommandConfiguration externalCommandConfiguration = IniConfig.GetIniSection<ExternalCommandConfiguration>();

		private ObservableCollection<EditableObjectProxy<ExternalCommandData>> externalCommands = new ObservableCollection<EditableObjectProxy<ExternalCommandData>>();
		public ObservableCollection<EditableObjectProxy<ExternalCommandData>> ExternalCommands {
			get {
				return externalCommands;
			}
		}
		protected override void Initialize() {
			externalCommands.Clear();
			foreach (ExternalCommandData exCommand in externalCommandConfiguration.ExternalCommands) {
				var proxy = new EditableObjectProxy<ExternalCommandData>(exCommand);
				proxy.StartTransaction();
				externalCommands.Add(proxy);
			}
			base.Initialize();
			//proxy = new IniProxy(externalCommandConfiguration);
		}

		public ExternalCommandSettingsPage() : base() {
			InitializeComponent();
		}

		public override void Rollback() {
			base.Rollback();
			foreach (EditableObjectProxy<ExternalCommandData> exCommand in externalCommands) {
				exCommand.Rollback();
			}
		}
		public override void Commit() {
			base.Commit();
			externalCommandConfiguration.ExternalCommands.Clear();
			foreach (EditableObjectProxy<ExternalCommandData> exCommand in externalCommands) {
				exCommand.Commit();
				// Add current state to the configuration
				externalCommandConfiguration.ExternalCommands.Add(exCommand.ProxiedObject);
			}
		}

		/// <summary>
		/// Add a new default command to the list
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Add_Click(object sender, System.Windows.RoutedEventArgs e) {
			var newCommand = new ExternalCommandData {
				Name = "new"
			};
			var newCommandProxy = new EditableObjectProxy<ExternalCommandData>(newCommand);
			newCommandProxy.StartTransaction();
			externalCommands.Add(newCommandProxy);
		}

		/// <summary>
		/// Remove the current selected item from the list
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Delete_Click(object sender, System.Windows.RoutedEventArgs e) {
			if (CommandsList.SelectedIndex >= 0) {
				externalCommands.RemoveAt(CommandsList.SelectedIndex);
			}
		}
	}
}