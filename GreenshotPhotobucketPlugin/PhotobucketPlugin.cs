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

namespace GreenshotPhotobucketPlugin {
	/// <summary>
	/// This is the GreenshotPhotobucketPlugin base code
	/// </summary>
	[Export(typeof(IGreenshotPlugin))]
	[ExportMetadata("name", "Photobucket plug-in")]
	public class PhotobucketPlugin : AbstractPlugin {
		private static readonly log4net.ILog LOG = log4net.LogManager.GetLogger(typeof(PhotobucketPlugin));
		private static PhotobucketConfiguration config;
		private IGreenshotHost host;
		private ComponentResourceManager resources;

		public PhotobucketPlugin() {
		}

		public override IEnumerable<IDestination> Destinations() {
			yield return new PhotobucketDestination(this, null);
		}

		/// <summary>
		/// Implementation of the IGreenshotPlugin.Initialize
		/// </summary>
		/// <param name="host">Use the IGreenshotPluginHost interface to register events</param>
		/// <param name="metadata">IDictionary<string, object></param>
		/// <returns>true if plugin is initialized, false if not (doesn't show)</returns>
		public override bool Initialize(IGreenshotHost pluginHost, IDictionary<string, object> metadata) {
			this.host = (IGreenshotHost)pluginHost;

			// Get configuration
			config = IniConfig.GetIniSection<PhotobucketConfiguration>();
			resources = new ComponentResourceManager(typeof(PhotobucketPlugin));

			// Register our configuration
			SettingsWindow.RegisterSettingsPage<PhotobucketSettingsPage>("settings_plugins,photobucket.settings_title");
			return true;
		}

		public override void Shutdown() {
			LOG.Debug("Photobucket Plugin shutdown.");
		}

		/// <summary>
		/// Upload the capture to Photobucket
		/// </summary>
		/// <param name="captureDetails"></param>
		/// <param name="surfaceToUpload">ISurface</param>
		/// <param name="uploadURL">out string for the url</param>
		/// <returns>true if the upload succeeded</returns>
		public bool Upload(ICaptureDetails captureDetails, ISurface surfaceToUpload, string albumPath, out string uploadURL) {
			SurfaceOutputSettings outputSettings = new SurfaceOutputSettings(config.OutputFileFormat, config.OutputFileJpegQuality, config.OutputFileAutoReduceColors);
			try {
				string filename = Path.GetFileName(FilenameHelper.GetFilename(config.OutputFileFormat, captureDetails));
				PhotobucketInfo photobucketInfo = null;
			
				// Run upload in the background
				new PleaseWaitForm().ShowAndWait("Photobucket plug-in", Language.GetString("photobucket", LangKey.communication_wait), 
					delegate() {
						photobucketInfo = PhotobucketUtils.UploadToPhotobucket(surfaceToUpload, outputSettings, albumPath, captureDetails.Title, filename);
					}
				);
				// This causes an exeption if the upload failed :)
				LOG.DebugFormat("Uploaded to Photobucket page: " + photobucketInfo.Page);
				uploadURL = null;
				try {
					if (config.UsePageLink) {
						uploadURL = photobucketInfo.Page;
						Clipboard.SetText(photobucketInfo.Page);
					} else {
						uploadURL = photobucketInfo.Original;
						Clipboard.SetText(photobucketInfo.Original);
					}
				} catch (Exception ex) {
					LOG.Error("Can't write to clipboard: ", ex);
				}
				return true;
			} catch (Exception e) {
				LOG.Error(e);
				MessageBox.Show(Language.GetString("photobucket", LangKey.upload_failure) + " " + e.Message);
			}
			uploadURL = null;
			return false;
		}
	}
}
