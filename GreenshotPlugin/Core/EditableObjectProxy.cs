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
using System.Dynamic;
using System.Reflection;
using System.Linq;
using Greenshot.IniFile;

namespace GreenshotPlugin.Core {
	public interface ITransactionalObject {
		void StartTransaction();
		void Commit();
		void Rollback();
	}
	/// <summary>
	/// This class makes a dynamic proxy, via a DynamicObject, which implements INotifyPropertyChanged
	/// </summary>
	public class DynamicProxy<T> : DynamicObject, INotifyPropertyChanged {
		#region protected methods
		protected PropertyInfo GetPropertyInfo(string propertyName) {
			return ProxiedObject.GetType().GetProperties().First(propertyInfo => propertyInfo.Name == propertyName);
		}

		protected virtual void SetMember(string propertyName, object value) {
			var propertyInfo = GetPropertyInfo(propertyName);

			if (propertyInfo.PropertyType == value.GetType()) {
				propertyInfo.SetValue(ProxiedObject, value, null);
			} else {
				propertyInfo.SetValue(ProxiedObject, Convert.ChangeType(value, propertyInfo.PropertyType), null);
			}

			RaisePropertyChanged(propertyName);
		}

		protected virtual object GetMember(string propertyName) {
			return GetPropertyInfo(propertyName).GetValue(ProxiedObject, null);
		}

		protected virtual void OnPropertyChanged(string propertyName) {
			if (PropertyChanged != null) {
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		protected virtual void RaisePropertyChanged(string propertyName) {
			OnPropertyChanged(propertyName);
		}
		#endregion

		#region constructor
		public DynamicProxy() {

		}

		public DynamicProxy(T proxiedObject) {
			ProxiedObject = proxiedObject;
		}
		#endregion

		public override bool TryConvert(ConvertBinder binder, out object result) {
			if (binder.Type == typeof(INotifyPropertyChanged)) {
				result = this;
				return true;
			}
			if (ProxiedObject != null && binder.Type.IsAssignableFrom(ProxiedObject.GetType())) {
				result = ProxiedObject;
				return true;
			} else
				return base.TryConvert(binder, out result);
		}

		public override bool TryGetMember(GetMemberBinder binder, out object result) {
			result = GetMember(binder.Name);
			return true;
		}

		public override bool TrySetMember(SetMemberBinder binder, object value) {
			SetMember(binder.Name, value);
			return true;
		}

		#region public properties
		public T ProxiedObject {
			get;
			set;
		}
		#endregion

		#region INotifyPropertyChanged Member
		public event PropertyChangedEventHandler PropertyChanged;
		#endregion
	}

	/// <summary>
	/// This class is used as a proxy which implements IEditableObject
	/// when EndEdit is called all CHANGED values are written to the original object
	/// when CancelEdit is called, nothing is done.
	/// </summary>
	public class EditableObjectProxy<T> : DynamicProxy<T>, ITransactionalObject {
		#region private nested class
		private class BackupState {
			public BackupState() {
				OriginalValues = new Dictionary<string, object>();
				NewValues = new Dictionary<string, object>();
			}

			public void SetOriginalValue(string propertyName, object value) {
				if (!OriginalValues.ContainsKey(propertyName)) {
					OriginalValues.Add(propertyName, value);
				}
			}

			public void SetNewValue(string propertyName, object value) {
				if (OriginalValues.ContainsKey(propertyName) && OriginalValues[propertyName] == value) {
					return;
				}

				if (NewValues.ContainsKey(propertyName)) {
					NewValues[propertyName] = value;
				} else {
					NewValues.Add(propertyName, value);
				}
			}

			public Dictionary<string, object> OriginalValues {
				get;
				private set;
			}
			public Dictionary<string, object> NewValues {
				get;
				private set;
			}
		}
		#endregion

		#region private members
		private BackupState _editBackup;
		#endregion

		#region protected methods
		protected override void SetMember(string propertyName, object value) {
			if (IsEditing) {
				_editBackup.SetOriginalValue(propertyName, GetPropertyInfo(propertyName).GetValue(ProxiedObject, null));
				_editBackup.SetNewValue(propertyName, value);
				RaisePropertyChanged(propertyName);
			} else {
				base.SetMember(propertyName, value);
			}
		}

		protected override object GetMember(string propertyName) {
			return IsEditing && _editBackup.NewValues.ContainsKey(propertyName) ? _editBackup.NewValues[propertyName] : base.GetMember(propertyName);
		}
		#endregion

		#region constructor
		public EditableObjectProxy() : base() {
		}

		public EditableObjectProxy(T proxiedObject) : base(proxiedObject) {
		}
		#endregion

		#region public methods
		public override bool TryConvert(ConvertBinder binder, out object result) {
			if (binder.Type == typeof(IEditableObject)) {
				result = this;
				return true;
			} else {
				return base.TryConvert(binder, out result);
			}
		}
		#endregion

		#region IEditableObject methods
		public void StartTransaction() {
			if (!IsEditing) {
				_editBackup = new BackupState();
			}
		}

		public void Rollback() {
			if (IsEditing) {
				_editBackup = null;
			}
		}

		public void Commit() {
			if (IsEditing) {
				var editObject = _editBackup;
				_editBackup = null;

				foreach (var item in editObject.NewValues) {
					SetMember(item.Key, item.Value);
				}
			}
		}
		#endregion

		#region public properties
		public bool IsEditing {
			get {
				return _editBackup != null;
			}
		}
		public bool IsChanged {
			get {
				return IsEditing && _editBackup.NewValues.Count > 0;
			}
		}
		#endregion
	}

	/// <summary>
	/// Proxy for the IniSection, this allows to access the IniValues via an indexer
	/// </summary>
	public class IniProxy : EditableObjectProxy<IniSection> {
		public IniProxy(IniSection proxiedObject) : base(proxiedObject) {
		}

		/// <summary>
		/// This makes sure the IniValues of the IniSection are available via an indexer.
		/// </summary>
		/// <param name="binder"></param>
		/// <param name="indexes">string[]{propertyName}</param>
		/// <param name="result">IniValue</param>
		/// <returns>true if IniValue is found</returns>
		public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result) {
			IniSection section = ProxiedObject as IniSection;
			string key = (string)indexes[0];
			if (section.Values.ContainsKey(key)) {
				result = section.Values[key];
				return true;
			}
			result = null;
			return false;
		}
	}
}
