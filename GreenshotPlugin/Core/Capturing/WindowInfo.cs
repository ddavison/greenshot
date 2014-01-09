
using GreenshotPlugin.UnmanagedHelpers;
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
using System.Text;
using System.Windows.Media;
using GreenshotPlugin.Core.Imaging;
using System.Drawing;
using System.Windows.Forms;

namespace GreenshotPlugin.Core.Capturing {

	/// <summary>
	/// This class holds all the available information on a Window.
	/// The information is retrieved & updated "automatically" by the WinEventHook
	/// </summary>
	public class WindowInfo : INotifyPropertyChanged {
		public event PropertyChangedEventHandler PropertyChanged;
		/// <summary>
		/// Retrieve the windows title, also called Text
		/// </summary>
		/// <param name="hWnd">IntPtr for the window</param>
		/// <returns>string</returns>
		private static string getText(IntPtr hWnd) {
			StringBuilder title = new StringBuilder(260, 260);
			User32.GetWindowText(hWnd, title, title.Capacity);
			return title.ToString();
		}
		/// <summary>
		/// Retrieve the windows classname
		/// </summary>
		/// <param name="hWnd">IntPtr for the window</param>
		/// <returns>string</returns>
		private static string getClassname(IntPtr hWnd) {
			StringBuilder classNameBuilder = new StringBuilder(260, 260);
			User32.GetClassName(hWnd, classNameBuilder, classNameBuilder.Capacity);
			return classNameBuilder.ToString();
		}

		/// <summary>
		/// Get the icon for a hWnd
		/// </summary>
		/// <param name="hWnd"></param>
		/// <returns></returns>
		private static ImageSource GetIcon(IntPtr hWnd) {
			IntPtr ICON_SMALL = IntPtr.Zero;
			IntPtr ICON_BIG = new IntPtr(1);
			IntPtr ICON_SMALL2 = new IntPtr(2);

			IntPtr iconHandle = User32.SendMessage(hWnd, (int)WindowsMessages.WM_GETICON, ICON_SMALL2, IntPtr.Zero);
			if (iconHandle == IntPtr.Zero) {
				iconHandle = User32.SendMessage(hWnd, (int)WindowsMessages.WM_GETICON, ICON_SMALL, IntPtr.Zero);
			}
			if (iconHandle == IntPtr.Zero) {
				iconHandle = User32.GetClassLongWrapper(hWnd, (int)ClassLongIndex.GCL_HICONSM);
			}
			if (iconHandle == IntPtr.Zero) {
				iconHandle = User32.SendMessage(hWnd, (int)WindowsMessages.WM_GETICON, ICON_BIG, IntPtr.Zero);
			}
			if (iconHandle == IntPtr.Zero) {
				iconHandle = User32.GetClassLongWrapper(hWnd, (int)ClassLongIndex.GCL_HICON);
			}

			if (iconHandle == IntPtr.Zero) {
				return null;
			}

			using (System.Drawing.Icon icon = System.Drawing.Icon.FromHandle(iconHandle)) {
				return icon.ToImageSource();
			}
		}

		/// <summary>
		/// A helper method to create the WindowInfo for a hWnd
		/// </summary>
		/// <param name="hWnd">IntPtr</param>
		/// <returns>WindowInfo</returns>
		public static WindowInfo CreateFor(IntPtr hWnd, IntPtr parent = default(IntPtr)) {
			return new WindowInfo {
				Handle = hWnd,
				Classname = getClassname(hWnd),
				Text = getText(hWnd),
				Parent = parent
			};
		}

		/// <summary>
		/// The hWnd
		/// </summary>
		public IntPtr Handle {
			get;
			set;
		}

		/// <summary>
		/// The parent hWnd
		/// </summary>
		public IntPtr Parent {
			get;
			set;
		}

		public bool HasParent {
			get {
				return Parent == IntPtr.Zero;
			}
		}

		private List<WindowInfo> children = new List<WindowInfo>();
		public List<WindowInfo> Children {
			get {
				return children;
			}
		}


		private string text = null;
		/// <summary>
		/// Property for the "title" of the window, use "Property = null" to update
		/// </summary>
		public string Text {
			get {
				return this.text;
			}
			set {
				if (value != null) {
					text = value;
				} else {
					text = getText(Handle);
				}
				this.PropertyChanged.Notify(() => this.Text);
			}
		}

		private string classname = null;
		public string Classname {
			get {
				return this.classname;
			}
			set {
				classname = value;
				this.PropertyChanged.Notify(() => this.Classname);
			}
		}

		private Rectangle bounds;
		public Rectangle Bounds {
			get {
				return this.bounds;
			}
			set {
				if (value != Rectangle.Empty) {
					bounds = value;
				} else {
					Rectangle windowRect = Rectangle.Empty;
					if (!HasParent && DWM.isDWMEnabled()) {
						DWM.GetExtendedFrameBounds(Handle, out windowRect);
					}

					if (windowRect.IsEmpty) {
						User32.GetWindowRect(Handle, out windowRect);
					}

					// Correction for maximized windows, only if it's not an app
					if (!HasParent && !isApp && Maximised) {
						Size size = Size.Empty;
						User32.GetBorderSize(Handle, out size);
						windowRect = new Rectangle(windowRect.X + size.Width, windowRect.Y + size.Height, windowRect.Width - (2 * size.Width), windowRect.Height - (2 * size.Height));
					}
					bounds = windowRect;
				}
				this.PropertyChanged.Notify(() => this.Bounds);
			}
		}

		public bool Maximised {
			get {
				if (isApp) {
					if (Visible) {
						foreach (Screen screen in Screen.AllScreens) {
							if (bounds.Equals(screen.Bounds)) {
								return true;
							}
						}
					}
					return false;
				}
				return User32.IsZoomed(Handle);
			}
		}

		/// <summary>
		/// Gets whether the window is visible.
		/// </summary>
		public bool Visible {
			get {
				if (isApp) {
					return ModernUI.AppVisible(Bounds);
				}
				if (isGutter) {
					// gutter is only made available when it's visible
					return true;
				}
				if (isAppLauncher) {
					return ModernUI.IsLauncherVisible;
				}
				return User32.IsWindowVisible(Handle);
			}
		}

		private ImageSource displayIcon;
		public ImageSource DisplayIcon {
			get {
				if (displayIcon == null) {
					displayIcon = GetIcon(Handle);
				}
				return this.displayIcon;
			}
			set {
				displayIcon = value;
			}
		}

		public bool isApp {
			get {
				return ModernUI.MODERNUI_WINDOWS_CLASS.Equals(classname);
			}
		}


		public bool isGutter {
			get {
				return ModernUI.MODERNUI_GUTTER_CLASS.Equals(classname);
			}
		}

		public bool isAppLauncher {
			get {
				return ModernUI.MODERNUI_APPLAUNCHER_CLASS.Equals(classname);
			}
		}

		/// <summary>
		/// Check if this window is the window of a metro app
		/// </summary>
		public bool isMetroApp {
			get {
				return isAppLauncher || isApp;
			}
		}
	}
}
