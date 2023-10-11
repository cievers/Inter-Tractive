using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Geometry.Generators {
	public class ConvexWrap {
		private readonly ConvexPolygon start;
		private readonly ConvexPolygon end;

		public ConvexWrap(ConvexPolygon start, ConvexPolygon end) {
			this.start = start;
			this.end = end;
		}
		
		public Hull Hull() {
			// Because of vertex splitting for unique normals per face, removing only the indices of a face leaves many
			// unused vertices and normals still in the mesh. These could and should be optimized, although that would
			// also cause the just filtered indices to shift again
			// TODO: The two convex polygons might intersect, what the hell now?!
			var points = new HashSet<Vector3>(start.Points);
			var solid = new ConvexPolyhedron(start.Points.Concat(end.Points).ToList()).Hull();
			var result = new List<int>();
			for (var i = 0; i < solid.Indices.Length; i += 3) {
				var contains0 = points.Contains(solid.Vertices[solid.Indices[i]]);
				var contains1 = points.Contains(solid.Vertices[solid.Indices[i+1]]);
				var contains2 = points.Contains(solid.Vertices[solid.Indices[i+2]]);
				if (contains0 ^ contains1 || contains1 ^ contains2) {
					result.Add(i);
					result.Add(i+1);
					result.Add(i+2);
				}
			}
			return new Hull(solid.Vertices, solid.Normals, result.ToArray());
		}
	}
}