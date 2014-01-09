
using GreenshotPlugin.WPF;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace GreenshotPlugin.Core.Settings {
	public static class SettingsExtensions {
		/// <summary>
		/// Is the control is design mode?
		/// </summary>
		/// <param name="contol"></param>
		/// <returns>true</returns>
		public static bool IsDesignMode(this Control contol) {
			return System.ComponentModel.DesignerProperties.GetIsInDesignMode(contol);
		}

		/// <summary>
		/// Set the content to the translated value
		/// </summary>
		/// <param name="contol"></param>
		/// <param name="languageKey">Key to use for the translation</param>
		public static void Translate(this ContentControl control, string languageKey) {
			if (control.Content == null && languageKey != null) {
				control.Content = GreenshotPlugin.Core.Language.GetString(languageKey);
			}
		}


		/// <summary>
		/// Check if the control's prop has a binding
		/// </summary>
		/// <param name="control"></param>
		/// <param name="prop"></param>
		/// <returns></returns>
		public static bool HasBinding(this Control control, DependencyProperty prop) {
			return control.GetBindingExpression(prop) != null;
		}

		/// <summary>
		/// Apply a binding on the DependencyProperty "prop" on control if the current binding is null
		/// </summary>
		/// <param name="control"></param>
		/// <param name="prop"></param>
		/// <param name="binding"></param>
		public static void SetBindingIfNull(this Control control, DependencyProperty prop, string binding) {
			if (!control.HasBinding(prop)) {
				control.SetBinding(prop, binding);
			}
		}

		/// <summary>
		/// Apply the settings Style to the control
		/// </summary>
		/// <param name="control"></param>
		public static void ApplySettingsStyle(this Control control) {
			if (control.Style == null) {
				Style settingsStyle = (Style)Application.Current.TryFindResource("SettingsControl");
				if (settingsStyle != null) {
					control.Style = settingsStyle;
				}
			}
		}

		/// <summary>
		/// Apply a binding for the expert settings to the control
		/// </summary>
		/// <param name="control">Control to bind the VisibilityProperty to</param>
		/// <param name="configPath">Config path, e.g CoreConfig or Config</param>
		/// <param name="configProperty">Property in the config</param>
		public static void ApplyExpertSettingsBinding(this Control control, string configPath, string configProperty) {
			if (!control.HasBinding(Control.VisibilityProperty)) {
				MultiBinding multiBinding = new MultiBinding();
				multiBinding.Converter = new OrBooleanConverter();
				Binding propertyVisibleBinding = new Binding(string.Format("{0}[{1}].IsVisible", configPath, configProperty));
				propertyVisibleBinding.Converter = new BooleanToVisibilityConverter();
				multiBinding.Bindings.Add(propertyVisibleBinding);
				Binding showExpertBinding = new Binding("CoreConfig.ShowExpertSettings");
				showExpertBinding.Converter = new BooleanToVisibilityConverter();
				multiBinding.Bindings.Add(showExpertBinding);
				control.SetBinding(Control.VisibilityProperty, multiBinding);
			}
			if (!control.HasBinding(Control.FontWeightProperty)) {
				Binding propertyExpertBinding = new Binding(string.Format("{0}[{1}].IsExpert", configPath, configProperty));
				propertyExpertBinding.Converter = new BoolToFontWeightConverter();
				control.SetBinding(Control.FontWeightProperty, propertyExpertBinding);
			}
		}

		/// <summary>
		/// Apply a binding for the fixed settings to the control
		/// </summary>
		/// <param name="control">Control to bind the IsEnabled to</param>
		/// <param name="configPath">Config path, e.g CoreConfig or Config</param>
		/// <param name="configProperty">Property in the config</param>
		/// <param name="dependsOnIsChecked">This control is only enabled if the dependsOnIsChecked element is checked</param>
		public static void ApplyFixedBinding(this Control control, string configPath, string configProperty, string dependsOnIsChecked) {
			if (!control.HasBinding(Control.IsEnabledProperty)) {
				Binding isEnabledBinding = new Binding(string.Format("{0}[{1}].IsEditable", configPath, configProperty));
				if (dependsOnIsChecked == null) {
					control.SetBinding(Control.IsEnabledProperty, isEnabledBinding);
				} else {
					MultiBinding multiBinding = new MultiBinding();
					multiBinding.Converter = new AndBooleanConverter();
					multiBinding.Bindings.Add(isEnabledBinding);
					Binding dependsOnBinding = new Binding("IsChecked");
					dependsOnBinding.ElementName = dependsOnIsChecked;
					multiBinding.Bindings.Add(dependsOnBinding);
					control.SetBinding(Control.IsEnabledProperty, multiBinding);
				}

			}
		}
	}
}
