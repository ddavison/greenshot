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
using System.Collections.ObjectModel;
using System.Globalization;
using System.Reflection;
using System.Windows.Data;

using GreenshotPlugin.Core;

namespace GreenshotPlugin.WPF {

	/// <summary>
	/// This class makes it possible to display a combo-box with the items from an enum
	/// Example:
	/// <g:EnumDisplayer Type="{x:Type gp:ScreenCaptureMode}" x:Key="screenCaptureModes"/>
	/// 
	/// <ComboBox ItemsSource="{Binding Source={StaticResource screenCaptureModes},Path=DisplayValues}" SelectedValue="{Binding [BindingVar]}" SelectedValuePath="Key" DisplayMemberPath="Value"/>
	/// </summary>
	public class EnumDisplayer {
		private Type type;
		private IDictionary<object, string> displayValues = new Dictionary<object, string>();
		
		public EnumDisplayer() {
		}
	
		public Type Type {
			get {
				return type;
			}
			set {
				if (!value.IsEnum) {
					throw new ArgumentException("parameter is not an Enumerated type", "value");
				}
				try {
					this.type = value;
					displayValues.Clear();
					string typename = this.type.Name;
					var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static);
					foreach (var field in fields) {
						object enumValue = field.GetValue(null);

						string displayKey = string.Format("{0}.{1}", typename, enumValue.ToString());

						string displayString = null;
						if (displayKey != null && Language.hasKey(displayKey)) {
							displayString = Language.GetString(displayKey);
						} else {
							displayString = enumValue.ToString();
						}

						if (displayString != null) {
							displayValues.Add(enumValue, displayString);
						}
					}
				} catch {
				}
			}
		}
		
		/// <summary>
		/// Bind to this for the values
		/// </summary>
		public IDictionary<object, string> DisplayValues {
			get {
				return displayValues;
			}
		}
	}
}
