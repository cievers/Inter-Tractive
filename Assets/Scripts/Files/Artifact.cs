using System;
using UnityEngine;

namespace Files {
	public class Artifact {
		public static string Write(Publication.Publication file, string extension) {
			var path = Stamp(extension);
			file.Write(path);
			return path;
		}
		public static string Directory() {
			return $"{Application.dataPath}/../Artifacts/";
		}
		public static string Stamp(string extension) {
			return Directory() + $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss.ms}.{extension}";
		}
	}
}