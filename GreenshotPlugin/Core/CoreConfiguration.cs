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
using System.Windows.Forms;
using Greenshot.IniFile;
using Greenshot.Plugin;
using System.Reflection;
using log4net;
using Greenshot.Core;

namespace GreenshotPlugin.Core {
	public enum WindowCaptureMode {
		Screen, GDI, Aero, AeroTransparent, Auto
	}

	public enum BuildStates {
		UNSTABLE,
		RELEASE_CANDIDATE,
		RELEASE
	}
	
	public enum ClickActions {
		DO_NOTHING,
		OPEN_LAST_IN_EXPLORER,
		OPEN_LAST_IN_EDITOR,
		OPEN_SETTINGS,
		SHOW_CONTEXT_MENU
	}

	/// <summary>
	/// A container for the ImageOutputSettings
	/// </summary>
	public class ImageOutputSettings {
		public string Name {
			get;
			set;
		}
		public OutputFormat Format {
			get;
			set;
		}
		public int JPGQuality {
			get;
			set;
		}
		public bool AutoQuantize {
			get;
			set;
		}
		public bool ForceQuantize {
			get;
			set;
		}

		/// <summary>
		/// A list of effects that need to be applied, the string is a key to the effect information
		/// </summary>
		public List<string> Effects {
			get;
			set;
		}
	}

	/// <summary>
	/// Description of CoreConfiguration.
	/// </summary>
	[IniSection("Core", Description="Greenshot core configuration"), Serializable]
	public class CoreConfiguration : IniSection {
		#region General
		[IniProperty("Language", Description = "The language in IETF format (e.g. en-US)")]
		public string Language {
			get;
			set;
		}

		[IniProperty("IsFirstLaunch", Description = "Is this the first time launch?", DefaultValue = "true")]
		public bool IsFirstLaunch {
			get;
			set;
		}

		[IniProperty("NotificationSound", Description = "The wav-file to play when a capture is taken, loaded only once at the Greenshot startup", DefaultValue = "default")]
		public string NotificationSound {
			get;
			set;
		}
		#endregion

		#region Hotkeys
		[IniProperty("RegionHotkey", Description="Hotkey for starting the region capture", DefaultValue="PrintScreen")]
		public string RegionHotkey {
			get;
			set;
		}

		[IniProperty("WindowHotkey", Description="Hotkey for starting the window capture", DefaultValue="Alt + PrintScreen")]
		public string WindowHotkey {
			get;
			set;
		}

		[IniProperty("FullscreenHotkey", Description="Hotkey for starting the fullscreen capture", DefaultValue="Ctrl + PrintScreen")]
		public string FullscreenHotkey {
			get;
			set;
		}

		[IniProperty("LastregionHotkey", Description="Hotkey for starting the last region capture", DefaultValue="Shift + PrintScreen")]
		public string LastregionHotkey {
			get;
			set;
		}

		[IniProperty("IEHotkey", Description="Hotkey for starting the IE capture", DefaultValue="Shift + Ctrl + PrintScreen")]
		public string IEHotkey {
			get;
			set;
		}
		#endregion


		private List<string> outputDestinations = new List<string>();
		[IniProperty("Destinations", Separator=",", Description="Which destinations? Possible options (more might be added by plugins) are: Editor, FileDefault, FileWithDialog, Clipboard, Printer, EMail, Picker", DefaultValue="Picker")]
		public List<string> OutputDestinations {
			get {
				return outputDestinations;
			}
			set {
				outputDestinations = value;
			}
		}

		#region Clipboard
		[IniProperty("ClipboardWritePNG", Description = "Exports to clipboard contain PNG", DefaultValue = "True")]
		public bool ClipboardWritePNG {
			get;
			set;
		}

		[IniProperty("ClipboardWriteHTML", Description = "Exports to clipboard contain HTML", DefaultValue = "False")]
		public bool ClipboardWriteHTML {
			get;
			set;
		}
		[IniProperty("ClipboardWriteHTMLDataUrl", Description = "If HTML is exported to the clipboard, use a Data URL (Inline image)", DefaultValue = "False")]
		public bool ClipboardWriteHTMLDataUrl {
			get;
			set;
		}
		[IniProperty("ClipboardWriteDIB", Description = "Exports to clipboard contain DIB", DefaultValue = "True")]
		public bool ClipboardWriteDIB {
			get;
			set;
		}

		[IniProperty("ClipboardWriteBITMAP", Description = "Exports to clipboard contain a Bitmap", DefaultValue = "True")]
		public bool ClipboardWriteBITMAP {
			get;
			set;
		}

		[IniProperty("EnableSpecialDIBClipboardReader", Expert=true, Description = "Enable a special DIB clipboard reader", DefaultValue = "True")]
		public bool EnableSpecialDIBClipboardReader {
			get;
			set;
		}
		#endregion

		private List<string> pickerDestinations = new List<string>();
		[IniProperty("PickerDestinations", Separator = ",", Description = "Specify which destinations, and in what order, are shown in the destination picker. Empty (default) means all.")]
		public List<string> PickerDestinations {
			get {
				return pickerDestinations;
			}
			set {
				pickerDestinations = value;
			}
		}

		#region Capture
		[IniProperty("CaptureMousepointer", Description = "Should the mouse be captured?", DefaultValue = "true")]
		public bool CaptureMousepointer {
			get;
			set;
		}

		[IniProperty("CaptureWindowsInteractive", Description="Use interactive window selection to capture? (false=Capture active window)", DefaultValue="false")]
		public bool CaptureWindowsInteractive {
			get;
			set;
		}

		[IniProperty("CaptureDelay", Expert = true, Description = "Capture delay in millseconds.", DefaultValue = "100")]
		public int CaptureDelay {
			get;
			set;
		}

		[IniProperty("ScreenCaptureMode", Description = "The capture mode used to capture a screen. (Auto, FullScreen, Fixed)", DefaultValue = "Auto")]
		public ScreenCaptureMode ScreenCaptureMode {
			get;
			set;
		}

		[IniProperty("ScreenToCapture", Description = "The screen number to capture when using ScreenCaptureMode Fixed.", DefaultValue = "1")]
		public int ScreenToCapture {
			get;
			set;
		}

		[IniProperty("WindowCaptureMode", Description = "The capture mode used to capture a Window (Screen, GDI, Aero, AeroTransparent, Auto).", DefaultValue = "Auto")]
		public WindowCaptureMode WindowCaptureMode {
			get;
			set;
		}

		[IniProperty("WindowCaptureAllChildLocations", Expert = true, Description = "Enable/disable capture all children, very slow but will make it possible to use this information in the editor.", DefaultValue = "False")]
		public bool WindowCaptureAllChildLocations {
			get;
			set;
		}

		[IniProperty("DWMBackgroundColor", Description="The background color for a DWM window capture.")]
		public Color DWMBackgroundColor {
			get;
			set;
		}

		[IniProperty("PlayCameraSound", LanguageKey="settings_playsound",Description="Play a camera sound after taking a capture.", DefaultValue="false")]
		public bool PlayCameraSound {
			get;
			set;
		}


		[IniProperty("WindowCaptureRemoveCorners", Expert = true, Description = "Remove the corners from a window capture", DefaultValue = "True")]
		public bool WindowCaptureRemoveCorners {
			get;
			set;
		}

		[IniProperty("ZoomerEnabled", Description = "Sets if the zoomer is enabled", DefaultValue = "True")]
		public bool ZoomerEnabled {
			get;
			set;
		}

		[IniProperty("WindowCornerCutShape", Expert = true, Description = "The cutshape which is used to remove the window corners, is mirrorred for all corners", DefaultValue = "5,3,2,1,1")]
		public List<int> WindowCornerCutShape {
			get;
			set;
		}

		[IniProperty("NoGDICaptureForProduct", Expert = true, Description = "List of products for which GDI capturing doesn't work.", DefaultValue = "IntelliJ,IDEA")]
		public List<string> NoGDICaptureForProduct {
			get;
			set;
		}

		[IniProperty("NoDWMCaptureForProduct", Expert = true, Description = "List of products for which DWM capturing doesn't work.", DefaultValue = "Citrix,ICA,Client")]
		public List<string> NoDWMCaptureForProduct {
			get;
			set;
		}
		#endregion

		#region Output
		[IniProperty("OutputFilePath", Description="Output file path.")]
		public string OutputFilePath {
			get;
			set;
		}

		[IniProperty("OutputFileAllowOverwrite", Description = "If the target file already exists True will make Greenshot always overwrite and False will display a 'Save-As' dialog.", DefaultValue = "true")]
		public bool OutputFileAllowOverwrite {
			get;
			set;
		}

		[IniProperty("OutputFileFilenamePattern", Description = "Filename pattern for screenshot.", DefaultValue = "${capturetime:d\"yyyy-MM-dd HH_mm_ss\"}-${title}")]
		public string OutputFileFilenamePattern {
			get;
			set;
		}

		[IniProperty("OutputFileFormat", Description="Default file type for writing screenshots. (bmp, gif, jpg, png, tiff)", DefaultValue="png")]
		public OutputFormat OutputFileFormat {
			get;
			set;
		}

		[IniProperty("OutputFileReduceColors", Description="If set to true, than the colors of the output file are reduced to 256 (8-bit) colors", DefaultValue="false")]
		public bool OutputFileReduceColors {
			get;
			set;
		}

		[IniProperty("OutputFileAutoReduceColors", Description = "If set to true the amount of colors is counted and if smaller than 256 the color reduction is automatically used.", DefaultValue = "false")]
		public bool OutputFileAutoReduceColors {
			get;
			set;
		}

		[IniProperty("OutputFileCopyPathToClipboard", Description="When saving a screenshot, copy the path to the clipboard?", DefaultValue="true")]
		public bool OutputFileCopyPathToClipboard {
			get;
			set;
		}

		[IniProperty("OutputFileAsFullpath", Description="SaveAs Full path?")]
		public string OutputFileAsFullpath {
			get;
			set;
		}
		
		[IniProperty("OutputFileJpegQuality", Description="JPEG file save quality in %.", DefaultValue="80")]
		public int OutputFileJpegQuality {
			get;
			set;
		}

		[IniProperty("OutputFilePromptQuality", Description="Ask for the quality before saving?", DefaultValue="false")]
		public bool OutputFilePromptQuality {
			get;
			set;
		}

		[IniProperty("OutputFileIncrementingNumber", Description="The number for the ${NUM} in the filename pattern, is increased automatically after each save.", DefaultValue="1")]
		public uint OutputFileIncrementingNumber {
			get;
			set;
		}
		#endregion

		#region Print
		[IniProperty("OutputPrintPromptOptions", LanguageKey="settings_alwaysshowprintoptionsdialog", Description="Ask for print options when printing?", DefaultValue="true")]
		public bool OutputPrintPromptOptions {
			get;
			set;
		}

		[IniProperty("OutputPrintAllowRotate", LanguageKey="printoptions_allowrotate", Description="Allow rotating the picture for fitting on paper?", DefaultValue="false")]
		public bool OutputPrintAllowRotate {
			get;
			set;
		}

		[IniProperty("OutputPrintAllowEnlarge", LanguageKey="printoptions_allowenlarge", Description="Allow growing the picture for fitting on paper?", DefaultValue="false")]
		public bool OutputPrintAllowEnlarge {
			get;
			set;
		}

		[IniProperty("OutputPrintAllowShrink", LanguageKey="printoptions_allowshrink", Description="Allow shrinking the picture for fitting on paper?", DefaultValue="true")]
		public bool OutputPrintAllowShrink {
			get;
			set;
		}

		[IniProperty("OutputPrintCenter", LanguageKey="printoptions_allowcenter", Description="Center image when printing?", DefaultValue="true")]
		public bool OutputPrintCenter {
			get;
			set;
		}

		[IniProperty("OutputPrintInverted", LanguageKey="printoptions_inverted", Description="Print image inverted (use e.g. for console captures)", DefaultValue="false")]
		public bool OutputPrintInverted {
			get;
			set;
		}

		[IniProperty("OutputPrintGrayscale", LanguageKey = "printoptions_printgrayscale", Description = "Force grayscale printing", DefaultValue = "false")]
		public bool OutputPrintGrayscale {
			get;
			set;
		}

		[IniProperty("OutputPrintMonochrome", LanguageKey = "printoptions_printmonochrome", Description = "Force monorchrome printing", DefaultValue = "false")]
		public bool OutputPrintMonochrome {
			get;
			set;
		}

		[IniProperty("OutputPrintMonochromeThreshold", Description = "Threshold for monochrome filter (0 - 255), lower value means less black", DefaultValue = "127")]
		public byte OutputPrintMonochromeThreshold {
			get;
			set;
		}

		[IniProperty("OutputPrintFooter", LanguageKey = "printoptions_timestamp", Description = "Print footer on print?", DefaultValue = "true")]
		public bool OutputPrintFooter {
			get;
			set;
		}

		[IniProperty("OutputPrintFooterPattern", Expert = true, Description = "Footer pattern", DefaultValue = "${capturetime:d\"D\"} ${capturetime:d\"T\"} - ${title}")]
		public string OutputPrintFooterPattern {
			get;
			set;
		}
		#endregion

		#region IE
		[IniProperty("IECapture", Description="Enable/disable IE capture", DefaultValue="True")]
		public bool IECapture {
			get;
			set;
		}

		[IniProperty("IEFieldCapture", Description="Enable/disable IE field capture, very slow but will make it possible to annotate the fields of a capture in the editor.", DefaultValue="False")]
		public bool IEFieldCapture {
			get;
			set;
		}

		[IniProperty("WindowClassesToCheckForIE", Description = "Comma separated list of Window-Classes which need to be checked for a IE instance!", DefaultValue = "AfxFrameOrView70,IMWindowClass")]
		public List<string> WindowClassesToCheckForIE {
			get;
			set;
		}
		#endregion

		#region Network & updates
		[IniProperty("UseProxy", Description = "Use your global proxy?", DefaultValue = "True")]
		public bool UseProxy {
			get;
			set;
		}
		[IniProperty("UpdateCheckInterval", Description="How many days between every update check? (0=no checks)", DefaultValue="1")]
		public int UpdateCheckInterval {
			get;
			set;
		}

		[IniProperty("LastUpdateCheck", Description="Last update check")]
		public DateTime LastUpdateCheck {
			get;
			set;
		}

		[IniProperty("CheckForUnstable", Expert = true, Description = "Also check for unstable version updates", DefaultValue = "False")]
		public bool CheckForUnstable {
			get;
			set;
		}
		#endregion

		#region Admin
		[IniProperty("DisableSettings", Description = "Enable/disable the access to the settings, can only be changed manually in this .ini", DefaultValue = "False")]
		public bool DisableSettings {
			get;
			set;
		}

		[IniProperty("DisableQuickSettings", Description = "Enable/disable the access to the quick settings, can only be changed manually in this .ini", DefaultValue = "False")]
		public bool DisableQuickSettings {
			get;
			set;
		}

		[IniProperty("DisableTrayicon", Description = "Disable the trayicon, can only be changed manually in this .ini", DefaultValue = "False")]
		public bool HideTrayicon {
			get;
			set;
		}

		[IniProperty("ShowExpertCheckbox", Description = "Show expert checkbox in the settings, can only be changed manually in this .ini", DefaultValue = "True")]
		public bool ShowExpertCheckbox {
			get;
			set;
		}

		[IniProperty("IncludePlugins", Description = "Comma separated list of Plugins which are allowed. If something in the list, than every plugin not in the list will not be loaded!")]
		public List<string> IncludePlugins {
			get;
			set;
		}

		[IniProperty("ExcludePlugins", Description = "Comma separated list of Plugins which are NOT allowed.")]
		public List<string> ExcludePlugins {
			get;
			set;
		}

		[IniProperty("ExcludeDestinations", Description = "Comma separated list of destinations which should be disabled.", DefaultValue = "OneNote")]
		public List<string> ExcludeDestinations {
			get;
			set;
		}
		#endregion

		#region Advanced

		[IniProperty("OptimizeForRDP", Expert = true, Description = "Make some optimizations for usage with remote desktop 'like' applications", DefaultValue = "False")]
		public bool OptimizeForRDP {
			get;
			set;
		}

		[IniProperty("MinimizeWorkingSetSize", Expert = true, Description = "Optimize memory footprint, but with a performance penalty!", DefaultValue = "False")]
		public bool MinimizeWorkingSetSize {
			get;
			set;
		}

		#endregion

		#region Title-fix processor
		[IniProperty("ActiveTitleFixes", Description="The title-fixes that are active.")]
		public List<string> ActiveTitleFixes {
			get;
			set;
		}

		[IniProperty("TitleFixMatcher", Description="The regular expressions to match the title with.")]
		public Dictionary<string, string> TitleFixMatcher {
			get;
			set;
		}

		[IniProperty("TitleFixReplacer", Description="The replacements for the matchers.")]
		public Dictionary<string, string> TitleFixReplacer {
			get;
			set;
		}
		#endregion

		[IniProperty("ExperimentalFeatures", Description="A list of experimental features, this allows us to test certain features before releasing them.", ExcludeIfNull=true)]
		public List<string> ExperimentalFeatures {
			get;
			set;
		}

		[IniProperty("ShowExpertSettings", Description = "If set to true, show expert settings", DefaultValue = "False")]
		public bool ShowExpertSettings {
			get;
			set;
		}

		#region TrayIcon
		[IniProperty("ShowTrayNotification", LanguageKey = "settings_shownotify", Description = "Show a notification from the systray when a capture is taken.", DefaultValue = "true")]
		public bool ShowTrayNotification {
			get;
			set;
		}

		[IniProperty("LeftClickAction", Description = "Specify what action is made if the tray icon is left clicked, if a double-click action is specified this action is initiated after a delay (configurable via the windows double-click speed)", DefaultValue = "SHOW_CONTEXT_MENU")]
		public ClickActions LeftClickAction {
			get;
			set;
		}

		[IniProperty("DoubleClickAction", Description = "Specify what action is made if the tray icon is double clicked", DefaultValue = "OPEN_LAST_IN_EXPLORER")]
		public ClickActions DoubleClickAction {
			get;
			set;
		}

		[IniProperty("MaxMenuItemLength", Description = "Maximum length of submenu items in the context menu, making this longer might cause context menu issues on dual screen systems.", DefaultValue = "25")]
		public int MaxMenuItemLength {
			get;
			set;
		}


		[IniProperty("ThumnailPreview", Expert = true, Description = "Enable/disable thumbnail previews", DefaultValue = "True")]
		public bool ThumnailPreview {
			get;
			set;
		}
		#endregion

		#region MAPI
		[IniProperty("MailApiSubjectPattern", Description = "The subject pattern for the email destination (settings for Outlook can be found under the Office section)", DefaultValue = "${title}")]
		public string MailApiSubjectPattern {
			get;
			set;
		}
		[IniProperty("MailApiTo", Description = "The 'to' field for the email destination (settings for Outlook can be found under the Office section)", DefaultValue = "")]
		public string MailApiTo {
			get;
			set;
		}

		[IniProperty("MailApiCC", Description = "The 'CC' field for the email destination (settings for Outlook can be found under the Office section)", DefaultValue = "")]
		public string MailApiCC {
			get;
			set;
		}

		[IniProperty("MailApiBCC", Description = "The 'BCC' field for the email destination (settings for Outlook can be found under the Office section)", DefaultValue = "")]
		public string MailApiBCC {
			get;
			set;
		}
		#endregion

		[IniProperty("OptimizePNGCommand", Expert = true, Description = "Optional command to execute on a temporary PNG file, the command should overwrite the file and Greenshot will read it back. Note: this command is also executed when uploading PNG's!", DefaultValue = "")]
		public string OptimizePNGCommand {
			get;
			set;
		}

		[IniProperty("OptimizePNGCommandArguments", Expert = true, Description = "Arguments for the optional command to execute on a PNG, {0} is replaced by the temp-filename from Greenshot. Note: Temp-file is deleted afterwards by Greenshot.", DefaultValue = "\"{0}\"")]
		public string OptimizePNGCommandArguments {
			get;
			set;
		}

		[IniProperty("LastSaveWithVersion", Description = "Version of Greenshot which created this .ini")]
		public string LastSaveWithVersion {
			get;
			set;
		}

		[IniProperty("ProcessEXIFOrientation", Expert=true, Description = "When reading images from files or clipboard, use the EXIF information to correct the orientation", DefaultValue = "True")]
		public bool ProcessEXIFOrientation {
			get;
			set;
		}

		private Rectangle lastCapturedRegion = Rectangle.Empty;
		[IniProperty("LastCapturedRegion", Description = "Location of the last captured region")]
		public Rectangle LastCapturedRegion {
			get {
				return lastCapturedRegion;
			}
			set {
				lastCapturedRegion = value;
			}
		}

		[IniProperty("LogLevel", Expert = true, Description = "Level for logging", DefaultValue = "WARN")]
		public LogLevel LogLevel {
			get;
			set;
		}

		// Specifies what THIS build is
		public BuildStates BuildState = BuildStates.UNSTABLE;

		/// <summary>
		/// A helper method which returns true if the supplied experimental feature is enabled
		/// </summary>
		/// <param name="experimentalFeature"></param>
		/// <returns></returns>
		public bool isExperimentalFeatureEnabled(string experimentalFeature) {
			return (ExperimentalFeatures != null && ExperimentalFeatures.Contains(experimentalFeature));
		}

		/// <summary>
		/// Supply values we can't put as defaults
		/// </summary>
		/// <param name="property">The property to return a default for</param>
		/// <returns>object with the default value for the supplied property</returns>
		public override object GetDefault(string property) {
			switch(property) {
				case "PluginWhitelist":
				case "PluginBacklist":
					return new List<string>();
				case "OutputFileAsFullpath":
					if (IniConfig.IsPortable) {
						return Path.Combine(Application.StartupPath, @"..\..\Documents\Pictures\Greenshots\dummy.png");
					} else {
						return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop),"dummy.png");
					}
				case "OutputFilePath":
					if (IniConfig.IsPortable) {
						string pafOutputFilePath = Path.Combine(Application.StartupPath, @"..\..\Documents\Pictures\Greenshots");
						if (!Directory.Exists(pafOutputFilePath)) {
							try {
								Directory.CreateDirectory(pafOutputFilePath);
								return pafOutputFilePath;
							} catch (Exception ex) {
								LOG.Warn(ex);
								// Problem creating directory, fallback to Desktop
							}
						} else {
							return pafOutputFilePath;
						}
					}
					return Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
				case "DWMBackgroundColor":
					return Color.Transparent;
				case "ActiveTitleFixes":
					List<string> activeDefaults = new List<string>();
					activeDefaults.Add("Firefox");
					activeDefaults.Add("IE");
					activeDefaults.Add("Chrome");
					return activeDefaults; 
				case "TitleFixMatcher":
					Dictionary<string, string> matcherDefaults = new Dictionary<string, string>();
					matcherDefaults.Add("Firefox", " - Mozilla Firefox.*");
					matcherDefaults.Add("IE", " - (Microsoft|Windows) Internet Explorer.*");
					matcherDefaults.Add("Chrome", " - Google Chrome.*");
					return matcherDefaults; 
				case "TitleFixReplacer":
					Dictionary<string, string> replacerDefaults = new Dictionary<string, string>();
					replacerDefaults.Add("Firefox", "");
					replacerDefaults.Add("IE", "");
					replacerDefaults.Add("Chrome", "");
					return replacerDefaults; 
			}
			return null;
		}
		
		/// <summary>
		/// This method will be called before converting the property, making to possible to correct a certain value
		/// Can be used when migration is needed
		/// </summary>
		/// <param name="propertyName">The name of the property</param>
		/// <param name="propertyValue">The string value of the property</param>
		/// <returns>string with the propertyValue, modified or not...</returns>
		public override string PreCheckValue(string propertyName, string propertyValue) {
			// Changed the separator, now we need to correct this
			if ("Destinations".Equals(propertyName)) {
				if (propertyValue != null) {
					return propertyValue.Replace('|',',');
				}
			}
			if("OutputFilePath".Equals(propertyName)) {
				if (string.IsNullOrEmpty(propertyValue)) {
					return null;
				}
			}
			return base.PreCheckValue(propertyName, propertyValue);
		}

		/// <summary>
		/// This method will be called after reading the configuration, so eventually some corrections can be made
		/// </summary>
		public override void AfterLoad() {
			base.AfterLoad();
			// Comment with releases
			// CheckForUnstable = true;

			if (string.IsNullOrEmpty(LastSaveWithVersion)) {
				// Store version, this can be used later to fix settings after an update
				LastSaveWithVersion = Assembly.GetEntryAssembly().GetName().Version.ToString();
				// Disable the AutoReduceColors as it causes issues with Mozzila applications and some others
				OutputFileAutoReduceColors = false;
			}

			if (OutputDestinations == null) {
				OutputDestinations = new List<string>();
			}

			// Make sure there is an output!
			if (OutputDestinations.Count == 0) {
				OutputDestinations.Add("Editor");
			}

			// Prevent both settings at once, bug #3435056
			if (OutputDestinations.Contains("Clipboard") && OutputFileCopyPathToClipboard) {
				OutputFileCopyPathToClipboard = false;
			}

			// Make sure the lists are lowercase, to speedup the check
			if (NoGDICaptureForProduct != null) {
				// Fix error in configuration
				if (NoGDICaptureForProduct.Count == 1) {
					if ("intellij idea".Equals(NoGDICaptureForProduct[0])) {
						NoGDICaptureForProduct[0] = "intellij,idea";
						this.IsDirty = true;
					}
				}
				for (int i = 0; i < NoGDICaptureForProduct.Count; i++) {
					NoGDICaptureForProduct[i] = NoGDICaptureForProduct[i].ToLower();
				}
			}
			if (NoDWMCaptureForProduct != null) {
				// Fix error in configuration
				if (NoDWMCaptureForProduct.Count == 1) {
					if ("citrix ica client".Equals(NoDWMCaptureForProduct[0])) {
						NoDWMCaptureForProduct[0] = "citrix,ica,client";
						this.IsDirty = true;
					}
				}
				for(int i=0; i< NoDWMCaptureForProduct.Count; i++) {
					NoDWMCaptureForProduct[i] = NoDWMCaptureForProduct[i].ToLower();
				}
			}
		}

		/// <summary>
		/// Used to check the designmode during a constructor
		/// </summary>
		/// <returns></returns>
		public static bool IsInDesignMode {
			get {
				return (Application.ExecutablePath.IndexOf("devenv.exe", StringComparison.OrdinalIgnoreCase) > -1) || (Application.ExecutablePath.IndexOf("sharpdevelop.exe", StringComparison.OrdinalIgnoreCase) > -1 || (Application.ExecutablePath.IndexOf("wdexpress.exe", StringComparison.OrdinalIgnoreCase) > -1));
			}
		}

		public static string PortableAppPath {
			get {
				string applicationStartupPath = "";
				try {
					applicationStartupPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
				} catch {
					applicationStartupPath = @".";
				}

				return Path.Combine(applicationStartupPath, @"App\Greenshot");
			}
		}

		/// <summary>
		/// Returns true if we are a PortableApp
		/// </summary>
		public static bool IsPortableApp {
			get {
				if (Directory.Exists(PortableAppPath)) {
					return true;
				}
				return false;
			}
		}
	}
}