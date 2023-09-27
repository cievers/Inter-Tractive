using System.Collections.Generic;
using System.Linq;
using Geometry.Tracts;
using UnityEngine;

namespace Geometry {
	public class Plane {
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