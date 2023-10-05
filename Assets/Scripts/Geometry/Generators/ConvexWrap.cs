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
			var points = new HashSet<Vector3>(start.Points);
			var solid = new ConvexPolyhedron(start.Points.Concat(end.Points).ToList()).Hull();
			var result = new List<int>();
			for (var i = 0; i < solid.Indices.Length; i += 3) {
				var point0 = solid.Vertices[solid.Indices[i]];
				var point1 = solid.Vertices[solid.Indices[i+1]];
				var point2 = solid.Vertices[solid.Indices[i+2]];
				var point0InStart = points.Contains(point0);
				var point1InStart = points.Contains(point1);
				var point2InStart = points.Contains(point2);
				if (!((point0InStart && point1InStart && point2InStart) || (!point0InStart && !point1InStart && !point2InStart))) {
					result.Add(i);
					result.Add(i+1);
					result.Add(i+2);
				}
			}
			return new Hull(solid.Vertices, solid.Normals, result.ToArray());
		}
	}
}