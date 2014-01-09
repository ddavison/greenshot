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
using Greenshot.IniFile;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GreenshotPlugin.Core.Settings {
	/// <summary>
	/// The SettingsWindow is the window which contains the treeview with all the settings pages and show the currently selected settings page.
	/// </summary>
	public partial class SettingsWindow : Window {
		private static CoreConfiguration coreConfiguration = IniConfig.GetIniSection<CoreConfiguration>();
		private static IDictionary<string, SettingsTreeItem> treeItems = new SortedDictionary<string, SettingsTreeItem>();
		private string initialSelectedPath = null;

		#region Code to register the settings pages
		/// <summary>
		/// Register the SettingsPage Type T to the supplied the keyPath
		/// </summary>
		/// <typeparam name="T">Type which extends SettingsPage</typeparam>
		/// <param name="keyPath">key,key,key</param>
		public static void RegisterSettingsPage<T>(string keyPath) where T : SettingsPage {
			RegisterSettingsPage(keyPath, typeof(T), null);
		}

		/// <summary>
		/// Register the web-page to the supplied the keyPath
		/// </summary>
		/// <param name="keyPath">key,key,key</param>
		/// <param name="url">URL for the path</param>
		public static void RegisterSettingsPage(string keyPath, string url) {
			RegisterSettingsPage(keyPath, null, url);
		}

		private static SettingsTreeItem FindSettingsTreeItem(string keyPath) {
			IDictionary<string, SettingsTreeItem> current = treeItems;
			string[] pathEntries = keyPath.Split(new char[] { ',' });
			string currentLocation = null;
			for (int i = 0; i < pathEntries.Length; i++) {
				currentLocation = pathEntries[i];
				if (current.ContainsKey(currentLocation)) {
					if (i == pathEntries.Length - 1) {
						return current[currentLocation];
					}
					current = current[currentLocation].Children;
				}
			}
			return null;
		}

		/// <summary>
		/// Register the SettingsPage Type T or URL to the supplied the keyPath
		/// </summary>
		/// <param name="settingsPageType">Type which extends SettingsPage</param>
		/// <param name="keyPath">key,key,key</param>
		/// <param name="url">URL for the path</param>
		public static void RegisterSettingsPage(string keyPath, Type settingsPageType, string url) {
			IDictionary<string, SettingsTreeItem> current = treeItems;
			string[] pathEntries = keyPath.Split(new char[] { ',' });
			string currentLocation = null;
			for (int i = 0; i < pathEntries.Length; i++ ) {
				currentLocation = pathEntries[i];
				if (current.ContainsKey(currentLocation)) {
					if (i == pathEntries.Length - 1) {
						current[currentLocation].SettingsPageType = settingsPageType;
						current[currentLocation].Url = url;
					}
					current = current[currentLocation].Children;
				} else if (i < pathEntries.Length - 1) {
					SettingsTreeItem item = new SettingsTreeItem {
						Key = currentLocation
					};
					current.Add(currentLocation, item);
					current = item.Children;
				} else {
					current.Add(currentLocation, new SettingsTreeItem {
						Key = currentLocation,
						SettingsPageType = settingsPageType,
						Url = url
					});
				}
			}
		}

		#endregion

		#region Public Properties
		/// <summary>
		/// Make sure the elements in the SettingsTree are ordered how we like them
		/// </summary>
		public ObservableCollection<SettingsTreeItem> SettingsTree {
			get {
				return new ObservableCollection<SettingsTreeItem>(treeItems.Values.OrderBy(i => i.Text));
			}
		}
		protected IniProxy coreProxy;
		/// <summary>
		/// Binding can be done on the Config
		/// </summary>
		public dynamic CoreConfig {
			get {
				return coreProxy;
			}
		}

		#endregion
		/// <summary>
		/// Constructor
		/// </summary>
		public SettingsWindow(string initialSelectedPath) {
			this.initialSelectedPath = initialSelectedPath;
			coreProxy = new IniProxy(coreConfiguration);
			ResetUI(treeItems, true);
			InitializeComponent();
			this.DataContext = this;
		}

		#region Event handlers
		/// <summary>
		/// The user wants to apply the changes
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OK_Click(object sender, RoutedEventArgs e) {
			coreProxy.Commit();
			Apply(treeItems, true);
			IniConfig.Save();
			this.Close();
		}

		/// <summary>
		/// The user doesn't want to apply the changes
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Cancel_Click(object sender, RoutedEventArgs e) {
			coreProxy.Rollback();
			Apply(treeItems, false);
			this.Close();
		}

		private void Clear_Click(object sender, RoutedEventArgs e) {
			FilterBox.Text = null;
			ApplyFilter();
		}

		/// <summary>
		/// An element in the Settings Tree is selected, now do:
		/// 1) if a SettingsPage is registered to the item: 
		/// a) Create the page if it wasn't created
		/// b) Show the page
		/// 2) if a URL is registered to the item, navigate to it
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
			SettingsTreeItem settingsTreeItem = (SettingsTreeItem)e.NewValue;
			if (settingsTreeItem != null) {
				if (settingsTreeItem.SettingsPage != null) {
					CurrentSettingsPage.Content = settingsTreeItem.SettingsPage;
				} else if (settingsTreeItem.SettingsPageType != null) {
					settingsTreeItem.SettingsPage = InstanciatePage(settingsTreeItem.SettingsPageType);
					CurrentSettingsPage.Content = settingsTreeItem.SettingsPage;
				} else if (settingsTreeItem.Url != null) {
					CurrentSettingsPage.Navigate(new Uri(settingsTreeItem.Url));
				}
			}
		}

		private SettingsPage InstanciatePage(Type settingsPageType) {
			SettingsPage newSettingsPage = (SettingsPage)Activator.CreateInstance(settingsPageType);
			// Assign CoreConfiguration "IniProxy"
			newSettingsPage.CoreConfig = CoreConfig;
			return newSettingsPage;
		}

		/// <summary>
		/// Make it possible that the Frame can use the default proxy & credentials, so we can make it go to any link
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void CurrentSettingsPage_Navigating(object sender, System.Windows.Navigation.NavigatingCancelEventArgs e) {
			if (e.Uri != null && coreConfiguration.UseProxy) {
				e.WebRequest.Proxy = GreenshotPlugin.Core.NetworkHelper.CreateProxy(e.Uri);
			}
		}

		/// <summary>
		/// Key up on the Textbox with the filter, make all elements which don't match invisible
		/// Make the elements that match highlighted
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Filter_KeyUp(object sender, System.Windows.Input.KeyEventArgs e) {
			ApplyFilter();
		}

		#endregion

		#region Helper methods

		/// <summary>
		/// Apply the current filter to the tree items
		/// </summary>
		private void ApplyFilter() {
			string filterText = FilterBox.Text;
			if (!string.IsNullOrEmpty(filterText) && filterText.Length >= 2) {
				filterText = filterText.ToLower();
				Filter(treeItems, filterText);
			} else {
				ResetUI(treeItems, false);
			}
		}

		/// <summary>
		/// This method is called when the OK or Cancel buttons are pressed to apply settings and remove the page.
		/// </summary>
		/// <param name="treeItemsToApply"></param>
		/// <param name="apply"></param>
		private static void Apply(IDictionary<string, SettingsTreeItem> treeItemsToApply, bool apply) {
			if (treeItemsToApply == null) {
				return;
			}
			foreach (SettingsTreeItem settingsTreeItem in treeItemsToApply.Values) {
				if (settingsTreeItem.SettingsPage != null) {
					if (apply) {
						settingsTreeItem.SettingsPage.Commit();
					} else {
						settingsTreeItem.SettingsPage.Rollback();
					}
					settingsTreeItem.SettingsPage = null;
				}
				Apply(settingsTreeItem.Children, apply);
			}
		}

		/// <summary>
		/// Reset the elements in the UI
		/// </summary>
		/// <param name="treeItemsToApply"></param>
		private static void ResetUI(IDictionary<string, SettingsTreeItem> treeItemsToApply, bool fullReset) {
			if (treeItemsToApply == null) {
				return;
			}
			foreach (SettingsTreeItem settingsTreeItem in treeItemsToApply.Values) {
				settingsTreeItem.ResetUIProperties(fullReset);
				ResetUI(settingsTreeItem.Children, fullReset);
			}
		}

		/// <summary>
		/// Apply the filter to all the SettingsTreeItems
		/// </summary>
		/// <param name="treeItemsToApply"></param>
		/// <param name="filter"></param>
		/// <returns></returns>
		private bool Filter(IDictionary<string, SettingsTreeItem> treeItemsToApply, string filter) {
			if (treeItemsToApply == null) {
				return false;
			}
			bool foundElements = false;
			foreach (SettingsTreeItem settingsTreeItem in treeItemsToApply.Values) {
				bool foundChildren = Filter(settingsTreeItem.Children, filter);
				if (foundChildren) {
					foundElements = true;
				}

				// Check if page is instanciated, if not do so now
				if (settingsTreeItem.SettingsPageType != null && settingsTreeItem.SettingsPage == null) {
					settingsTreeItem.SettingsPage = InstanciatePage(settingsTreeItem.SettingsPageType);
				}

				// check tree item name
				if (settingsTreeItem.Text.ToLower().Contains(filter)) {
					settingsTreeItem.Visible = true;
					settingsTreeItem.Expanded = true;
					settingsTreeItem.Marked = !string.IsNullOrEmpty(filter);
					foundElements = true;
					continue;
				} else if (settingsTreeItem.SettingsPage != null) {
					// check page content
					Page settingsTreeItemPage = settingsTreeItem.SettingsPage;
					if (hasText(settingsTreeItemPage, filter)) {
						settingsTreeItem.Visible = true;
						settingsTreeItem.Expanded = true;
						settingsTreeItem.Marked = !string.IsNullOrEmpty(filter);
						foundElements = true;
						continue;
					}
				}

				settingsTreeItem.Marked = false;
				settingsTreeItem.Expanded = foundChildren;
				settingsTreeItem.Visible = foundChildren;
			}
			return foundElements;
		}

		/// <summary>
		/// Walks the tree items to find the node corresponding with
		/// the given item, then sets it to be selected.
		/// </summary>
		/// <param name="treeView">The tree view to set the selected
		/// item on</param>
		/// <param name="item">The item to be selected</param>
		/// <returns><c>true</c> if the item was found and set to be
		/// selected</returns>
		public bool SetSelectedItem(object item) {
			return SetSelected(SettingsTreeView, item);
		}

		static private bool SetSelected(ItemsControl parent, object child) {
			if (parent == null || child == null) {
				return false;
			}

			TreeViewItem childNode = parent.ItemContainerGenerator.ContainerFromItem(child) as TreeViewItem;

			if (childNode != null) {
				childNode.Focus();
				return childNode.IsSelected = true;
			}

			if (parent.Items.Count > 0) {
				foreach (object childItem in parent.Items) {
					ItemsControl childControl = parent.ItemContainerGenerator.ContainerFromItem(childItem) as ItemsControl;

					if (SetSelected(childControl, child)) {
						return true;
					}
				}
			}

			return false;
		}

		private static bool hasText(object current, string text) {
			// The logical tree can contain any type of object, not just 
			// instances of DependencyObject subclasses.  LogicalTreeHelper
			// only works with DependencyObject subclasses, so we must be
			// sure that we do not pass it an object of the wrong type.
			DependencyObject depObj = current as DependencyObject;
			if (depObj != null) {
				foreach (object logicalChild in LogicalTreeHelper.GetChildren(depObj)) {
					if (logicalChild is ContentControl) {
						ContentControl control = logicalChild as ContentControl;
						if (control.Content is string) {
							string content = control.Content as string;
							if (!string.IsNullOrEmpty(content)) {
								if (content.ToLower().Contains(text)) {
									//control.Background = System.Windows.Media.Brushes.Red;
									return true;
								}
							}
						}
					}
					if (hasText(logicalChild, text)) {
						return true;
					}
				}
			}
			return false;
		}
		#endregion

		private void Window_Loaded(object sender, RoutedEventArgs e) {
			if (initialSelectedPath != null) {
				SetSelectedItem(SettingsWindow.FindSettingsTreeItem(initialSelectedPath));
			}
		}

	}
}
