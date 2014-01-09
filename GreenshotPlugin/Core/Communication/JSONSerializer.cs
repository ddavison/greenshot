using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;

namespace GreenshotPlugin.Core {

	/// <summary>
	/// A simple helper class for the DataContractJsonSerializer
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class JSONSerializer<T> {
		private static DataContractJsonSerializer dataContractJsonSerializer = new DataContractJsonSerializer(typeof(T));
		public static T Deserialize(string jsonString) {
			using (MemoryStream stream = new MemoryStream()) {
				byte[] content = Encoding.UTF8.GetBytes(jsonString);
				stream.Write(content, 0, content.Length);
				stream.Seek(0, SeekOrigin.Begin);
				return (T)dataContractJsonSerializer.ReadObject(stream);
			}
		}

		public static string Serialize(T jsonObject) {
			using (MemoryStream stream = new MemoryStream()) {
				dataContractJsonSerializer.WriteObject(stream, jsonObject);
				return Encoding.UTF8.GetString(stream.ToArray());
			}
		}
	}
}
