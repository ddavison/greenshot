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
using System.Web;
using System.Windows;
using Confluence;
using Greenshot.IniFile;
using Greenshot.Plugin;
using GreenshotPlugin.Core;
using System.ComponentModel.Composition;
using GreenshotPlugin.WPF;
using GreenshotPlugin.Core.Settings;

namespace GreenshotConfluencePlugin {
	/// <summary>
	/// This is the ConfluencePlugin base code
	/// </summary>
	[Export(typeof(IGreenshotPlugin))]
	[ExportMetadata("name", "Confluence plug-in")]
	public class ConfluencePlugin : AbstractPlugin {
		private static readonly log4net.ILog LOG = log4net.LogManager.GetLogger(typeof(ConfluencePlugin));
		private static ConfluenceConnector confluenceConnector = null;
		private static ConfluenceConfiguration config = null;
		private static IGreenshotHost host;

		private static void CreateConfluenceConntector() {
			if (confluenceConnector == null) {
				if (config.Url.Contains("soap-axis")) {
					confluenceConnector = new ConfluenceConnector(config.Url, config.Timeout);
				} else {
					confluenceConnector = new ConfluenceConnector(config.Url + ConfluenceConfiguration.DEFAULT_POSTFIX2, config.Timeout);
				}
			}
		}

		public static ConfluenceConnector ConfluenceConnectorNoLogin {
			get {
				return confluenceConnector;
			}
		}

		public static ConfluenceConnector ConfluenceConnector {
			get {
				if (confluenceConnector == null) {
					CreateConfluenceConntector();
				}
				try {
					if (!confluenceConnector.isLoggedIn) {
						confluenceConnector.login();
					}
				} catch (Exception e) {
					MessageBox.Show(Language.GetFormattedString("confluence", LangKey.login_error, e.Message));
				}
				return confluenceConnector;
			}
		}

		public ConfluencePlugin() {
		}

		public override IEnumerable<IDestination> Destinations() {
			if (ConfluenceDestination.IsInitialized) {
				yield return new ConfluenceDestination();
			} else {
				yield break;
			}
		}

		/// <summary>
		/// Implementation of the IGreenshotPlugin.Initialize
		/// </summary>
		/// <param name="host">Use the IGreenshotPluginHost interface to register events</param>
		/// <param name="metadata">IDictionary<string, object></param>
		/// <returns>true if plugin is initialized, false if not (doesn't show)</returns>
		public override bool Initialize(IGreenshotHost pluginHost, IDictionary<string, object> metadata) {
			host = pluginHost;

			// Register configuration (don't need the configuration itself)
			config = IniConfig.GetIniSection<ConfluenceConfiguration>();
			if(config.IsDirty) {
				IniConfig.Save();
			}
			// Register our configuration
			SettingsWindow.RegisterSettingsPage<ConfluenceSettingsPage>("settings_plugins,confluence.plugin_settings");
			return true;
		}

		public override void Shutdown() {
			LOG.Debug("Confluence Plugin shutdown.");
			if (confluenceConnector != null) {
				confluenceConnector.logout();
				confluenceConnector = null;
			}
		}
	}
}
