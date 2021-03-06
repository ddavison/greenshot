﻿/*
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

namespace GreenshotImgurPlugin {
	/// <summary>
	/// Description of ImgurConfiguration.
	/// </summary>
	[IniSection("Imgur", Description="Greenshot Imgur Plugin configuration")]
	public class ImgurConfiguration : IniSection {
		[IniProperty("ImgurApiUrl", Description = "Url to Imgur system.", DefaultValue = "http://api.imgur.com/2")]
		public string ImgurApiUrl {
			get;
			set;
		}
		
		[IniProperty("UploadFormat", Description="What file type to use for uploading", DefaultValue="png")]
		public OutputFormat UploadFormat {
			get;
			set;
		}
		
		[IniProperty("UploadJpegQuality", Description="JPEG file save quality in %.", DefaultValue="80")]
		public int UploadJpegQuality {
			get;
			set;
		}
		
		[IniProperty("UploadReduceColors", Description="Reduce color amount of the uploaded image to 256", DefaultValue="False")]
		public bool UploadReduceColors {
			get;
			set;
		}
		
		[IniProperty("UsePageLink", Description = "Use pagelink instead of direct link on the clipboard", DefaultValue = "False")]
		public bool UsePageLink {
			get;
			set;
		}
		
		[IniProperty("AnonymousAccess", Description = "Use anonymous access to Imgur", DefaultValue="true")]
		public bool AnonymousAccess {
			get;
			set;
		}
		
		[IniProperty("ImgurToken", Description = "The Imgur token", Encrypted=true, ExcludeIfNull=true)]
		public string ImgurToken {
			get;
			set;
		}
		
		[IniProperty("ImgurTokenSecret", Description = "The Imgur token secret", Encrypted=true, ExcludeIfNull=true)]
		public string ImgurTokenSecret {
			get;
			set;
		}
		
		[IniProperty("AddTitle", Description = "Is the title passed on to Imgur", DefaultValue = "True")]
		public bool AddTitle {
			get;
			set;
		}
		
		[IniProperty("AddFilename", Description = "Is the filename passed on to Imgur", DefaultValue = "True")]
		public bool AddFilename {
			get;
			set;
		}
		
		[IniProperty("ImgurUploadHistory", Description="Imgur upload history (ImgurUploadHistory.hash=deleteHash)")]
		public Dictionary<string, string> ImgurUploadHistory {
			get;
			set;
		}
		
		// Not stored, only run-time!
		public Dictionary<string, ImgurInfo> runtimeImgurHistory = new Dictionary<string, ImgurInfo>();
		public int Credits {
			get;
			set;
		}

		/// <summary>
		/// Supply values we can't put as defaults
		/// </summary>
		/// <param name="property">The property to return a default for</param>
		/// <returns>object with the default value for the supplied property</returns>
		public override object GetDefault(string property) {
			switch(property) {
				case "ImgurUploadHistory":
					return new Dictionary<string, string>();
			}
			return null;
		}
	}
}
