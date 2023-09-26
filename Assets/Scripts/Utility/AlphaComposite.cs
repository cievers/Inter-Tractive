using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using Camera;
using UnityEngine;

namespace Utility {
	public class AlphaComposite {
		// TODO: This feels like it can be generalized to an interface/set of classes that perform one single-value result computation on a separate thread
		private readonly Texture2D white;
		private readonly Texture2D black;
		private readonly Frame frame;
		private readonly ConcurrentBag<Color32[,]> output;

		public AlphaComposite(Texture2D white, Texture2D black, Frame frame, ConcurrentBag<Color32[,]> output) {
			this.white = white;
			this.black = black;
			this.frame = frame;
			this.output = output;
			new Thread(Composite).Start();
		}

		// public void Write(string path) {
		// 	this.path = path;
		// 	new Thread(Store).Start();
		// }
		private void Composite() {
			// var alpha = new Texture2D(frame.Width, frame.Height, TextureFormat.ARGB32, false);
			var result = new Color32[frame.Width,frame.Height];
			for (var u = 0; u < frame.Width; u++) {
				for (var v = 0; v < frame.Height; v++) {
					result[u,v] = ResolveColor(white.GetPixel(u, v), black.GetPixel(u, v));
				}
			}
			output.Add(result);
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
	}
}