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
		
		public void Write(string path) {
			Publication.Png.Write(path, texture.EncodeToPNG());
		}
	}
}