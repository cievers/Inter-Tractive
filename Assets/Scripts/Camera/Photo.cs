using System;
using System.Collections.Concurrent;
using Files.Types;
using UnityEngine;
using Utility;

namespace Camera {
	public class Photo : MonoBehaviour {
		public new UnityEngine.Camera camera;
		public int width = 1920; 
		public int height = 1080;

		private bool takeHiResShot;
		private bool ready = true;
		private string path;
		private ConcurrentBag<Color32[,]> composite;

		private void Start() {
			composite = new ConcurrentBag<Color32[,]>();
		}
		private void Update() {
			if (composite.TryTake(out var colors)) {
				new Png(colors).Write(path);
				ready = true;
				Stored?.Invoke(path);
			}
		}
		private void LateUpdate() {
			takeHiResShot |= Input.GetKeyDown("k");
			if (takeHiResShot) {
				Trigger();
				takeHiResShot = false;
			}
		}
		public void Trigger() {
			if (!ready) {
				throw new TimeoutException("This photo camera is not ready to process another capture");
			}
			Capture();
		}
		private void Capture() {
			// We're now busy processing a capture
			ready = false;
			path = PhotoStamp();
			
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
			var _ = new AlphaComposite(white, black, frame, composite);
			Captured?.Invoke(path);
			//
			// bytes = white.EncodeToPNG();
			// filename = PhotoName(frame.Width, frame.Height+1);
			// System.IO.File.WriteAllBytes(filename, bytes);
			//
			// bytes = black.EncodeToPNG();
			// filename = PhotoName(frame.Width, frame.Height+2);
			// System.IO.File.WriteAllBytes(filename, bytes);
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
		private static string PhotoStamp() {
			return $"{Application.dataPath}/../Screenshots/{DateTime.Now:yyyy-MM-dd_HH-mm-ss.ms}.png";
		}

		public delegate void PhotoEvent(string name);
		public event PhotoEvent Captured;
		public event PhotoEvent Stored;
	}
}