using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace GreenshotPlugin.Core.Imaging {
	public static class GdiExtensionMethods {
		/// <summary>
		/// Convert a "GDI" bitmap to a WPF ImageSource
		/// </summary>
		/// <param name="icon">Icon to convert</param>
		/// <returns>ImageSource</returns>
		public static ImageSource ToImageSource(this Icon icon) {
			using (MemoryStream iconStream = new MemoryStream()) {
				icon.Save(iconStream);
				iconStream.Seek(0, SeekOrigin.Begin);

				return System.Windows.Media.Imaging.BitmapFrame.Create(iconStream);
			}
		}
	}
}
