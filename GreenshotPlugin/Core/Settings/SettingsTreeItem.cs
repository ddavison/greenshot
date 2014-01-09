using GreenshotPlugin.Core;
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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace GreenshotPlugin.Core.Settings {
	public class SettingsTreeItem : INotifyPropertyChanged {
		private IDictionary<string, SettingsTreeItem> children = new SortedDictionary<string, SettingsTreeItem>();
		public event PropertyChangedEventHandler PropertyChanged;
		private bool visible = true;
		private bool marked = false;
		private bool expanded = false;

		public void ResetUIProperties(bool fullReset) {
			Visible = true;
			Marked = false;
			if (fullReset) {
				Expanded = false;
			}
		}

		public bool Visible {
			get {
				return visible;
			}
			set {
				visible = value;
				if (PropertyChanged != null) {
					PropertyChanged(this, new PropertyChangedEventArgs("Visible"));
				}
			}
		}

		public bool Marked {
			get {
				return marked;
			}
			set {
				marked = value;
				if (PropertyChanged != null) {
					PropertyChanged(this, new PropertyChangedEventArgs("Marked"));
				}
			}
		}

		public bool Expanded {
			get {
				return expanded;
			}
			set {
				expanded = value;
				if (PropertyChanged != null) {
					PropertyChanged(this, new PropertyChangedEventArgs("Expanded"));
				}
			}
		}

		public String Key {
			get;
			set;
		}

		public String Text {
			get {
				return Language.GetString(Key);
			}
		}

		public Type SettingsPageType {
			get;
			set;
		}

		public SettingsPage SettingsPage {
			get;
			set;
		}

		public string Url {
			get;
			set;
		}

		public IDictionary<string, SettingsTreeItem> Children {
			get {
				return children;
			}
		}

		/// <summary>
		/// Make sure the Children are ordered how we like them
		/// </summary
		public ObservableCollection<SettingsTreeItem> ChildrenSortedByText {
			get {
				return new ObservableCollection<SettingsTreeItem>(children.Values.OrderBy(i => i.Text));
			}
		}
	}
}
