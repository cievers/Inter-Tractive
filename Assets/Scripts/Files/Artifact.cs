using System;
using UnityEngine;

namespace Files {
	public class Artifact {
		public static string Write(Publication.Publication file, string extension) {
			var path = Location(extension);
			file.Write(path);
			return path;
		}
		public static string Write(Publication.Publication file, string directory, string extension) {
			var path = directory + Stamp(extension);
			file.Write(path);
			return path;
		}
		public static string Directory() {
			return $"{Application.dataPath}/../Artifacts/";
		}
		public static string Location(string extension) {
			return Directory() + Stamp(extension);
		}
		private static string Stamp(string extension) {
			return $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss.ms}.{extension}";
		}
	}
}