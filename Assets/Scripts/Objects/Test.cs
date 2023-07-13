using System;
using Geometry;
using Maps.Cells;
using UnityEngine;

namespace Objects {
	public class Test : MonoBehaviour {
		private void Start() {
			var cell = new Cuboid(Vector3.zero, Vector3.one);
			Debug.Log("Center");
			Debug.Log(cell.Intersects(new Vector3(0.5f, 0.5f, 0.5f)));
			Debug.Log("Sides");
			Debug.Log(cell.Intersects(new Vector3(0f, 0.5f, 0.5f)));
			Debug.Log(cell.Intersects(new Vector3(0.5f, 0f, 0.5f)));
			Debug.Log(cell.Intersects(new Vector3(0.5f, 0.5f, 0f)));
			Debug.Log("Corner");
			Debug.Log(cell.Intersects(Vector3.zero));
			Debug.Log(cell.Intersects(Vector3.one));
			Debug.Log("Miss");
			Debug.Log(cell.Intersects(new Vector3(1.5f, 0.5f, 0.5f)));
			Debug.Log(cell.Intersects(new Vector3(0.5f, 1.5f, 0.5f)));
			Debug.Log(cell.Intersects(new Vector3(0.5f, 0.5f, 1.5f)));
			Debug.Log(cell.Intersects(new Vector3(1, 2, 3)));
		}
	}
}