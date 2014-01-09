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
using System.Windows;
using System.Reflection;
using System;
using System.Windows.Threading;
using Greenshot.Helpers;
using Greenshot.Forms;
using Greenshot.Windows;

namespace Greenshot {
	/// <summary>
	/// Logic for the Application.xaml
	/// </summary>
	public partial class App : Application {
		static App() {
			AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
			AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
		}

		static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args) {
			Assembly ayResult = null;
			string sShortAssemblyName = args.Name.Split(',')[0];
			Assembly[] ayAssemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (Assembly ayAssembly in ayAssemblies) {
				if (sShortAssemblyName == ayAssembly.FullName.Split(',')[0]) {
					ayResult = ayAssembly;
					break;
				}
			}
			return ayResult;
		}

		public App() {
		}

		private void Application_Startup(object sender, StartupEventArgs e) {
			Application.Current.DispatcherUnhandledException += new DispatcherUnhandledExceptionEventHandler(Application_ThreadException);
			MainForm.Start(e.Args);
			Application.Current.Shutdown();
		}

		private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e) {
			Exception exceptionToLog = e.ExceptionObject as Exception;
			new BugReportWindow(exceptionToLog).ShowDialog();
		}

		private static void Application_ThreadException(object sender, DispatcherUnhandledExceptionEventArgs e) {
			Exception exceptionToLog = e.Exception;
			new BugReportWindow(exceptionToLog).ShowDialog();
		}
	}
}
