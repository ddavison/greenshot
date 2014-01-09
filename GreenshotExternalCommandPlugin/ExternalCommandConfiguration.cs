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
using System.Linq;
using Greenshot.IniFile;
using GreenshotPlugin.Core;
using System.Drawing;

namespace ExternalCommand {
	/// <summary>
	/// To simplify the usage of the External command, this class contains all the data.
	/// When writing to the ini, every value from the class is mapped to its own dictionary.
	/// </summary>
	public class ExternalCommandData {
		public Image DisplayIcon {
			get {
				return IconCache.IconForCommand(this);
			}
		}

		public string Name {
			get;
			set;
		}
		public string Commandline {
			get;
			set;
		}
		public string Arguments {
			get;
			set;
		}
		public bool RunInBackground {
			get;
			set;
		}
		public bool IsActive {
			get;
			set;
		}
	}

	/// <summary>
	/// Description of FlickrConfiguration.
	/// </summary>
	[IniSection("ExternalCommand", Description="Greenshot ExternalCommand Plugin configuration")]
	public class ExternalCommandConfiguration : IniSection {
		private const string MSPAINT = "MS Paint";
		private static string paintPath;
		private static bool hasPaint = false;

		private const string PAINTDOTNET = "Paint.NET";
		private static string paintDotNetPath;
		private static bool hasPaintDotNet = false;
		static ExternalCommandConfiguration() {
			try {
				paintPath = PluginUtils.GetExePath("pbrush.exe");
				hasPaint = !string.IsNullOrEmpty(paintPath) && File.Exists(paintPath);
				paintDotNetPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"Paint.NET\PaintDotNet.exe");
				hasPaintDotNet = !string.IsNullOrEmpty(paintDotNetPath) && File.Exists(paintDotNetPath);
				// check for non 64-bit
				if (!hasPaintDotNet) {
					paintDotNetPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"Paint.NET\PaintDotNet.exe");
					hasPaintDotNet = !string.IsNullOrEmpty(paintDotNetPath) && File.Exists(paintDotNetPath);
				}
			} catch {
			}
		}

		[IniProperty("Commands", Description = "The commands that are available.")]
		public List<string> commands;

		[IniProperty("Commandline", Description = "The commandline for the output command.")]
		public Dictionary<string, string> commandlines;

		[IniProperty("Argument", Description = "The arguments for the output command.")]
		public Dictionary<string, string> argumentsList;

		[IniProperty("RunInbackground", Description = "Should the command be started in the background.")]
		public Dictionary<string, bool> runInbackgroundList;

		[IniProperty("IsActive", Description = "Is the destination active")]
		public Dictionary<string, bool> isActiveList;

		/// <summary>
		/// This is were we store all the external commands information while running
		/// In the AfterLoad this List is created, in the BeforeSave the list is written to the other properties
		/// </summary>
		public List<ExternalCommandData> ExternalCommands {
			get;
			set;
		}

		/// <summary>
		/// Here we map the loaded properties to real objects, so later on the logic is a lot easier.
		/// BeforeSave does the reverse!
		/// </summary>
		public override void AfterLoad() {
			base.AfterLoad();
			ExternalCommands = new List<ExternalCommandData>();
			if (commands != null) {
				foreach (string command in commands) {
					string arguments = "\"{0}\"";
					bool runInBackground = false;
					bool isActive = true;
					string commandline = null;
					argumentsList.TryGetValue(command, out arguments);
					runInbackgroundList.TryGetValue(command, out runInBackground);
					isActiveList.TryGetValue(command, out isActive);
					if (commandlines.TryGetValue(command, out commandline)) {
						ExternalCommands.Add(new ExternalCommandData {
							Name = command,
							Arguments = arguments,
							RunInBackground = runInBackground,
							IsActive = isActive,
							Commandline = commandline
						});
					}
				}
			}
			if (ExternalCommands.Count < 2) {
				if (hasPaintDotNet) {
					ExternalCommands.Add(new ExternalCommandData {
						Name = PAINTDOTNET,
						Commandline = paintDotNetPath,
						Arguments = "\"{0}\"",
						IsActive = true,
						RunInBackground = true
					});
				}
				if (hasPaint) {
					ExternalCommands.Add(new ExternalCommandData {
						Name = MSPAINT,
						Commandline = paintPath,
						Arguments = "\"{0}\"",
						IsActive = true,
						RunInBackground = true
					});
				}
			}
			ExternalCommands = ExternalCommands.OrderBy(item => item.Name).ToList();
			// Clear all data, we don't need it during runtime.
			commands = null;
			argumentsList = null;
			commandlines = null;
			runInbackgroundList = null;
			isActiveList = null;
		}

		/// <summary>
		/// Here we map the real ExternalCommandData objects to the properties that are written to the .ini file
		/// AfterLoad does the reverse.
		/// </summary>
		public override void BeforeSave() {
			base.BeforeSave();
			// Create new empty dictionaries
			commands = new List<string>();
			argumentsList = new Dictionary<string, string>();
			commandlines = new Dictionary<string, string>();
			runInbackgroundList = new Dictionary<string, bool>();
			isActiveList = new Dictionary<string, bool>();
			// Map each Property of the ExternalCommandData to the right dictionary
			foreach (ExternalCommandData command in ExternalCommands) {
				commands.Add(command.Name);
				argumentsList.Add(command.Name, command.Arguments);
				runInbackgroundList.Add(command.Name, command.RunInBackground);
				commandlines.Add(command.Name, command.Commandline);
				isActiveList.Add(command.Name, command.IsActive);
			}
		}
	}
}
