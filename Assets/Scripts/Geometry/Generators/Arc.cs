using System;
using System.Collections.Generic;
using UnityEngine;

namespace Geometry.Generators {
	public static class Arc {
		// TODO: Functions in this class are not really safe for use, but probably contain some decent starting points for these functionalities
		public static IEnumerable<Vector3> Bend(Vector2 pointA, Vector2 directionA, Vector2 pointB, Vector2 directionB, int vertices) {
			// Consider the case where the two directions are moving away from each other, meaning the normal will be flipped?
			// Also, the dot product and fraction will change
			Debug.Log(directionA);
			Debug.Log(directionB);
			var normal = Vector3.Cross(directionA, directionB).normalized;
			var lineA = new Geometry.Planar.Line(pointA, Vector3.Cross(directionA.normalized, normal).normalized);
			var lineB = new Geometry.Planar.Line(pointB, Vector3.Cross(normal, directionB.normalized).normalized);
			Debug.Log(lineA.Origin);
			Debug.Log(lineA.Direction);
			Debug.Log(lineB.Origin);
			Debug.Log(lineB.Direction);
			var origin = (Vector2) lineA.Intersection(lineB);
			var radius = (pointA - origin).magnitude;
			var axisA = (pointB - origin).normalized;
			var axisB = Vector3.Cross(normal, axisA).normalized;
			var circle = Circle.Walk(origin, axisA * radius, axisB * radius, vertices);
			Debug.Log(origin);
			Debug.Log(axisA);
			Debug.Log(axisB);
			var dot = Vector3.Dot(axisA, axisB);
			var fraction = (int) Math.Ceiling(((2 - (1 + dot)) / 2) * vertices);
			Debug.Log("Returning bend with "+fraction+" vertices");
			return circle.Points[1..fraction];
		}
		public static IEnumerable<Vector3> Semicircle(Vector3 a, Vector3 b, Vector3 normal, int vertices) {
			var origin = (a + b) / 2;
			var radius = (b - origin).magnitude;
			var axisA = (b - origin).normalized;
			var axisB = Vector3.Cross(normal,axisA).normalized;
			var circle = Circle.Walk(origin, axisA * radius, axisB * radius, vertices * 2);
			return circle.Points[..vertices];
		}
	}
}