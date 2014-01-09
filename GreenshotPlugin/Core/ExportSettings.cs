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
using System.Collections.Generic;
using System.Windows.Forms;
using Greenshot.IniFile;
using GreenshotPlugin.Controls;
using GreenshotPlugin.Core;

namespace GreenshotPlugin.Core {
	public enum OutputFormat {
		bmp,
		gif,
		jpg,
		png,
		tiff,
		greenshot
	}

	/// <summary>
	/// Description of ExportSettings.
	/// </summary>
	public class ExportSettings : IniSection {

		[IniProperty("OutputFileFilenamePattern", Description = "Filename pattern for screenshot.", DefaultValue = "${capturetime:d\"yyyy-MM-dd HH_mm_ss\"}-${title}")]
		public string OutputFileFilenamePattern {
			get;
			set;
		}

		[IniProperty("OutputFileFormat", Description = "Default file type for writing screenshots. (bmp, gif, jpg, png, tiff)", DefaultValue = "png")]
		public OutputFormat OutputFileFormat {
			get;
			set;
		}

		[IniProperty("OutputFileReduceColors", Description = "If set to true, than the colors of the output file are reduced to 256 (8-bit) colors", DefaultValue = "false")]
		public bool OutputFileReduceColors {
			get;
			set;
		}

		[IniProperty("OutputFileAutoReduceColors", Description = "If set to true the amount of colors is counted and if smaller than 256 the color reduction is automatically used.", DefaultValue = "false")]
		public bool OutputFileAutoReduceColors {
			get;
			set;
		}

		[IniProperty("OutputFileJpegQuality", Description = "JPEG file save quality in %.", DefaultValue = "80")]
		public int OutputFileJpegQuality {
			get;
			set;
		}

		[IniProperty("OutputFilePromptQuality", Description = "Ask for the quality before saving?", DefaultValue = "false")]
		public bool OutputFilePromptQuality {
			get;
			set;
		}

		[IniProperty("OutputFileIncrementingNumber", Description = "The number for the ${NUM} in the filename pattern, is increased automatically after each save.", DefaultValue = "1")]
		public uint OutputFileIncrementingNumber {
			get;
			set;
		}

		[IniProperty("OptimizePNGCommand", Description = "Optional command to execute on a temporary PNG file, the command should overwrite the file and Greenshot will read it back. Note: this command is also executed when uploading PNG's!", DefaultValue = "")]
		public string OptimizePNGCommand {
			get;
			set;
		}

		[IniProperty("OptimizePNGCommandArguments", Description = "Arguments for the optional command to execute on a PNG, {0} is replaced by the temp-filename from Greenshot. Note: Temp-file is deleted afterwards by Greenshot.", DefaultValue = "\"{0}\"")]
		public string OptimizePNGCommandArguments {
			get;
			set;
		}
	}
}
