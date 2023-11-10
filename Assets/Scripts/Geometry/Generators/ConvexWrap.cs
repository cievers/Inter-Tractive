using System;
using System.Collections.Generic;
using System.Linq;
using Geometry.Topology;
using UnityEngine;

namespace Geometry.Generators {
	public class ConvexWrap {
		private readonly ConvexPolygon start;
		private readonly ConvexPolygon end;

		public ConvexWrap(ConvexPolygon start, ConvexPolygon end) {
			this.start = start;
			this.end = end;
			// var intersection = start.IntersectionLine(end);
			// if (intersection != null) {
			// 	// Debug.Log("Defined a wrap from two intersecting planes");
			// 	// Debug.Log(intersection);
			// 	var line = (Line) intersection;
			// 	var startSplit = start.Split(line);
			// 	var endSplit = end.Split(line);
			// 	
			// 	// Debug.Log(startSplit.Length);
			// 	// Debug.Log(endSplit.Length);
			// 	// foreach (var split in start.Split(line).Concat(end.Split(line))) {
			// 	// 	Debug.Log("Split a part of a polygon!");
			// 	// 	foreach (var point in split.Points) {
			// 	// 		Debug.Log(point);
			// 	// 	}
			// 	// }
			// 	if (startSplit.Length > 1 && endSplit.Length > 1) {
			// 		var startSidesA = startSplit[0].Points.Select(end.Side);
			// 		var startSidesB = startSplit[1].Points.Select(end.Side);
			// 		var endSidesA = endSplit[0].Points.Select(start.Side);
			// 		var endSidesB = endSplit[1].Points.Select(start.Side);
			//
			// 		this.start = startSidesA.Count(side => side == Side.Positive) >= startSidesB.Count(side => side == Side.Positive) ? startSplit[0] : startSplit[1];
			// 		this.end = endSidesA.Count(side => side == Side.Negative) >= endSidesB.Count(side => side == Side.Negative) ? endSplit[0] : endSplit[1];
			// 	}
			// }
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