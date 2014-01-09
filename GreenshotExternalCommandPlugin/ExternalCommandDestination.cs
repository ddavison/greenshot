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
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using Greenshot.IniFile;
using Greenshot.Plugin;
using GreenshotPlugin.Core;
using System.ComponentModel;

namespace ExternalCommand {
	/// <summary>
	/// Description of ExternalCommandDestination.
	/// </summary>
	public class ExternalCommandDestination : AbstractDestination {
		private static log4net.ILog LOG = log4net.LogManager.GetLogger(typeof(ExternalCommandDestination));
		private static ExternalCommandConfiguration config = IniConfig.GetIniSection<ExternalCommandConfiguration>();
		private ExternalCommandData presetCommand;

		public ExternalCommandDestination(ExternalCommandData commando) {
			this.presetCommand = commando;
		}

		public override string Designation {
			get {
				return "External " + presetCommand.Name.Replace(',','_');
			}
		}

		public override string Description {
			get {
				return presetCommand.Name;
			}
		}

		public override IEnumerable<IDestination> DynamicDestinations() {
			yield break;
		}

		public override Image DisplayIcon {
			get {
				return presetCommand.DisplayIcon;
			}
		}

		public override bool IsActive {
			get {
				return presetCommand.IsActive;
			}
		}

		public override ExportInformation ExportCapture(bool manuallyInitiated, ISurface surface, ICaptureDetails captureDetails) {
			ExportInformation exportInformation = new ExportInformation(this.Designation, this.Description);
			SurfaceOutputSettings outputSettings = new SurfaceOutputSettings();

			
			if (presetCommand != null) {
				bool runInBackground = presetCommand.RunInBackground;
				string fullPath = captureDetails.Filename;
				if (fullPath == null) {
					fullPath = ImageOutput.SaveNamedTmpFile(surface, captureDetails, outputSettings);
				}

				string output = null;
				if (runInBackground) {
					Thread commandThread = new Thread(delegate() {
						CallExternalCommand(presetCommand, fullPath, out output);
					});
					commandThread.Name = "Running " + presetCommand;
					commandThread.IsBackground = true;
					commandThread.Start();
					exportInformation.ExportMade = true;
				} else {
					try {
						if (CallExternalCommand(presetCommand, fullPath, out output) == 0) {
							exportInformation.ExportMade = true;
						} else {
							exportInformation.ErrorMessage = output;
						}
					} catch (Exception ex) {
						exportInformation.ErrorMessage = ex.Message;
					}
				}

				//exportInformation.Uri = "file://" + fullPath;
			}
			ProcessExport(exportInformation, surface);
			return exportInformation;
		}

		private int CallExternalCommand(ExternalCommandData commando, string fullPath, out string output) {
			try {
				return CallExternalCommand(commando, fullPath, null, out output);
			} catch (Win32Exception ex) {
				try {
					return CallExternalCommand(commando, fullPath, "runas", out output);
				} catch {
					throw ex;
				}
			}
		}

		private int CallExternalCommand(ExternalCommandData commando, string fullPath, string verb, out string output) {
			string commandline = commando.Commandline;
			string arguments = commando.Arguments;
			output = null;
			if (commandline != null && commandline.Length > 0) {
				Process p = new Process();
				p.StartInfo.FileName = commandline;
				p.StartInfo.Arguments = String.Format(arguments, fullPath);
				p.StartInfo.UseShellExecute = false;
				p.StartInfo.RedirectStandardOutput = true;
				if (verb != null) {
					p.StartInfo.Verb = verb;
				}
				LOG.Info("Starting : " + p.StartInfo.FileName + " " + p.StartInfo.Arguments);
				p.Start();
				p.WaitForExit();
				output = p.StandardOutput.ReadToEnd();
				if (output != null && output.Trim().Length > 0) {
					LOG.Info("Output:\n" + output);
				}
				LOG.Info("Finished : " + p.StartInfo.FileName + " " + p.StartInfo.Arguments);
				return p.ExitCode;
			}
			return -1;
		}
	}
}
