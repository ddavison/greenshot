/*
 * Greenshot - a free and open source screenshot tool
 * Copyright (C) 2007-2013  Thomas Braun, Jens Klingen, Robin Krom, Francis Noel
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
using System.Text;
using System.Threading;
using System.IO;
using GreenshotPlugin.Core;
using Greenshot.IniFile;
using System.Diagnostics;

namespace log4net {
	public enum LogLevel {
		DEBUG,
		INFO,
		WARN,
		ERROR,
		FATAL
	}

	public interface ILog {
		void Debug(string msg);
		void Debug(Exception ex);
		void Debug(string msg, Exception ex);
		void Error(string msg);
		void Error(Exception ex);
		void Error(string msg, Exception ex);
		void Info(string msg);
		void Warn(string msg);
		void Warn(Exception ex);
		void Warn(string msg, Exception ex);
		void Fatal(string msg);

		void DebugFormat(string msg, params object[] objs);
		void ErrorFormat(string msg, params object[] objs);
		void InfoFormat(string msg, params object[] objs);
		void WarnFormat(string msg, params object[] objs);
		void FatalFormat(string msg, params object[] objs);

		bool IsDebugEnabled {
			get;
		}
	}

	internal class FileLogger : ILogger {
		private static CoreConfiguration coreConfig = null;
		private static FileLogger instance = null;
		public static FileLogger Instance {
			get {
				return instance;
			}
		}

		public static void CreateInstance(string filename, int sizelimitKB) {
			if (instance == null) {
				instance = new FileLogger();
			}
			try {
				instance.Init(filename, sizelimitKB);
			} catch (Exception ex) {
				// Couldn't initialize the file logger.
				Debug.WriteLine(ex.ToString());
			}
		}
		private FileLogger() {
			AppDomain.CurrentDomain.ProcessExit += new EventHandler(CurrentDomain_ProcessExit);
			AppDomain.CurrentDomain.DomainUnload += new EventHandler(CurrentDomain_ProcessExit);
		}

		System.Timers.Timer _saveTimer;
		private Queue<String> _que = new Queue<String>(500);
		private StreamWriter _output;
		private string _filename;
		private int _sizeLimit = 0;
		private long _lastSize = 0;
		private DateTime _lastFileDate;
		private string _FilePath = "";

		public void Init(string filename, int sizelimitKB) {
			if (_filename != null) {
				return;
			}
			_sizeLimit = sizelimitKB;
			_filename = filename;
			// handle folder names as well -> create dir etc.
			_FilePath = Path.GetDirectoryName(filename);
			if (_FilePath != "") {
				_FilePath = Directory.CreateDirectory(_FilePath).FullName;
				if (_FilePath.EndsWith("\\") == false) {
					_FilePath += "\\";
				}
			}
			_output = new StreamWriter(filename, true);
			FileInfo fi = new FileInfo(filename);
			_lastSize = fi.Length;
			_lastFileDate = fi.LastWriteTime;

			_saveTimer = new System.Timers.Timer(500);
			_saveTimer.Elapsed += new System.Timers.ElapsedEventHandler(_saveTimer_Elapsed);
			_saveTimer.Enabled = true;
			_saveTimer.AutoReset = true;
		}

		void _saveTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e) {
			WriteData();
		}
 
		private void shutdown() {
			_saveTimer.Enabled = false;
			WriteData();
			if (_output != null) {
				_output.Flush();
				_output.Close();
				_output = null;
			}
		}

		void CurrentDomain_ProcessExit(object sender, EventArgs e) {
			shutdown();
		}

		private void WriteData() {
			while (_que.Count > 0) {
				string logdata = _que.Dequeue();
				if (_output != null && logdata != null) {
					if (_sizeLimit > 0) {
						// implement size limited logs
						// implement rolling logs
						#region [  rolling size limit ]
						_lastSize += logdata.Length;
						if (_lastSize > _sizeLimit * 1000) {
							_output.Flush();
							_output.Close();
							int count = 1;
							while (File.Exists(_FilePath + Path.GetFileNameWithoutExtension(_filename) + "." + count.ToString("0000")))
								count++;

							File.Move(_filename, _FilePath + Path.GetFileNameWithoutExtension(_filename) + "." + count.ToString("0000"));
							_output = new StreamWriter(_filename, true);
							_lastSize = 0;
						}
						#endregion
					}
					if (DateTime.Now.Subtract(_lastFileDate).Days > 0) {
						// implement date logs
						#region [  rolling dates  ]
						_output.Flush();
						_output.Close();
						int count = 1;
						while (File.Exists(_FilePath + Path.GetFileNameWithoutExtension(_filename) + "." + count.ToString("0000"))) {
							File.Move(_FilePath + Path.GetFileNameWithoutExtension(_filename) + "." + count.ToString("0000"), _FilePath + Path.GetFileNameWithoutExtension(_filename) + "." + count.ToString("0000") + "." + _lastFileDate.ToString("yyyy-MM-dd"));
							count++;
						}
						File.Move(_filename, _FilePath + Path.GetFileNameWithoutExtension(_filename) + "." + count.ToString("0000") + "." + _lastFileDate.ToString("yyyy-MM-dd"));

						_output = new StreamWriter(_filename, true);
						_lastFileDate = DateTime.Now;
						_lastSize = 0;
						#endregion
					}
					_output.Write(logdata);
				}
			}
			if (_output != null) {
				_output.Flush();
			}
		}

		public override void Log(LogLevel level, string typename, string msg, params object[] objs) {
			if (coreConfig == null && IniConfig.isInitialized) {
				coreConfig = IniConfig.GetIniSection<CoreConfiguration>(false);
			}
			if (coreConfig == null || level >= coreConfig.LogLevel) {
				_que.Enqueue(FormatLog(level, typename, msg, objs));
			}
		}
	}

	internal abstract class ILogger {
		protected string FormatLog(LogLevel level, string type, string msg, object[] objs) {
			StringBuilder sb = new StringBuilder();
			sb.Append(DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff"));
			sb.Append("|").Append(level).Append("|").Append(Thread.CurrentThread.ManagedThreadId).Append("|").Append(type).Append("|");
			if (objs != null) {
				sb.AppendFormat(msg, objs).AppendLine();
			} else {
				sb.AppendLine(msg);
			}
			return sb.ToString();
		}

		public abstract void Log(LogLevel level, string typename, string msg, params object[] objs);
	}

	internal class ConsoleLogger : ILogger {
		private static CoreConfiguration coreConfig = null;

		private static ConsoleLogger instance = null;
		public static ConsoleLogger Instance {
			get {
				return instance;
			}
		}

		public static void CreateInstance() {
			if (instance == null) {
				instance = new ConsoleLogger();
			}

		}

		private ConsoleLogger() {
			AppDomain.CurrentDomain.ProcessExit += new EventHandler(CurrentDomain_ProcessExit);
			AppDomain.CurrentDomain.DomainUnload += new EventHandler(CurrentDomain_ProcessExit);
			_saveTimer = new System.Timers.Timer(500);
			_saveTimer.Elapsed += new System.Timers.ElapsedEventHandler(_saveTimer_Elapsed);
			_saveTimer.Enabled = true;
			_saveTimer.AutoReset = true;
		}

		System.Timers.Timer _saveTimer;
		private Queue<String> _que = new Queue<String>(500);

		void _saveTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e) {
			WriteData();
		}

		private void shutdown() {
			_saveTimer.Enabled = false;
			WriteData();
		}

		void CurrentDomain_ProcessExit(object sender, EventArgs e) {
			shutdown();
		}

		private void WriteData() {
			StringBuilder sb = new StringBuilder();
			while (_que.Count > 0) {
				sb.Append(_que.Dequeue());
			}
			if (sb.Length > 0) {
				Debug.Write(sb.ToString());
			}
		}

		public override void Log(LogLevel level, string typename, string msg, params object[] objs) {
			if (coreConfig == null && IniConfig.isInitialized) {
				coreConfig = IniConfig.GetIniSection<CoreConfiguration>(false);
			}
			if (coreConfig == null || level >= coreConfig.LogLevel) {
				_que.Enqueue(FormatLog(level, typename, msg, objs));
			}
		}
	}

	internal class Logger : ILog {
		public Logger(Type type) {
			typename = type.Namespace + "." + type.Name;
		}

		private string typename = "";

		private void log(LogLevel level, string msg, params object[] objs) {
			try {
				if (FileLogger.Instance != null) {
					FileLogger.Instance.Log(level, typename, msg, objs);
				} else if (ConsoleLogger.Instance != null) {
					ConsoleLogger.Instance.Log(level, typename, msg, objs);
				}
			} catch  {
				// Ignore
			}
		}

		#region ILog Members

		public bool IsDebugEnabled {
			get {
				return true;
			}
		}

		public void Debug(string msg) {
			log(LogLevel.DEBUG, msg, null);
		}

		public void Debug(Exception ex) {
			log(LogLevel.DEBUG, "{0}", ex);
		}

		public void Debug(string msg, Exception ex) {
			log(LogLevel.DEBUG, msg + " {0}", ex);
		}

		public void DebugFormat(string msg, params object[] objs) {
			log(LogLevel.DEBUG, msg, objs);
		}

		public void Error(string msg) {
			log(LogLevel.ERROR, msg, null);
		}

		public void Error(Exception ex) {
			log(LogLevel.ERROR, "{0}", ex);
		}

		public void Error(string msg, Exception ex) {
			log(LogLevel.ERROR, msg + " {0}", ex);
		}

		public void ErrorFormat(string msg, params object[] objs) {
			log(LogLevel.ERROR, msg, objs);
		}

		public void Info(string msg) {
			log(LogLevel.INFO, msg, null);
		}

		public void InfoFormat(string msg, params object[] objs) {
			log(LogLevel.INFO, msg, objs);
		}

		public void Warn(string msg) {
			log(LogLevel.WARN, msg, null);
		}

		public void Warn(Exception ex) {
			log(LogLevel.WARN, "{0}", ex);
		}

		public void Warn(string msg, Exception ex) {
			log(LogLevel.WARN, msg + " {0}", ex);
		}

		public void WarnFormat(string msg, params object[] objs) {
			log(LogLevel.WARN, msg, objs);
		}

		public void Fatal(string msg) {
			log(LogLevel.FATAL, msg, null);
		}

		public void FatalFormat(string msg, params object[] objs) {
			log(LogLevel.FATAL, msg, objs);
		}
		#endregion
	}

	public static class LogManager {
		public static ILog GetLogger(Type obj) {
			// Ensure console logger is always available
			Configure();
			return new Logger(obj);
		}
		public static void Configure() {
			ConsoleLogger.CreateInstance();
		}

		public static void Configure(string filename, int sizelimitKB) {
			FileLogger.CreateInstance(filename, sizelimitKB);
		}
	}
}
