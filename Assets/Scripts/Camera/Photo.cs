using System;
using System.Linq;
using Files.Types;
using UnityEngine;

namespace Camera {
	public class Photo : MonoBehaviour {
		public new UnityEngine.Camera camera;
		public int width = 1920; 
		public int height = 1080;
		
		public void Capture() {
			// Set up camera for a transparent render
			var frame = PhotoSize();
			var filter = camera.clearFlags;
			var background = camera.backgroundColor;
			var buffer = new RenderTexture(frame.Width, frame.Height, 24);
			camera.targetTexture = buffer;
			camera.clearFlags = CameraClearFlags.SolidColor;
			
			// Capture the subject on two different backgrounds
			var white = Capture(buffer, frame, Color.white);
			var black = Capture(buffer, frame, Color.black);
				
			// Revert settings from the camera
			camera.clearFlags = filter;
			camera.backgroundColor = background;
			camera.targetTexture = null;
			RenderTexture.active = null; // JC: added to avoid errors
			Destroy(buffer);

			// Process the different backgrounds to find true alpha blending values of the subject against the background
			var colors = new Color32[frame.Width * frame.Height];
			for (var u = 0; u < frame.Width; u++) {
				for (var v = 0; v < frame.Height; v++) {
					colors[u + v * frame.Width] = ResolveColor(white.GetPixel(u, v), black.GetPixel(u, v));
				}
			}
			var result = new Texture2D(frame.Width, frame.Height);
			result.SetPixels32(colors);
			result.Apply();
			
			// Write it as PNG
			var path = new Png(result).Write();
			Captured?.Invoke(path);
			Debug.Log("Saved screenshot as "+path);
		}
		private Texture2D Capture(RenderTexture buffer, Frame frame, Color background) {
			camera.backgroundColor = background;
			camera.Render();
				
			RenderTexture.active = buffer;
			var texture = new Texture2D(frame.Width, frame.Height, TextureFormat.RGB24, false);
			texture.ReadPixels(new Rect(0, 0, frame.Width, frame.Height), 0, 0);
			texture.Apply();
			
			return texture;
		}
		private Frame PhotoSize() {
			// TODO: Scaling mode to contain within the given dimensions
			// TODO: Scaling mode to cover the given dimensions
			// TODO: Scaling mode to crop to the given dimensions (return literal values)
			var currentWidth = camera.scaledPixelWidth;
			var currentHeight = camera.scaledPixelHeight;
			var scaleWidth = (double) width / currentWidth;
			var scaleHeight = (double) height / currentHeight;
			var scale = Math.Min(scaleWidth, scaleHeight);
			return new Frame((int) (currentWidth * scale), (int) (currentHeight * scale));
		}
		private Color32 ResolveColor(Color white, Color black) {
			if (white == Color.white && black == Color.black) {
				return new Color32(0, 0, 0, 0);
			}
			if (white == black) {
				return new Color32((byte) (white.r * 255), (byte) (white.g * 255), (byte) (white.b * 255), 255);
			}
			// white.r = r * a + 1 * (1-a)
			// black.r = r * a + 0 * (1-a)
			
			// white.r = r * a + (1-a)
			// black.r = r * a
			
			// white.r = r * a + (1-a)
			// r = black.r/a
			
			// white.r = black.r/a * a + (1-a)
			// white.r = black.r + (1-a)
			// white.r - black.r = 1-a
			// white.r - black.r - 1 = -a
			// -white.r + black.r + 1 = a

			var alphas = new double[] {-white.r + black.r + 1, -white.g + black.g + 1, -white.b + black.b + 1};
			var alpha = alphas.Average();
			
			// Debug.Log("Deciphering an in-between blended color");
			// Debug.Log(white);
			// Debug.Log(black);
			// Debug.Log(alphas[0]);
			// Debug.Log(alphas[1]);
			// Debug.Log(alphas[2]);
			// Debug.Log(alpha);
			// Debug.Log(new Color((float) (black.r/alpha), (float) (black.r/alpha),(float) (black.r/alpha), (float) alpha));
			// Debug.Log(new Color32((byte) (255*black.r/alpha), (byte) (255*black.r/alpha), (byte) (255*black.r/alpha), (byte) (255*alpha)));
			
			return new Color32((byte) (255*black.r/alpha), (byte) (255*black.g/alpha), (byte) (255*black.b/alpha), (byte) (255*alpha));
		}

		public delegate void PhotoCaptured(string name);
		public event PhotoCaptured Captured;
	}
}