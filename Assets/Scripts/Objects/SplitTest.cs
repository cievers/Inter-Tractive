using System;
using System.Collections.Generic;
using Geometry.Generators;
using Geometry.Planar;
using UnityEngine;

namespace Objects {
	public class SplitTest : MonoBehaviour {
		private void Start() {
			var line = new Line(new Vector2(1, -1), new Vector2(0, 1));
			var perimeter = new ConvexPerimeter(new List<Vector2> {
				new(0, 0),
				new(2, 0),
				new(0, 1),
				new(2, 1),
			});
			foreach (var split in perimeter.Split(line)) {
				Debug.Log("A part of the polygon!");
				foreach (var point in split.Points) {
					Debug.Log(point);
				}
			}
		}
	}
}