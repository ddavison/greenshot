using System.Windows;
using System.Windows.Controls;

namespace GreenshotPlugin.WPF {
	/// <summary>
	/// Logic for the ExportSettingsControl.xaml
	/// </summary>
	public partial class ExportSettingsControl : UserControl {
		public static readonly DependencyProperty HasFilenamePatternProperty = DependencyProperty.Register("HasFilenamePattern", typeof(bool), typeof(ExportSettingsControl));

		public bool HasFilenamePattern {
			get {
				return (bool)GetValue(HasFilenamePatternProperty);
			}
			set {
				SetValue(HasFilenamePatternProperty, value);
			}
		}

		public ExportSettingsControl() {
			InitializeComponent();
		}
	}
}
