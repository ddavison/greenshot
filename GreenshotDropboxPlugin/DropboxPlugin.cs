/*
 * Greenshot - a free and open source screenshot tool
 * Copyright (C) 2007-2013  Thomas Braun, Jens Klingen, Robin Krom, Francis Noel
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
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Greenshot.IniFile;
using Greenshot.Plugin;
using GreenshotPlugin.Controls;
using GreenshotPlugin.Core;
using System.ComponentModel.Composition;
using GreenshotPlugin.Core.Settings;

namespace GreenshotDropboxPlugin {
	/// <summary>
	/// This is the Dropbox base code
	/// </summary>
	[Export(typeof(IGreenshotPlugin))]
	[ExportMetadata("name", "Dropbox plug-in")]
	public class DropboxPlugin : AbstractPlugin {
		private static readonly log4net.ILog LOG = log4net.LogManager.GetLogger(typeof(DropboxPlugin));
		private static DropboxPluginConfiguration config;
		private IGreenshotHost host;
		private ComponentResourceManager resources;

		public DropboxPlugin() {
		}

		public override IEnumerable<IDestination> Destinations() {
			yield return new DropboxDestination(this);
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
			config = IniConfig.GetIniSection<DropboxPluginConfiguration>();
			resources = new ComponentResourceManager(typeof(DropboxPlugin));

			// Register our configuration
			SettingsWindow.RegisterSettingsPage<DropboxSettingsPage>("settings_plugins,dropbox.settings_title");
			return true;
		}

		public override void Shutdown() {
			LOG.Debug("Dropbox Plugin shutdown.");
		}

		/// <summary>
		/// This will be called when the menu item in the Editor is clicked
		/// </summary>
		public bool Upload(ICaptureDetails captureDetails, ISurface surfaceToUpload, out string uploadUrl) {
			uploadUrl = null;
			SurfaceOutputSettings outputSettings = new SurfaceOutputSettings(config.UploadFormat, config.UploadJpegQuality, false);
			try {
				string dropboxUrl = null;
				new PleaseWaitForm().ShowAndWait("Dropbox plug-in", Language.GetString("dropbox", LangKey.communication_wait), 
					delegate() {
						string filename = Path.GetFileName(FilenameHelper.GetFilename(config.UploadFormat, captureDetails));
						dropboxUrl = DropboxUtils.UploadToDropbox(surfaceToUpload, outputSettings, filename);
					}
				);
				if (dropboxUrl == null) {
					return false;
				}
				uploadUrl = dropboxUrl;
				return true;
			} catch (Exception e) {
				LOG.Error(e);
				MessageBox.Show(Language.GetString("dropbox", LangKey.upload_failure) + " " + e.ToString());
				return false;
			}
		}
	}
}
