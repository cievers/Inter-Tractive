using System;
using System.Linq;
using UnityEngine;

namespace Files.Types {
	public class Png : Publication.Png {
		private readonly Texture2D texture;

		public Png(Texture2D texture) {
			this.texture = texture;
		}
		public Png(Color32[,] pixels) {
			texture = new Texture2D(pixels.GetLength(0), pixels.GetLength(1));
			texture.SetPixels32(pixels.Cast<Color32>().ToArray());
		}

		public string Write() {
			var stamp = Stamp();
			Write(stamp);
			return stamp;
		}
		public void Write(string path) {
			Publication.Png.Write(path, texture.EncodeToPNG());
		}
		public static string Directory() {
			return $"{Application.dataPath}/../Screenshots/";
		}
		public static string Stamp() {
			return Directory() + $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss.ms}.png";
		}
	}
}