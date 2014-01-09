/*
 * A Picasa Plugin for Greenshot
 * Copyright (C) 2011  Francis Noel
 * 
 * For more information see: http://getgreenshot.org/
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

namespace GreenshotPicasaPlugin {
	/// <summary>
	/// This is the Picasa base code
	/// </summary>
	[Export(typeof(IGreenshotPlugin))]
	[ExportMetadata("name", "Picasa plug-in")]
	public class PicasaPlugin : AbstractPlugin {
		private static readonly log4net.ILog LOG = log4net.LogManager.GetLogger(typeof(PicasaPlugin));
		private static PicasaConfiguration config;
		private IGreenshotHost host;
		private ComponentResourceManager resources;
		
		public PicasaPlugin() {
		}

		public override IEnumerable<IDestination> Destinations() {
			yield return new PicasaDestination(this);
		}


		public override IEnumerable<IProcessor> Processors() {
			yield break;
		}

		/// <summary>
		/// Implementation of the IGreenshotPlugin.Initialize
		/// </summary>
		/// <param name="host">Use the IGreenshotPluginHost interface to register events</param>
		/// <param name="metadata">IDictionary<string, object></param>
		/// <returns>true if plugin is initialized, false if not (doesn't show)</returns>
		public override bool Initialize(IGreenshotHost pluginHost, IDictionary<string, object> metadata) {
			this.host = pluginHost;

			// Get configuration
			config = IniConfig.GetIniSection<PicasaConfiguration>();
			resources = new ComponentResourceManager(typeof(PicasaPlugin));

			// Register our configuration
			SettingsWindow.RegisterSettingsPage<PicasaSettingsPage>("settings_plugins,picasa.settings_title");
			return true;
		}

		public bool Upload(ICaptureDetails captureDetails, ISurface surfaceToUpload, out string uploadUrl) {
			SurfaceOutputSettings outputSettings = new SurfaceOutputSettings(config.UploadFormat, config.UploadJpegQuality);
			try {
				string url = null;
				new PleaseWaitForm().ShowAndWait("Picasa plug-in", Language.GetString("picasa", LangKey.communication_wait), 
					delegate() {
						string filename = Path.GetFileName(FilenameHelper.GetFilename(config.UploadFormat, captureDetails));
						string contentType = "image/" + config.UploadFormat.ToString();
						url = PicasaUtils.UploadToPicasa(surfaceToUpload, outputSettings, captureDetails.Title, filename);
					}
				);
				uploadUrl = url;

				if (uploadUrl != null && config.AfterUploadLinkToClipBoard) {
					ClipboardHelper.SetClipboardData(uploadUrl);
				}
				return true;
			} catch (Exception e) {
				MessageBox.Show(Language.GetString("picasa", LangKey.upload_failure) + " " + e.ToString());
			}
			uploadUrl = null;
			return false;
		}
	}
}
