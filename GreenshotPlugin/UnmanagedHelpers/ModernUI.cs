using Greenshot.Interop;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
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
namespace GreenshotPlugin.UnmanagedHelpers {
	/// <summary>
	/// This class contains all the Windows 8 (and up) Modern UI (Metro) code
	/// </summary>
	public class ModernUI {
		public const string MODERNUI_WINDOWS_CLASS = "Windows.UI.Core.CoreWindow";
		public const string MODERNUI_APPLAUNCHER_CLASS = "ImmersiveLauncher";
		public const string MODERNUI_GUTTER_CLASS = "ImmersiveGutter";
		// All currently known classes: "ImmersiveGutter", "Snapped Desktop", "ImmersiveBackgroundWindow","ImmersiveLauncher","Windows.UI.Core.CoreWindow","ApplicationManager_ImmersiveShellWindow","SearchPane","MetroGhostWindow","EdgeUiInputWndClass", "NativeHWNDHost", "Shell_CharmWindow"
		private static IAppVisibility appVisibility = null;

		static ModernUI() {
			try {
				// Only try to instanciate when Windows 8 or later.
				if (Environment.OSVersion.Version.Major >= 6 && Environment.OSVersion.Version.Minor >= 2) {
					appVisibility = COMWrapper.CreateInstance<IAppVisibility>();					
				}
			} catch {
			}
		}

		public static bool AppVisible(Rectangle windowBounds) {
			foreach (Screen screen in Screen.AllScreens) {
				if (screen.Bounds.Contains(windowBounds)) {
					if (windowBounds.Equals(screen.Bounds)) {
						// Fullscreen, it's "visible" when AppVisibilityOnMonitor says yes
						// Although it might be the other App, this is not "very" important
						RECT rect = new RECT(screen.Bounds);
						IntPtr monitor = User32.MonitorFromRect(ref rect, User32.MONITOR_DEFAULTTONULL);
						if (monitor != IntPtr.Zero) {
							if (appVisibility != null) {
								MONITOR_APP_VISIBILITY monitorAppVisibility = appVisibility.GetAppVisibilityOnMonitor(monitor);
								//LOG.DebugFormat("App {0} visible: {1} on {2}", Text, monitorAppVisibility, screen.Bounds);
								if (monitorAppVisibility == MONITOR_APP_VISIBILITY.MAV_APP_VISIBLE) {
									return true;
								}
							}
						}
					} else {
						// Is only partly on the screen, when this happens the app is allways visible!
						return true;
					}
				}
			}
			return false;
		}

		/// <summary>
		/// Get the hWnd for the AppLauncer
		/// </summary>
		public static IntPtr AppLauncher {
			get {
				if (appVisibility == null) {
					return IntPtr.Zero;
				}
				return User32.FindWindow(MODERNUI_APPLAUNCHER_CLASS, null);
			}
		}

		/// <summary>
		/// Return true if the app-launcher is visible
		/// </summary>
		public static bool IsLauncherVisible {
			get {
				if (appVisibility != null) {
					return appVisibility.IsLauncherVisible;
				}
				return false;
			}
		}

		/// <summary>
		/// Retrieve all Metro app hWnd's
		/// </summary>
		public static IEnumerable<IntPtr> MetroApps {
			get {
				// if the appVisibility != null we have Windows 8.
				if (appVisibility == null) {
					yield break;
				}
				IntPtr nextHandle = User32.FindWindow(MODERNUI_WINDOWS_CLASS, null);
				while (nextHandle != IntPtr.Zero) {
					yield return nextHandle;
					nextHandle = User32.FindWindowEx(IntPtr.Zero, nextHandle, MODERNUI_WINDOWS_CLASS, null);
				};
				// check for gutter
				IntPtr gutterHandle = User32.FindWindow(MODERNUI_GUTTER_CLASS, null);
				if (gutterHandle != IntPtr.Zero) {
					yield return gutterHandle;
				}

			}
		}
	}
}
