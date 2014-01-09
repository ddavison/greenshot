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
using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;
using Greenshot.Helpers;
using Greenshot.Configuration;

namespace Greenshot.Windows {
	/// <summary>
	/// Logic for the BugReportWindow.xaml
	/// </summary>
	public partial class BugReportWindow : Window {
		private static readonly log4net.ILog LOG = log4net.LogManager.GetLogger(typeof(BugReportWindow));

		public string ExceptionText {
			get;
			set;
		}

		public BugReportWindow(Exception exception) {
			ExceptionText = EnvironmentInfo.BuildReport(exception);
			LOG.Error(EnvironmentInfo.ExceptionToString(exception));

			InitializeComponent();
			this.DataContext = this;
		}

		private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e) {
			try {
				Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
				e.Handled = true;
			} catch (Exception) {
				MessageBox.Show(GreenshotPlugin.Core.Language.GetFormattedString(LangKey.error_openlink, e.Uri.AbsoluteUri), GreenshotPlugin.Core.Language.GetString(LangKey.error));
			}
		}
	}
}
