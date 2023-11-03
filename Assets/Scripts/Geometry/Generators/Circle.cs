using System;
using UnityEngine;

namespace Geometry.Generators {
	public static class Circle {
		public static Walk Walk(Vector3 origin, Vector3 axisA, Vector3 axisB, int vertices) {
			var result = new Vector3[vertices];
			for(var i = 0; i < vertices; i++) {
				var t = (float) i / vertices;
				result[i] = origin + axisA * Mathf.Cos(2 * Mathf.PI * t) + axisB * Mathf.Sin(2 * Mathf.PI * t);
			}
			return new Walk(result);
		}
		public static Walk Walk(Vector3 origin, Tuple<Vector3, Vector3> axis, int vertices) {
			return Walk(origin, axis.Item1, axis.Item2, vertices);
		}
	}
}