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
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

using Greenshot.Plugin;
using System.Windows.Input;
using System.Windows.Forms;
using System.Windows.Data;

namespace GreenshotPlugin.Core.Settings {
	/// <summary>
	/// A simple control that allows the user to select pretty much any valid hotkey combination
	/// See: http://www.codeproject.com/KB/buttons/hotkeycontrol.aspx
	/// But is modified to work with WPF and in Greenshot, also added localized support
	/// </summary>
	public class SettingsHotkeyTextBox : SettingsTextBox {
		private static log4net.ILog LOG = log4net.LogManager.GetLogger(typeof(SettingsHotkeyTextBox));
		[DllImport("user32.dll", EntryPoint = "GetKeyNameTextA", SetLastError = true)]
		private static extern int GetKeyNameText(uint lParam, [Out] StringBuilder lpString, int nSize);
		[DllImport("user32.dll", SetLastError = true)]
		private static extern uint MapVirtualKey(uint uCode, uint uMapType);
		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint virtualKeyCode);
		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

		// Holds the list of hotkeys
		private static Dictionary<int, HotKeyHandler> keyHandlers = new Dictionary<int, HotKeyHandler>();
		private static int hotKeyCounter = 1;
		private const uint WM_HOTKEY = 0x312;
		private static IntPtr hotkeyHWND = IntPtr.Zero;

		private enum MapType : uint {
			MAPVK_VK_TO_VSC = 0, //The uCode parameter is a virtual-key code and is translated into a scan code. If it is a virtual-key code that does not distinguish between left- and right-hand keys, the left-hand scan code is returned. If there is no translation, the function returns 0.
			MAPVK_VSC_TO_VK = 1, //The uCode parameter is a scan code and is translated into a virtual-key code that does not distinguish between left- and right-hand keys. If there is no translation, the function returns 0.
			MAPVK_VK_TO_CHAR = 2,	  //The uCode parameter is a virtual-key code and is translated into an unshifted character value in the low order word of the return value. Dead keys (diacritics) are indicated by setting the top bit of the return value. If there is no translation, the function returns 0.
			MAPVK_VSC_TO_VK_EX = 3,	//The uCode parameter is a scan code and is translated into a virtual-key code that distinguishes between left- and right-hand keys. If there is no translation, the function returns 0.
			MAPVK_VK_TO_VSC_EX = 4 //The uCode parameter is a virtual-key code and is translated into a scan code. If it is a virtual-key code that does not distinguish between left- and right-hand keys, the left-hand scan code is returned. If the scan code is an extended scan code, the high byte of the uCode value can contain either 0xe0 or 0xe1 to specify the extended scan code. If there is no translation, the function returns 0.
		}

		/// <summary>
		/// Key for the Hotkey
		/// </summary>
		public Key Hotkey {
			get;
			set;
		}

		/// <summary>
		/// ModifierKeys for the hotkey
		/// </summary>
		public ModifierKeys Modifiers {
			get;
			set;
		}

		/// <summary>
		/// Creates a new HotkeyControl
		/// </summary>
		public SettingsHotkeyTextBox() : base() {
			// Handle events that occurs when keys are pressed
			this.PreviewKeyDown += ShortcutTextBox_PreviewKeyDown;
			this.PreviewKeyUp += ShortcutTextBox_PreviewKeyDown;
			this.Loaded += SettingsHotkeyTextBox_Loaded;
		}

		void SettingsHotkeyTextBox_Loaded(object sender, System.Windows.RoutedEventArgs e) {
			SettingsPage settingsPage = (SettingsPage)this.DataContext;
			if (settingsPage != null) {
				string shortcutConfig = settingsPage.CoreConfig[ConfigProperty].Value;
				Modifiers = HotkeyModifiersFromString(shortcutConfig);
				Hotkey = HotkeyFromString(shortcutConfig);
				Text = GetLocalizedHotkeyString(Modifiers, Hotkey);
			}
		}

		protected override void OnInitialized(System.EventArgs e) {
			base.OnInitialized(e);
			// Clear binding from the SettingsTextBox
			BindingOperations.ClearBinding(this, TextProperty);
		}

		private void ShortcutTextBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e) {
			// The text box grabs all input.
			e.Handled = true;
			// Fetch the actual shortcut key.
			Key key = (e.Key == Key.System ? e.SystemKey : e.Key);

			// Ignore modifier keys.
			if (key == Key.LeftShift || key == Key.RightShift
				|| key == Key.LeftCtrl || key == Key.RightCtrl
				|| key == Key.LeftAlt || key == Key.RightAlt
				|| key == Key.LWin || key == Key.RWin) {
				return;
			}

			Hotkey = key;
			Modifiers = Keyboard.Modifiers;
			SettingsPage settingsPage = (SettingsPage)this.DataContext;
			settingsPage.CoreConfig[ConfigProperty].Value = GetHotkeyString(Modifiers, Hotkey);
			// Update the text box.
			Text = GetLocalizedHotkeyString(Modifiers, Hotkey);
		}

		/// <summary>
		/// Helper method to convert the hotkey string directly to a localized string
		/// </summary>
		/// <param name="hotkeyString">config hotkey string</param>
		/// <returns>Localized hotkey string</returns>
		public static string GetLocalizedHotkeyStringFromString(string hotkeyString) {
			Key key = HotkeyFromString(hotkeyString);
			ModifierKeys modifiers = HotkeyModifiersFromString(hotkeyString);
			return GetLocalizedHotkeyString(modifiers, key);
		}

		/// <summary>
		/// Parse the string, and get the Key
		/// </summary>
		/// <param name="hotkeyString"></param>
		/// <returns>Key</returns>
		public static Key HotkeyFromString(string hotkeyString) {
			Key key = Key.None;
			try {
				if (!string.IsNullOrEmpty(hotkeyString)) {
					if (hotkeyString.LastIndexOf('+') > 0) {
						hotkeyString = hotkeyString.Remove(0, hotkeyString.LastIndexOf('+') + 1).Trim();
					}
					key = (Key)Key.Parse(typeof(Key), hotkeyString);
				}
			} catch {
			}
			return key;
		}

		/// <summary>
		/// Parse the string, and get the ModifierKeys
		/// </summary>
		/// <param name="modifiersString"></param>
		/// <returns>ModifierKeys</returns>
		public static ModifierKeys HotkeyModifiersFromString(string modifiersString) {
			ModifierKeys modifiers = ModifierKeys.None;
			if (!string.IsNullOrEmpty(modifiersString)) {
				if (modifiersString.ToLower().Contains("alt")) {
					modifiers |= ModifierKeys.Alt;
				}
				if (modifiersString.ToLower().Contains("ctrl")) {
					modifiers |= ModifierKeys.Control;
				}
				if (modifiersString.ToLower().Contains("shift")) {
					modifiers |= ModifierKeys.Shift;
				}
				if (modifiersString.ToLower().Contains("win")) {
					modifiers |= ModifierKeys.Windows;
				}
			}
			return modifiers;
		}

		/// <summary>
		/// Get the Localized String for the supplied modifiers & hotkey
		/// </summary>
		/// <param name="modifiers"></param>
		/// <param name="hotkey"></param>
		/// <returns></returns>
		public static string GetLocalizedHotkeyString(ModifierKeys modifiers, Key hotkey) {
			// Build the shortcut key name.
			StringBuilder shortcutText = new StringBuilder();

			if ((modifiers & ModifierKeys.Control) != 0) {
				shortcutText.Append(GetKeyName(Key.LeftCtrl) + "+");
			}
			if ((modifiers & ModifierKeys.Shift) != 0) {
				shortcutText.Append(GetKeyName(Key.LeftShift) + "+");
			}
			if ((modifiers & ModifierKeys.Alt) != 0) {
				shortcutText.Append(GetKeyName(Key.LeftAlt) + "+");
			}
			// Fix snapshot
			if (hotkey == Key.Snapshot) {
				shortcutText.Append(GetKeyName(Key.PrintScreen));
			} else {
				shortcutText.Append(GetKeyName(hotkey));
			}
			return shortcutText.ToString();
		}

		/// <summary>
		/// Get the english (storage) String for the supplied modifiers & hotkey
		/// </summary>
		/// <param name="modifiers"></param>
		/// <param name="hotkey"></param>
		/// <returns></returns>
		public static string GetHotkeyString(ModifierKeys modifiers, Key hotkey) {
			StringBuilder shortcutConfig = new StringBuilder();
			// Build the shortcut key name.
			if ((modifiers & ModifierKeys.Control) != 0) {
				shortcutConfig.Append("Ctrl + ");
			}
			if ((modifiers & ModifierKeys.Shift) != 0) {
				shortcutConfig.Append("Shift + ");
			}
			if ((modifiers & ModifierKeys.Alt) != 0) {
				shortcutConfig.Append("Alt + ");
			}
			// Fix snapshot
			if (hotkey == Key.Snapshot) {
				shortcutConfig.Append("PrintScreen");
			} else {
				shortcutConfig.Append(hotkey.ToString());
			}
			return shortcutConfig.ToString();
		}

		/// <summary>
		/// Get the localized keyname
		/// </summary>
		/// <param name="givenKey"></param>
		/// <returns></returns>
		private static string GetKeyName(Key givenKey) {
			StringBuilder keyName = new StringBuilder();
			const uint NUMPAD = 55;

			Key virtualKey = givenKey;
			string keyString = "";
			// Make VC's to real keys
			switch (virtualKey) {
				case Key.Multiply:
					GetKeyNameText(NUMPAD << 16, keyName, 100);
					keyString = keyName.ToString().Replace("*", "").Trim().ToLower();
					if (keyString.IndexOf("(") >= 0) {
						return "* " + keyString;
					}
					keyString = keyString.Substring(0, 1).ToUpper() + keyString.Substring(1).ToLower();
					return keyString + " *";
				case Key.Divide:
					GetKeyNameText(NUMPAD << 16, keyName, 100);
					keyString = keyName.ToString().Replace("*", "").Trim().ToLower();
					if (keyString.IndexOf("(") >= 0) {
						return "/ " + keyString;
					}
					keyString = keyString.Substring(0, 1).ToUpper() + keyString.Substring(1).ToLower();
					return keyString + " /";
			}
			uint scanCode = MapVirtualKey((uint)KeyInterop.VirtualKeyFromKey(givenKey), (uint)MapType.MAPVK_VK_TO_VSC);

			// because MapVirtualKey strips the extended bit for some keys
			switch (virtualKey) {
				case Key.Left:
				case Key.Up:
				case Key.Right:
				case Key.Down: // arrow keys
				case Key.Prior:
				case Key.Next: // page up and page down
				case Key.End:
				case Key.Home:
				case Key.Insert:
				case Key.Delete:
				case Key.NumLock:
					LOG.Debug("Modifying Extended bit");
					scanCode |= 0x100; // set extended bit
					break;
				case Key.PrintScreen: // PrintScreen
					scanCode = 311;
					break;
				case Key.Pause: // PrintScreen
					scanCode = 69;
					break;
			}
			scanCode |= 0x200;
			if (GetKeyNameText(scanCode << 16, keyName, 100) != 0) {
				string visibleName = keyName.ToString();
				if (visibleName.Length > 1) {
					visibleName = visibleName.Substring(0, 1) + visibleName.Substring(1).ToLower();
				}
				return visibleName;
			} else {
				return givenKey.ToString();
			}
		}

		/// <summary>
		/// Register the HWND for the hotkeys
		/// </summary>
		/// <param name="hWnd"></param>
		public static void RegisterHotkeyHWND(IntPtr hWnd) {
			hotkeyHWND = hWnd;
		}


		/// <summary>
		/// Handle WndProc messages for the hotkey
		/// </summary>
		/// <param name="m"></param>
		/// <returns>true if the message was handled</returns>
		public static bool HandleMessages(ref Message m) {
			if (m.Msg == WM_HOTKEY) {
				// Call handler
				keyHandlers[(int)m.WParam]();
				return true;
			}
			return false;
		}

		/// <summary>
		/// Register a hotkey
		/// </summary>
		/// <param name="hWnd">The window which will get the event</param>
		/// <param name="modifierKeyCode">The modifier, e.g.: Modifiers.CTRL, Modifiers.NONE or Modifiers.ALT</param>
		/// <param name="virtualKeyCode">The virtual key code</param>
		/// <param name="handler">A HotKeyHandler, this will be called to handle the hotkey press</param>
		/// <returns>the hotkey number, -1 if failed</returns>
		public static int RegisterHotKey(string hotkeyString, HotKeyHandler handler) {
			if (hotkeyHWND == IntPtr.Zero) {
				LOG.Warn("hotkeyHWND not registered!");
				return -1;
			}
			ModifierKeys modifiers = HotkeyModifiersFromString(hotkeyString);
			Key key = HotkeyFromString(hotkeyString);
			if (key == Key.None) {
				LOG.Warn("Trying to register a Keys.none hotkey, ignoring");
				return 0;
			}

			if (RegisterHotKey(hotkeyHWND, hotKeyCounter, (uint)modifiers, (uint)KeyInterop.VirtualKeyFromKey(key))) {
				keyHandlers.Add(hotKeyCounter, handler);
				return hotKeyCounter++;
			} else {
				LOG.Warn(String.Format("Couldn't register hotkey modifier {0} key {1}", modifiers, key));
				return -1;
			}
		}

		public static void UnregisterHotkeys() {
			foreach (int hotkey in keyHandlers.Keys) {
				UnregisterHotKey(hotkeyHWND, hotkey);
			}
			// Remove all key handlers
			keyHandlers.Clear();
		}

		public static void UnregisterHotkey(int hotkey) {
			bool removeHotkey = false;
			foreach (int availableHotkey in keyHandlers.Keys) {
				if (availableHotkey == hotkey) {
					UnregisterHotKey(hotkeyHWND, hotkey);
					removeHotkey = true;
				}
			}
			if (removeHotkey) {
				// Remove key handler
				keyHandlers.Remove(hotkey);
			}
		}
	}
}