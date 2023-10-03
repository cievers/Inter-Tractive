using System.Collections.Generic;
using System.Linq;
using Geometry.Tracts;
using UnityEngine;

namespace Geometry {
	public class Plane {
		public static Vector3 Projection(Vector3 point, Vector3 origin, Vector3 normal) {
			var normalized = normal.normalized;
			return point - normalized * Vector3.Dot(normalized, point - origin);
		}
		public static IEnumerable<Vector3> Projections(IEnumerable<Vector3> points, Vector3 origin, Vector3 normal) {
			return points.Select(point => Projection(point, origin, normal));
		}
		public static Vector3? Intersection(Segment segment, Vector3 origin, Vector3 normal) {
			// origin & normal define the plane
		    // origin Is a point on the plane
		    // normal Is a normal vector defining the plane direction (does not need to be normalized)
			var u = segment.Size;
			var dot = Vector3.Dot(normal, u);

			if (Mathf.Abs(dot) > 0) {
				// The factor is a multiplication of u representing where the intersection is
				// if this factor is between (0 - 1) the point intersects with the segment
				var v = segment.Start - origin;
				var fac = -Vector3.Dot(normal, v) / dot;
				if (fac is >= 0 and <= 1) {
					return segment.Start + u * fac;
				}
			}

			// The segment is parallel to plane, or beyond the bounds of the line segment
			return null;
		}
		public static Vector3? Intersection(Edge edge, Vector3 origin, Vector3 normal) {
			// The creation of a Segment instance seems unnecessarily costly here
			return Intersection(new Segment(edge.A, edge.B), origin, normal);
		}
		public static IEnumerable<Vector3> Intersections(IEnumerable<Edge> segments, Vector3 origin, Vector3 normal) {
			return segments
				.Select(edge => Intersection(edge, origin, normal))
				.Where(intersection => intersection != null)
				.Select(intersection => (Vector3) intersection);
		}
		public static IEnumerable<Vector3> Intersections(IEnumerable<Segment> segments, Vector3 origin, Vector3 normal) {
			return segments
				.Select(segment => Intersection(segment, origin, normal))
				.Where(intersection => intersection != null)
				.Select(intersection => (Vector3) intersection);
		}
		public static IEnumerable<Vector3> Intersections(Tract tract, Vector3 origin, Vector3 normal) {
			return tract.Segments
				.Select(segment => Intersection(segment, origin, normal))
				.Where(intersection => intersection != null)
				.Select(intersection => (Vector3) intersection);
		}
		public static IEnumerable<Vector3> Intersections(Tractogram tractogram, Vector3 origin, Vector3 normal) {
			return tractogram.Tracts.SelectMany(tract => Intersections(tract, origin, normal));
		}
	}
}