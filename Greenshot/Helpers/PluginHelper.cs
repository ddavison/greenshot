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
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

using Greenshot.Configuration;
using Greenshot.Plugin;
using GreenshotPlugin.Core;
using Greenshot.IniFile;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Linq;

namespace Greenshot.Helpers {
	public class PluginInfo {
		public string Name {
			get;
			set;
		}
		public string Version {
			get;
			set;
		}
		public string Location {
			get;
			set;
		}
		public string CreatedBy {
			get;
			set;
		}
	}
	/// <summary>
	/// The PluginHelper takes care of all plugin related functionality
	/// </summary>
	[Serializable]
	public class PluginHelper : IGreenshotHost {
		private static readonly log4net.ILog LOG = log4net.LogManager.GetLogger(typeof(PluginHelper));
		private static CoreConfiguration conf = IniConfig.GetIniSection<CoreConfiguration>();

		private static string pluginPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Application.ProductName + @"\Plugins");
		private static string applicationPath = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), @"Plugins");
		private static string pafPath =  Path.Combine(Application.StartupPath, @"App\Greenshot\Plugins");
		private static readonly PluginHelper instance = new PluginHelper();
		public static PluginHelper Instance {
			get {
				return instance;
			}
		}

		[ImportMany]
		public IEnumerable<Lazy<IGreenshotPlugin, IDictionary<string, object>>> Plugins {
			get;
			set;
		}

		private PluginHelper() {
			PluginUtils.Host = this;
		}
		
		public Form GreenshotForm {
			get {
				return MainForm.Instance;
			}
		}

		public NotifyIcon NotifyIcon {
			get {
				return MainForm.Instance.NotifyIcon;
			}
		}

		public bool HasPlugins {
			get {
				return (Plugins != null);
			}
		}

		public void Shutdown() {
			foreach (var plugin in Plugins) {
				if (plugin.IsValueCreated) {
					plugin.Value.Shutdown();
					plugin.Value.Dispose();
				}
			}
		}

		/// <summary>
		/// A helper method to get a custom attribute from an assembly and invoke a lambda on it.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="assembly"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		private static string GetAssemblyAttribute<T>(Assembly assembly, Func<T, string> value) where T : Attribute {
			T attribute = (T)Attribute.GetCustomAttribute(assembly, typeof(T));
			if (attribute != null) {
				return value.Invoke(attribute);
			}
			return null;
		}

		/// <summary>
		/// Create a list of PluginInfo for all available plugins
		/// </summary>
		/// <returns></returns>
		public List<PluginInfo> CreatePluginInfos() {
			List<PluginInfo> pluginInfos = new List<PluginInfo>();
			foreach (var plugin in Plugins) {
				pluginInfos.Add(CreatePluginInfo(plugin));
			}
			return pluginInfos;
		}

		/// <summary>
		/// Create an object that has all information for showing a plugin
		/// </summary>
		/// <param name="plugin"></param>
		/// <returns>PluginInfo</returns>
		public PluginInfo CreatePluginInfo(Lazy<IGreenshotPlugin, IDictionary<string, object>> plugin) {
			PluginInfo info = new PluginInfo();
			try {
				string location = plugin.Value.GetType().Assembly.Location;
				info.Location = location;
				object name;
				if (!plugin.Metadata.TryGetValue("name", out name)) {
					name = GetAssemblyAttribute<AssemblyProductAttribute>(plugin.Value.GetType().Assembly, a => a.Product);
					if (string.IsNullOrEmpty((string)name)) {
						name = Path.GetFileNameWithoutExtension(location);
					}
				}
				info.Name = (string)name;
				object version;
				if (!plugin.Metadata.TryGetValue("version", out version)) {
					version = plugin.Value.GetType().Assembly.GetName().Version.ToString();
				}
				info.Version = (string)version;
				object createdBy;
				if (!plugin.Metadata.TryGetValue("createdBy", out createdBy)) {
					createdBy = GetAssemblyAttribute<AssemblyCompanyAttribute>(plugin.Value.GetType().Assembly, a => a.Company);
				}
				info.CreatedBy = (string)createdBy;
			} catch (Exception ex) {
				LOG.Error("Error displaying plugin: ", ex);
			}
			return info;
		}

		/// <summary>
		/// Add plugins information to the Listview
		/// </summary>
		/// <param name="listview"></param>
		public void FillListview(ListView listview) {
			foreach (var plugin in Plugins) {
				var pluginInfo = CreatePluginInfo(plugin);
				ListViewItem item = new ListViewItem(pluginInfo.Name);
				item.SubItems.Add(pluginInfo.Version);
				item.SubItems.Add(pluginInfo.CreatedBy);
				item.SubItems.Add(pluginInfo.Location);
				item.Tag = plugin;
				listview.Items.Add(item);
			}
		}
		
		public bool isSelectedItemConfigurable(ListView listview) {
			if (listview.SelectedItems.Count > 0) {
				Lazy<IGreenshotPlugin, IDictionary<string, object>> plugin = listview.SelectedItems[0].Tag as Lazy<IGreenshotPlugin, IDictionary<string, object>>;
				if (plugin != null && plugin.IsValueCreated) {
					return plugin.Value.Configurable;
				}
			}
			return false;
		}
		
		public void ConfigureSelectedItem(ListView listview) {
			if (listview.SelectedItems.Count > 0) {
				Lazy<IGreenshotPlugin, IDictionary<string, object>> plugin = listview.SelectedItems[0].Tag as Lazy<IGreenshotPlugin, IDictionary<string, object>>;
				if (plugin != null && plugin.IsValueCreated) {
					plugin.Value.Configure();
				}
			}
		}

		#region Implementation of IGreenshotPluginHost
		
		/// <summary>
		/// Create a Thumbnail
		/// </summary>
		/// <param name="image">Image of which we need a Thumbnail</param>
		/// <returns>Image with Thumbnail</returns>
		public Image GetThumbnail(Image image, int width, int height) {
			return image.GetThumbnailImage(width, height,  new Image.GetThumbnailImageAbort(ThumbnailCallback), IntPtr.Zero);
		}

		///  <summary>
		/// Required for GetThumbnail, but not used
		/// </summary>
		/// <returns>true</returns>
		private bool ThumbnailCallback() {
			return true;
		}

		public ContextMenuStrip MainMenu {
			get {
				return MainForm.Instance.MainMenu;
			}
		}

		public IDestination GetDestination(string designation) {
			return DestinationHelper.GetDestination(designation);
		}
		public List<IDestination> GetAllDestinations() {
			return DestinationHelper.GetAllDestinations();
		}

		public ExportInformation ExportCapture(bool manuallyInitiated, string designation, ISurface surface, ICaptureDetails captureDetails) {
			return DestinationHelper.ExportCapture(manuallyInitiated, designation, surface, captureDetails);
		}

		/// <summary>
		/// Make Capture with specified Handler
		/// </summary>
		/// <param name="captureMouseCursor">bool false if the mouse should not be captured, true if the configuration should be checked</param>
		/// <param name="destination">IDestination</param>
		public void CaptureRegion(bool captureMouseCursor, IDestination destination) {
			CaptureHelper.CaptureRegion(captureMouseCursor, destination);
		}

		/// <summary>
		/// Use the supplied image, and handle it as if it's captured.
		/// </summary>
		/// <param name="imageToImport">Image to handle</param>
		public void ImportCapture(ICapture captureToImport) {
			MainForm.Instance.BeginInvoke((MethodInvoker)delegate {
				CaptureHelper.ImportCapture(captureToImport);
			});
		}
		
		/// <summary>
		/// Get an ICapture object, so the plugin can modify this
		/// </summary>
		/// <returns></returns>
		public ICapture GetCapture(Image imageToCapture) {
			Capture capture = new Capture(imageToCapture);
			capture.CaptureDetails = new CaptureDetails();
			capture.CaptureDetails.CaptureMode = CaptureMode.Import;
			capture.CaptureDetails.Title = "Imported";
			return capture;
		}
		#endregion

		#region Plugin loading
		
		private bool isNewer(string version1, string version2) {
			string [] version1Parts = version1.Split('.');
			string [] version2Parts = version2.Split('.');
			int parts = Math.Min(version1Parts.Length, version2Parts.Length);
			for(int i=0; i < parts; i++) {
				int v1 = Convert.ToInt32(version1Parts[i]);
				int v2 = Convert.ToInt32(version2Parts[i]);
				if (v1 > v2) {
					return true;
				}
				if (v1 < v2) {
					return false;
				}
			}
			return false;
		}

		public void LoadPlugins() {

			// Create the catalog for the plugin "locations"

			var catalog = new AggregateCatalog();

			// Plugins can be defined inside greenshot itself
			catalog.Catalogs.Add(new AssemblyCatalog(System.Reflection.Assembly.GetExecutingAssembly()));

			if (IniConfig.IsPortable && Directory.Exists(pafPath)) {
				// Portable
				foreach (string pluginFile in Directory.GetFiles(pafPath, "*.gsp", SearchOption.AllDirectories)) {
					catalog.Catalogs.Add(new SafeDirectoryCatalog(Path.GetDirectoryName(pluginFile), "*.gsp"));
				}
			} else {
				// Plugin path
				if (Directory.Exists(pluginPath)) {
					foreach (string pluginFile in Directory.GetFiles(pluginPath, "*.gsp", SearchOption.AllDirectories)) {
						catalog.Catalogs.Add(new SafeDirectoryCatalog(Path.GetDirectoryName(pluginFile), "*.gsp"));
					}
				}
	
				if (Directory.Exists(applicationPath)) {
					foreach(string pluginFile in Directory.GetFiles(applicationPath, "*.gsp", SearchOption.AllDirectories)) {
						catalog.Catalogs.Add(new SafeDirectoryCatalog(Path.GetDirectoryName(pluginFile), "*.gsp"));
					}
				}
			}
			//Create the CompositionContainer with the parts in the catalog
			CompositionContainer container = new CompositionContainer(catalog);

			//Fill the imports of this object
			container.ComposeParts(this);
			foreach (var plugin in Plugins) {
				try {
					LOG.InfoFormat("Loaded {0}", plugin.Metadata["name"]);
					plugin.Value.Initialize(this, plugin.Metadata);
				} catch (Exception ex) {
					LOG.Error("Error initializing plugin: ", ex);
				}
			}
		}
		#endregion
	}

	/// <summary>
	/// This makes sure we don't get problems loading plugins
	/// http://stackoverflow.com/questions/4144683/handle-reflectiontypeloadexception-during-mef-composition
	/// </summary>
	public class SafeDirectoryCatalog : ComposablePartCatalog {
		private static readonly log4net.ILog LOG = log4net.LogManager.GetLogger(typeof(SafeDirectoryCatalog));
		private readonly AggregateCatalog _catalog;

		public SafeDirectoryCatalog(string directory) : this(directory, "*.dll") {
		}
		public SafeDirectoryCatalog(string directory, string pattern) {
			var files = Directory.EnumerateFiles(directory, pattern, SearchOption.AllDirectories);

			_catalog = new AggregateCatalog();

			foreach (var file in files) {
				try {
					var asmCat = new AssemblyCatalog(file);

					//Force MEF to load the plugin and figure out if there are any exports
					// good assemblies will not throw the RTLE exception and can be added to the catalog
					if (asmCat.Parts.ToList().Count > 0) {
						_catalog.Catalogs.Add(asmCat);
					}
				} catch (Exception) {
					LOG.ErrorFormat("Error loading {0}", file);
				}
			}
		}

		public override IQueryable<ComposablePartDefinition> Parts {
			get {
				return _catalog.Parts;
			}
		}
	}
}
