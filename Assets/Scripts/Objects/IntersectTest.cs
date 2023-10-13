using Geometry.Planar;
using UnityEngine;

namespace Objects {
	public class IntersectTest : MonoBehaviour {
		private void Start() {
			var line = new Line(Vector2.zero, Vector2.right * 2);
			var segment1 = new Segment(new Vector2(1, 1), new Vector2(1, -1));
			var segment2 = new Segment(new Vector2(1, 1), new Vector2(1, 0.5f));
			var segment3 = new Segment(new Vector2(3, 1), new Vector2(3, -1));
			
			Debug.Log(line.Intersection(segment1));
			Debug.Log(line.Intersection(segment2));
			Debug.Log(line.Intersection(segment3));
		}
	}
}