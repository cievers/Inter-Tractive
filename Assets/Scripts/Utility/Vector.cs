using UnityEngine;

namespace Utility {
	public static class Vector {
		// TODO: These are only used by ConvexPolygon, but shouldn't that just use these functions from Unity itself?
		public static Vector2 Sub(this Vector2 a, Vector2 b) {
			return a - b;
		}

		public static float Cross(this Vector2 a, Vector2 b) {
			return a.x * b.y - a.y * b.x;
		}
	}
}