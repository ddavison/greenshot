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
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace GreenshotPlugin.Core.Capturing {
	/// <summary>
	/// The WinEventHook registers itself to all for us important Windows events.
	/// This makes it possible to know a.o. when a window is created, moved, updated and closed.
	/// The information in the WindowInfo objects is than updated accordingly, so when capturing everything is already available.
	/// </summary>
	public class WinEventHook : IDisposable {
		private static readonly log4net.ILog LOG = log4net.LogManager.GetLogger(typeof(WinEventHook));
		private List<IntPtr> winEventHooks = new List<IntPtr>();
		private WinEventDelegate winEventProc;

		// Used for caching so we can find the WindowInfo object quickly without iterating
		private IDictionary<IntPtr, WindowInfo> windowsCache = new ConcurrentDictionary<IntPtr, WindowInfo>();

		// The linked list with all the windows
		private LinkedList<WindowInfo> windows = new LinkedList<WindowInfo>();
		public LinkedList<WindowInfo> Windows {
			get {
				return windows;
			}
		}

		private static WinEventHook instance = new WinEventHook();
		public static WinEventHook Instance {
			get {
				return instance;
			}
		}

		/// <summary>
		/// Register to get the Windows events
		/// </summary>
		private WinEventHook() {
		}

		/// <summary>
		/// Are hooks active?
		/// </summary>
		public bool isHooked {
			get {
				return winEventHooks != null && winEventHooks.Count > 0;
			}
		}

		public void Hook() {
			winEventProc = new WinEventDelegate(WinEventDelegate);
			if (winEventHooks != null && winEventHooks.Count == 0) {
				winEventHooks.Add(User32.SetWinEventHook(WinEvent.EVENT_MIN, WinEvent.EVENT_MAX, IntPtr.Zero, winEventProc, 0, 0, WinEventHookFlags.WINEVENT_SKIPOWNPROCESS));
				//winEventHooks.Add(User32.SetWinEventHook(WinEvent.EVENT_OBJECT_LOCATIONCHANGE, WinEvent.EVENT_OBJECT_LOCATIONCHANGE, IntPtr.Zero, winEventProc, 0, 0, WinEventHookFlags.WINEVENT_SKIPOWNPROCESS));
				//winEventHooks.Add(User32.SetWinEventHook(WinEvent.EVENT_SYSTEM_MOVESIZESTART, WinEvent.EVENT_SYSTEM_MOVESIZEEND, IntPtr.Zero, winEventProc, 0, 0, WinEventHookFlags.WINEVENT_SKIPOWNPROCESS));
				//hooks.Add(SetWinEventHook((uint)WinEvent.EVENT_SYSTEM_MENUSTART, (uint)WinEvent.EVENT_SYSTEM_MENUPOPUPEND, IntPtr.Zero, _winEventProc, 0, 0, (uint)SetWinEventHookFlags.WINEVENT_SKIPOWNPROCESS));
				//winEventHooks.Add(User32.SetWinEventHook(WinEvent.EVENT_OBJECT_FOCUS, WinEvent.EVENT_OBJECT_FOCUS, IntPtr.Zero, winEventProc, 0, 0, WinEventHookFlags.WINEVENT_SKIPOWNPROCESS));
				//winEventHooks.Add(User32.SetWinEventHook(WinEvent.EVENT_OBJECT_NAMECHANGE, WinEvent.EVENT_OBJECT_NAMECHANGE, IntPtr.Zero, winEventProc, 0, 0, WinEventHookFlags.WINEVENT_SKIPOWNPROCESS));
			}
		}

		/// <summary>
		/// Remove all hooks
		/// </summary>
		private void Unhook() {
			if (winEventHooks != null) {
				foreach (IntPtr hook in winEventHooks) {
					if (hook != IntPtr.Zero) {
						User32.UnhookWinEvent(hook);
					}
				}
				winEventHooks = null;
			}
		}

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing) {
			if (disposing) {
				Unhook();
			}
		}

		/// <summary>
		/// WinEventDelegate for the creation & destruction
		/// </summary>
		/// <param name="hWinEventHook"></param>
		/// <param name="eventType"></param>
		/// <param name="hWnd"></param>
		/// <param name="idObject"></param>
		/// <param name="idChild"></param>
		/// <param name="dwEventThread"></param>
		/// <param name="dwmsEventTime"></param>
		private void WinEventDelegate(IntPtr hWinEventHook, WinEvent eventType, IntPtr hWnd, EventObjects idObject, int idChild, uint dwEventThread, uint dwmsEventTime) {
			if (hWnd == IntPtr.Zero || idObject != EventObjects.OBJID_WINDOW) {
				if (idObject != EventObjects.OBJID_CARET && idObject != EventObjects.OBJID_CURSOR) {
					LOG.InfoFormat("Unhandled eventType: {0}, hWnd {1}, idObject {2}, idChild {3}, dwEventThread {4}, dwmsEventTime {5}", eventType, hWnd, idObject, idChild, dwEventThread, dwmsEventTime);
				}
				return;
			}
			WindowInfo windowInfo = null;
			IntPtr hWndParent;
			bool isPreviouslyCreated = windowsCache.TryGetValue(hWnd, out windowInfo);
			if (windowInfo == null) {
				hWndParent = User32.GetParent(hWnd);
				windowInfo = WindowInfo.CreateFor(hWnd, hWndParent);
			} else {
				hWndParent = windowInfo.Parent;
			}
			if (windowInfo.Classname == "OleMainThreadWndClass") {
				// Not a peeps, this window is not interresting.
				return;
			}
			LOG.InfoFormat("eventType {0}, hWnd {1}, idObject {2}, idChild {3}, dwEventThread {4}, dwmsEventTime {5}", eventType, hWnd, idObject, idChild, dwEventThread, dwmsEventTime);
			switch (eventType) {
				case WinEvent.EVENT_OBJECT_NAMECHANGE:
					if (windowsCache.TryGetValue(hWnd, out windowInfo)) {
						// Force update of Text
						windowInfo.Text = null;
						LOG.InfoFormat("Rename {0} / {1}", windowInfo.Text, windowInfo.Classname);
					} else {
						// Huh?
						LOG.WarnFormat("No record of a window with hWnd {0}", hWnd);
					}
					break;
				case WinEvent.EVENT_OBJECT_CREATE:
					if (!isPreviouslyCreated) {
						if (hWndParent != IntPtr.Zero) {
							WindowInfo parent;
							if (!windowsCache.TryGetValue(hWndParent, out parent)) {
								parent = WindowInfo.CreateFor(hWndParent);
								add(parent);
							}
							parent.Children.Add(windowInfo);
							LOG.InfoFormat("Added child {0} / {1} to {2} / {3} / {4}", windowInfo.Text, windowInfo.Classname, parent.Handle, parent.Text, parent.Classname);
						} else {
							add(windowInfo);
							LOG.InfoFormat("Added {0} / {1}", windowInfo.Text, windowInfo.Classname);
						}
					} else {
						LOG.InfoFormat("'Activated' parent {0} / {1}", windowInfo.Text, windowInfo.Classname);
					}
					break;
				case WinEvent.EVENT_OBJECT_DESTROY:
					if (isPreviouslyCreated) {
						LOG.InfoFormat("Removing {0} / {1}", windowInfo.Text, windowInfo.Classname);
						remove(windowInfo);
					} else {
						if (hWndParent != IntPtr.Zero) {
							if (windowsCache.TryGetValue(hWnd, out windowInfo)) {
								LOG.WarnFormat("Unhandled destroy of Child from {0}", windowInfo.Text);
								// Implement child removal
							} else {
								LOG.WarnFormat("No record of a child-window with hWnd {0}", hWnd);
							}
						} else {
							LOG.WarnFormat("No record of a top-window with hWnd {0}", hWnd);
						}
					}
					break;
				case WinEvent.EVENT_OBJECT_FOCUS:
					// Move the top-window with the focus to the foreground
					if (isPreviouslyCreated) {
						moveToFront(windowInfo);
						LOG.InfoFormat("Focus: {0} / {1}", windowInfo.Text, windowInfo.Classname);
					}
					break;
				case WinEvent.EVENT_OBJECT_LOCATIONCHANGE:
				case WinEvent.EVENT_SYSTEM_MOVESIZESTART:
				case WinEvent.EVENT_SYSTEM_MOVESIZEEND:
					// Move the top-window with the focus to the foreground
					if (isPreviouslyCreated) {
						windowInfo.Bounds = System.Drawing.Rectangle.Empty;
						LOG.InfoFormat("Move/resize: {2} - {0} / {1}", windowInfo.Text, windowInfo.Classname, windowInfo.Bounds);
					}
					break;
			}
		}

		private void remove(WindowInfo windowInfo) {
			windows.Remove(windowInfo);
			windowsCache.Remove(windowInfo.Handle);
		}
		private void add(WindowInfo windowInfo) {
			windows.AddFirst(windowInfo);
			windowsCache.Add(windowInfo.Handle, windowInfo);
		}

		private void moveToFront(WindowInfo windowInfo) {
			windows.Remove(windowInfo);
			windows.AddFirst(windowInfo);
		}

	}
}
