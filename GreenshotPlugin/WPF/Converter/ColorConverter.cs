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
using System.Windows.Data;

namespace GreenshotPlugin.WPF {
	[ValueConversion(typeof(System.Drawing.Color), typeof(System.Windows.Media.Color))]
	public class ColorConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			if (value != null) {
				System.Drawing.Color source = (System.Drawing.Color)value;
				return System.Windows.Media.Color.FromArgb(source.A, source.R, source.G, source.B);
			}
			return null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			if (value != null) {
				System.Windows.Media.Color source = (System.Windows.Media.Color)value;
				return System.Drawing.Color.FromArgb(source.A, source.R, source.G, source.B);
			}
			return null;
		}
	}
}
