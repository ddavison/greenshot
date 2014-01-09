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
using GreenshotPlugin.Core;
using System.Windows.Controls;
using Greenshot.IniFile;
using System.Windows;
using System;
using System.IO;
using System.Windows.Markup;
using System.Windows.Resources;
using System.Reflection;

namespace GreenshotPlugin.Core.Settings {
	/// <summary>
	/// Base class for all the settings pages
	/// </summary>
	public class SettingsPage : Page {
		protected IniProxy proxy;
		/// <summary>
		/// Binding on anything but the CoreConfiguration can be done on the Config property
		/// which should be set with the specify configuration in the Initialize
		/// </summary>
		public dynamic Config {
			get {
				return proxy;
			}
		}

		/// <summary>
		/// Making sure the CoreConfiguration is also always available
		/// </summary>
		public dynamic CoreConfig {
			get;
			set;
		}

		/// <summary>
		/// Override to initialize your page!
		/// </summary>
		protected virtual void Initialize() {
		}

		protected void OnLoaded(object sender, RoutedEventArgs e) {
			Initialize();
			if (proxy != null) {
				proxy.StartTransaction();
			}
			this.DataContext = this;
		}

		public SettingsPage() : base() {
			this.Loaded += OnLoaded;
		}

		/// <summary>
		/// The Cancel method rolls-back the changes to the proxy
		/// </summary>
		public virtual void Rollback() {
			if (proxy != null) {
				proxy.Rollback();
			}
		}

		/// <summary>
		/// The Cancel method rolls-back the changes to the proxy
		/// </summary>
		public virtual void Commit() {
			// Apply edit changes
			if (proxy != null) {
				proxy.Commit();
			}
		}
	}
}
