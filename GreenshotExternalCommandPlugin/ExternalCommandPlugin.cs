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
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Greenshot.IniFile;
using Greenshot.Plugin;
using GreenshotPlugin.Core;
using System.ComponentModel.Composition;
using GreenshotPlugin.Core.Settings;

namespace ExternalCommand {
	/// <summary>
	/// An Plugin to run commands after an image was written
	/// </summary>
	[Export(typeof(IGreenshotPlugin))]
	[ExportMetadata("name", "External Command plug-in")]
	public class ExternalCommandPlugin : AbstractPlugin {
		private static readonly log4net.ILog LOG = log4net.LogManager.GetLogger(typeof(ExternalCommandPlugin));
		private static ExternalCommandConfiguration config = IniConfig.GetIniSection<ExternalCommandConfiguration>();
		private IGreenshotHost host;

		public ExternalCommandPlugin() {
		}

		public override IEnumerable<IDestination> Destinations() {
			foreach(ExternalCommandData command in config.ExternalCommands) {
				yield return new ExternalCommandDestination(command);
			}
		}

		/// <summary>
		/// Implementation of the IGreenshotPlugin.Initialize
		/// </summary>
		/// <param name="host">Use the IGreenshotPluginHost interface to register events</param>
		/// <param name="metadata">IDictionary<string, object></param>
		/// <returns>true if plugin is initialized, false if not (doesn't show)</returns>
		public override bool Initialize(IGreenshotHost pluginHost, IDictionary<string, object> metadata) {
			this.host = pluginHost;
			SettingsWindow.RegisterSettingsPage<ExternalCommandSettingsPage>("settings_plugins,externalcommand.settings_title");
			return true;
		}
	}
}