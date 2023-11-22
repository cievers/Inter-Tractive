/* This is taken from this blog post:
 * http://loyc-etc.blogspot.ca/2014/05/2d-convex-hull-in-c-45-lines-of-code.html
 *
 * All I have done is renamed "DList" to "CircularList" and then wrote a wrapper for the generic C# list.
 * The structure that is supposed to be used is *much* more efficient, but this works for my purposes.
 *
 * This can be dropped right into your Unity project and will work without any adjustments.
 *
 * Further copied and edited from https://gist.github.com/dLopreiato/7fd142d0b9728518552188794b8a750c#file-convexhull-cs-L4
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Objects.Collection;
using UnityEngine;
using Utility;

namespace Geometry.Generators {
	public class ConvexPerimeter {
		public IEnumerable<Vector2> Points {get;}
		public IEnumerable<Triangle> Faces => throw new NotImplementedException();
		public IEnumerable<Planar.Segment> Edges {
			get {
				var result = new List<Planar.Segment>();
				var points = Points.ToArray();
				for (var i = 0; i < points.Length - 1; i++) {
					result.Add(new Planar.Segment(points[i], points[i+1]));
				}
				result.Add(new Planar.Segment(points[^1], points[0]));
				return result;
			}
		}
		public float Diameter => Points.Aggregate(0f, (current, u) => Points.Select(v => (u - v).magnitude).Prepend(current).Max());
		public Edge Skewer => Points
			.SelectMany(u => Points.Select(v => new Edge(u, v)))
			.ToHashSet()
			.ToDictionary(s=> s, s => s.Size.magnitude)
			.Aggregate((x, y) => x.Value > y.Value ? x : y)
			.Key;

		public ConvexPerimeter(List<Vector2> points) {
			Points = Compute(points);
		}
		private ConvexPerimeter(List<Vector2> points, bool literal=false) {
			Points = literal ? points : Compute(points);
		}
		
		public static IEnumerable<Vector2> Compute(List<Vector2> points, bool sortInPlace = false) {
			if (!sortInPlace) {
				points = new List<Vector2>(points);
			}
			points.Sort((a, b) => a.x == b.x ? a.y.CompareTo(b.y) : a.x > b.x ? 1 : -1);

			// Importantly, CircularList provides O(1) insertion at beginning and end
			CircularList<Vector2> hull = new CircularList<Vector2>();
			int lower = 0, upper = 0; // Size of lower and upper hulls

			// Builds a hull such that the output polygon starts at the leftmost Vector2.
			for (var i = points.Count - 1; i >= 0; i--) {
				Vector2 p = points[i], p1;

				// build lower hull (at end of output list)
				while (lower >= 2 && (p1 = hull.Last).Sub(hull[^2]).Cross(p.Sub(p1)) >= 0) {
					hull.PopLast();
					lower--;
				}
				hull.PushLast(p);
				lower++;

				// Build upper hull (at beginning of output list)
				while (upper >= 2 && (p1 = hull.First).Sub(hull[1]).Cross(p.Sub(p1)) <= 0) {
					hull.PopFirst();
					upper--;
				}
				if (upper != 0) {
					// When upper=0, share the Vector2 added above
					hull.PushFirst(p);
				}
				upper++;
				Debug.Assert(upper + lower == hull.Count + 1);
			}
			hull.PopLast();
			return hull;
		}

		// public ConvexPerimeter[] Split(Planar.Line line) {
		// 	var a = new List<Vector2>();
		// 	var b = new List<Vector2>();
		// 	var intersected = false;
		// 	foreach (var edge in Edges) {
		// 		if (intersected) {
		// 			b.Add(edge.Start);
		// 		} else {
		// 			a.Add(edge.Start);
		// 		}
		// 		var intersection = line.Intersection(edge);
		// 		if (intersection != null) {
		// 			intersected = true;
		// 			var point = (Vector2) intersection;
		// 			a.Add(point);
		// 			b.Add(point);
		// 		}
		// 	}
		// 	return b.Count == 0 ? new[] {this} : new ConvexPerimeter[] {new(a, true), new(b, true)};
		// }
		public ConvexPerimeter[] Split(Planar.Line line) {
			var a = new List<Vector2>();
			var b = new List<Vector2>();
			foreach (var point in Points) {
				var side = line.Side(point);
				switch (side) {
					case Side.Positive:
						a.Add(point);
						break;
					case Side.Negative:
						b.Add(point);
						break;
					case Side.Contained:
						a.Add(point);
						b.Add(point);
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
			foreach (var edge in Edges) {
				var intersection = line.Intersection(edge);
				if (intersection != null) {
					var point = (Vector2) intersection;
					a.Add(point);
					b.Add(point);
				}
			}
			return a.Count == 0 || b.Count == 0 ? new[] {this} : new ConvexPerimeter[] {new(a), new(b)};
		}
	}
}