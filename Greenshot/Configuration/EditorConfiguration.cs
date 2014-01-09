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

using Greenshot.Drawing.Fields;
using GreenshotPlugin.UnmanagedHelpers;
using Greenshot.IniFile;
using System.Reflection;
using Greenshot.Plugin.Drawing;
using System.ComponentModel;

namespace Greenshot.Configuration {
	/// <summary>
	/// Description of CoreConfiguration.
	/// </summary>
	[IniSection("Editor", Description="Greenshot editor configuration")]
	public class EditorConfiguration : IniSection, IDataErrorInfo {
		List<Color> recentColors = new List<Color>();
		[IniProperty("RecentColors", Separator="|", Description="Last used colors")]
		public List<Color> RecentColors {
			get {
				return recentColors;
			}
			set {
				recentColors = value;
			}
		}

		[IniProperty("LastFieldValue", Separator="|", Description="Field values, make sure the last used settings are re-used")]
		public Dictionary<string, object> LastUsedFieldValues {
			get;
			set;
		}

		[IniProperty("MatchSizeToCapture", Description="Match the editor window size to the capture", DefaultValue="True")]
		public bool MatchSizeToCapture {
			get;
			set;
		}

		[IniProperty("WindowPlacementFlags", Description="Placement flags", DefaultValue="0")]
		public WindowPlacementFlags WindowPlacementFlags {
			get;
			set;
		}

		[IniProperty("WindowShowCommand", Description="Show command", DefaultValue="Normal")]
		public ShowWindowCommand ShowWindowCommand {
			get;
			set;
		}

		[IniProperty("WindowMinPosition", Description="Position of minimized window", DefaultValue="-1,-1")]
		public Point WindowMinPosition {
			get;
			set;
		}

		[IniProperty("WindowMaxPosition", Description="Position of maximized window", DefaultValue="-1,-1")]
		public Point WindowMaxPosition {
			get;
			set;
		}

		[IniProperty("WindowNormalPosition", Description="Position of normal window", DefaultValue="100,100,400,400")]
		public Rectangle WindowNormalPosition {
			get;
			set;
		}

		[IniProperty("ReuseEditor", Description = "Reuse already open editor", DefaultValue = "false")]
		public bool ReuseEditor {
			get;
			set;
		}

		[IniProperty("FreehandSensitivity", Expert=true, Description = "The smaller this number, the less smoothing is used. Decrease for detailed drawing, e.g. when using a pen. Increase for smoother lines. e.g. when you want to draw a smooth line.", DefaultValue = "3")]
		public int FreehandSensitivity {
			get;
			set;
		}

		[IniProperty("SuppressSaveDialogAtClose", Description="Suppressed the 'do you want to save' dialog when closing the editor.", DefaultValue="False")]
		public bool SuppressSaveDialogAtClose {
			get;
			set;
		}

		[IniProperty("AutoCropDifference", Expert = true, Description = "Sets how to compare the colors for the autocrop detection, the higher the more is 'selected'. Possible values are from 0 to 255, where everything above ~150 doesn't make much sense!", DefaultValue = "10")]
		public int AutoCropDifference {
			get;
			set;
		}

		/// <summary>
		/// Create a value for the scoped fieldtype,  with a preferred default
		/// </summary>
		/// <param name="scope">Scope (usually the name of the type) for which to create the field</param>
		/// <param name="fieldType">FieldType of the field to construct</param>
		/// <param name="preferredDefaultValue">preferred default</param>
		/// <returns>a new or cached value for the given fieldType, with the scope of it's value being restricted to the Type scope</returns>
		public object CreateCachedValue(string scope, FieldTypes fieldType, object preferredDefaultValue) {
			if (fieldType == FieldTypes.FLAGS) {
				return preferredDefaultValue;
			}
			string requestedField = scope + "." + fieldType.ToString();
			object fieldValue = preferredDefaultValue;
			
			// Check if the configuration exists
			if (LastUsedFieldValues == null) {
				LastUsedFieldValues = new Dictionary<string, object>();
			}
			
			// Check if settings for the requesting type exist, if not create!
			if (LastUsedFieldValues.ContainsKey(requestedField)) {
				// Check if a value is set (not null)!
				if (LastUsedFieldValues[requestedField] != null) {
					fieldValue = LastUsedFieldValues[requestedField];
				} else {
					// Overwrite null value
					LastUsedFieldValues[requestedField] = fieldValue;
				}
			} else {
				LastUsedFieldValues.Add(requestedField, fieldValue);
			}
			return fieldValue;
		}

		public override void AfterLoad() {
 			base.AfterLoad();
			if (AutoCropDifference < 0) {
				AutoCropDifference = 0;
			}
			if (AutoCropDifference > 255) {
				AutoCropDifference = 255;
			}
		}

		/// <summary>
		/// Update the cached value for a certain fieldtype
		/// </summary>
		/// <param name="scope"></param>
		/// <param name="fieldType"></param>
		/// <param name="newValue"></param>
		public void UpdateCachedValue(string scope, FieldTypes fieldType, object newValue) {
			if (fieldType == FieldTypes.FLAGS) {
				return;
			}
			string requestedField = scope + "." + fieldType.ToString();
			// Check if the configuration exists
			if (LastUsedFieldValues == null) {
				LastUsedFieldValues = new Dictionary<string, object>();
			}
			// check if settings for the requesting type exist, if not create!
			if (LastUsedFieldValues.ContainsKey(requestedField)) {
				LastUsedFieldValues[requestedField] = newValue;
			} else {
				LastUsedFieldValues.Add(requestedField, newValue);
			}
		}

		public WINDOWPLACEMENT GetEditorPlacement() {
			WINDOWPLACEMENT placement = WINDOWPLACEMENT.Default;
			placement.NormalPosition = new RECT(WindowNormalPosition);
			placement.MaxPosition = new POINT(WindowMaxPosition);
			placement.MinPosition = new POINT(WindowMinPosition);
			placement.ShowCmd = ShowWindowCommand;
			placement.Flags = WindowPlacementFlags;
			return placement;
		}

		public void SetEditorPlacement(WINDOWPLACEMENT placement) {
			WindowNormalPosition = placement.NormalPosition.ToRectangle();
			WindowMaxPosition = placement.MaxPosition.ToPoint();
			WindowMinPosition = placement.MinPosition.ToPoint();
			ShowWindowCommand = placement.ShowCmd;
			WindowPlacementFlags = placement.Flags;
		}

		public string Error {
			get {
				return String.Empty;
			}
		}

		public string this[string columnName] {
			get {
				string errorMessage = String.Empty;
				switch (columnName) {
					case "FirstName":
						if (FreehandSensitivity < 1) {
							errorMessage = "Should be at least 1";
						}
						if (FreehandSensitivity < 10) {
							errorMessage = "Should be at max 10";
						}
						break;
				}
				return errorMessage;
			}
		}
	}
}
