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
using GreenshotPlugin.WPF;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace GreenshotPlugin.Core.Settings {
	/// <summary>
	/// The SettingsCheckBox allows you to bind a CheckBox to a boolean value in the Configuration.
	/// </summary>
	public class SettingsCheckBox : CheckBox {
		public static readonly DependencyProperty ConfigPropertyProperty = DependencyProperty.Register("ConfigProperty", typeof(string), typeof(SettingsCheckBox));
		public static readonly DependencyProperty LanguageKeyProperty = DependencyProperty.Register("LanguageKey", typeof(string), typeof(SettingsCheckBox));
		public static readonly DependencyProperty ConfigPathProperty = DependencyProperty.Register("ConfigPath", typeof(string), typeof(SettingsCheckBox));
		public static readonly DependencyProperty DependsOnIsCheckedProperty = DependencyProperty.Register("DependsOnIsChecked", typeof(string), typeof(SettingsCheckBox));

		/// <summary>
		/// The config-property to bind to
		/// </summary>
		public string ConfigProperty {
			get {
				return (string)GetValue(ConfigPropertyProperty);
			}
			set {
				SetValue(ConfigPropertyProperty, value);
			}
		}

		/// <summary>
		/// The language-key for the translation
		/// </summary>
		public string LanguageKey {
			get {
				return (string)GetValue(LanguageKeyProperty);
			}
			set {
				SetValue(LanguageKeyProperty, value);
			}
		}

		/// <summary>
		/// The path to the configuration object
		/// </summary>
		public string ConfigPath {
			get {
				return (string)GetValue(ConfigPathProperty);
			}
			set {
				SetValue(ConfigPathProperty, value);
			}
		}

		/// <summary>
		/// If the element which is named here has it's IsChecked checked, this element is enabled.
		/// </summary>
		public string DependsOnIsChecked {
			get {
				return (string)GetValue(DependsOnIsCheckedProperty);
			}
			set {
				SetValue(DependsOnIsCheckedProperty, value);
			}
		}

		public SettingsCheckBox() : base() {
			SetValue(ConfigPathProperty, "CoreConfig");
		}

		protected override void OnInitialized(System.EventArgs e) {
			base.OnInitialized(e);

			this.ApplySettingsStyle();

			if (this.IsDesignMode()) {
				Content = LanguageKey;
				return;
			}

			this.Translate(LanguageKey);

			if (ConfigProperty != null) {
				this.SetBindingIfNull(IsCheckedProperty, string.Format("{0}.{1}", ConfigPath, ConfigProperty));
				this.ApplyFixedBinding(ConfigPath, ConfigProperty, DependsOnIsChecked);
				this.ApplyExpertSettingsBinding(ConfigPath, ConfigProperty);
			}
		}
	}
}
