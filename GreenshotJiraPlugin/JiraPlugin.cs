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
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using Greenshot.IniFile;
using Greenshot.Plugin;
using Jira;
using System;
using GreenshotPlugin.Core;
using System.ComponentModel.Composition;
using GreenshotPlugin.Core.Settings;

namespace GreenshotJiraPlugin {
	/// <summary>
	/// This is the JiraPlugin base code
	/// </summary>
	[Export(typeof(IGreenshotPlugin))]
	[ExportMetadata("name", "Jira plug-in")]
	public class JiraPlugin : AbstractPlugin {
		private static readonly log4net.ILog LOG = log4net.LogManager.GetLogger(typeof(JiraPlugin));
		private IGreenshotHost host;
		private JiraConnector jiraConnector = null;
		private JiraConfiguration config = null;
		private static JiraPlugin instance = null;

		protected override void Dispose(bool disposing) {
			if (disposing) {
				if (jiraConnector != null) {
					jiraConnector.Dispose();
					jiraConnector = null;
				}
			}
		}

		public static JiraPlugin Instance {
			get {
				return instance;
			}
		}

		public JiraPlugin() {
			instance = this;
		}
		
		public override IEnumerable<IDestination> Destinations() {
			yield return new JiraDestination(this);
		}

		//Needed for a fail-fast
		public JiraConnector CurrentJiraConnector {
			get {
				return jiraConnector;
			}
		}
		
		public JiraConnector JiraConnector {
			get {
				if (jiraConnector == null) {
					jiraConnector = new JiraConnector(true);
				}
				return jiraConnector;
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

			// Register configuration (don't need the configuration itself)
			config = IniConfig.GetIniSection<JiraConfiguration>();
			// Register our configuration
			SettingsWindow.RegisterSettingsPage<JiraSettingsPage>("settings_plugins,jira.settings_title");
			return true;
		}

		public override void Shutdown() {
			LOG.Debug("Jira Plugin shutdown.");
			if (jiraConnector != null) {
				jiraConnector.logout();
			}
		}
	}
}
