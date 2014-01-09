using System;
using System.Collections.Generic;
using Greenshot.Plugin;

namespace GreenshotPlugin.Core {
	public abstract class AbstractPlugin : IGreenshotPlugin, IDisposable {

		public virtual void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing) {
		}

		/// <summary>
		/// Implementation of the IGreenshotPlugin.Initialize
		/// </summary>
		/// <param name="host">Use the IGreenshotPluginHost interface to register events</param>
		/// <param name="metadata">IDictionary<string, string></param>
		public abstract bool Initialize(IGreenshotHost pluginHost, IDictionary<string, object> metadata);

		/// <summary>
		/// Implementation of the IPlugin.Shutdown
		/// </summary>
		public virtual void Shutdown() {
			// Do nothing
		}

		/// <summary>
		/// Implementation of the IPlugin.Configure
		/// </summary>
		public virtual void Configure() {
		}
		
		/// <summary>
		/// Implementation of the IPlugin.Configurable
		/// </summary>
		public virtual bool Configurable {
			get {
				return true;
			}
		}

		/// <summary>
		/// Implementation of the IPlugin.Destinations
		/// </summary>
		public virtual IEnumerable<IDestination> Destinations() {
			yield break;
		}

		/// <summary>
		/// Implementation of the IPlugin.Processors
		/// </summary>
		public virtual IEnumerable<IProcessor> Processors() {
			yield break;
		}
	}
}
