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
using System.IO;
using System.Reflection;
using System.Windows.Forms;

using Greenshot.IniFile;
using log4net;
using System;

namespace GreenshotPlugin.Core {
	/// <summary>
	/// Description of LogHelper.
	/// </summary>
	public class LogHelper {
		private const string LOG4NET_FILE = "log4net.xml";
		private const string LOG4NET_PORTABLE_FILE = "log4net-portable.xml";
		private static bool isLog4NetConfigured = false;
		private const string INIT_MESSAGE = "Greenshot initialization of log system failed";
		public static bool isInitialized {
			get {
				return isLog4NetConfigured;
			}
		}

		// Initialize Log4J
		public static string InitializeLog4NET() {
			// Setup log4j, currently the file is called log4net.xml

			if (System.Diagnostics.Debugger.IsAttached || CoreConfiguration.IsInDesignMode) {
				CoreConfiguration config = IniConfig.GetIniSection<CoreConfiguration>();
				config.LogLevel = LogLevel.INFO;
				LogManager.Configure();
			} else if (CoreConfiguration.IsPortableApp) {
				string logfile = Path.Combine(CoreConfiguration.PortableAppPath, @"Greenshot\Greenshot.log");
				LogManager.Configure(
					//where to put the logs : folders will be automatically created
					logfile,
					// limit the file sizes to 500kb, 0 = no limiting
					500
					);
				return logfile;
			} else {
				string logfile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Greenshot\Greenshot.log");
				LogManager.Configure(
					//where to put the logs : folders will be automatically created
					logfile,
					// limit the file sizes to 500kb, 0 = no limiting
					500
					);
				return logfile;
			}

			return null;
		}
	}
}
