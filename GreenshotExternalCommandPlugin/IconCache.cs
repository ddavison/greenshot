using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Greenshot.IniFile;
using GreenshotPlugin.Core;

namespace ExternalCommand {
	public static class IconCache {
		private static Dictionary<string, Image> iconCache = new Dictionary<string, Image>();
		private static ExternalCommandConfiguration config = IniConfig.GetIniSection<ExternalCommandConfiguration>();
		private static log4net.ILog LOG = log4net.LogManager.GetLogger(typeof(IconCache));

		public static Image IconForCommand(ExternalCommandData command) {
			if (!iconCache.ContainsKey(command.Name)) {
				Image icon = null;
				if (File.Exists(command.Commandline)) {
					try {
						icon = PluginUtils.GetExeIcon(command.Commandline, 0);
					} catch (Exception ex) {
						LOG.Warn("Problem loading icon for " + command.Commandline, ex);
					}
				}
				// Also add null to the cache if nothing is found
				iconCache.Add(command.Name, icon);
			}
			if (iconCache.ContainsKey(command.Name)) {
				return iconCache[command.Name];
			}
			return null;
		}
	}
}
