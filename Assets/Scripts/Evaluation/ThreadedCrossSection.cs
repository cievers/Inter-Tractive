using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Geometry;
using Geometry.Generators;
using Geometry.Tracts;
using UnityEngine;

namespace Evaluation {
	public class ThreadedCrossSection {
		private readonly UniformTractogram tractogram;
		private readonly Tract tract;
		private readonly ConcurrentBag<Hull> hull;

		public ThreadedCrossSection(UniformTractogram tractogram, Tract tract, ConcurrentBag<Hull> hull) {
			this.tractogram = tractogram;
			this.tract = tract;
			this.hull = hull;
		}
		
		public void Start() {
			var origin = tract.Points[1];
			var normal = tract.Points[2] - tract.Points[0];
			// var points = new List<Vector3>();
			// var tracts = tractogram.Tracts.ToArray();
			// for (var i = 0; i < 100 ; i++) {
			// 	points.AddRange(Geometry.Plane.Intersections(tracts[i], origin, normal));
			// }
			// foreach (var point in points) {
			// 	Instantiate(dot, point, Quaternion.identity);
			// }
			var projections = Geometry.Plane.Projections(tractogram.Slice(1), origin, normal).ToList();
			// foreach (var projection in projections) {
			// 	Instantiate(dot, projection, Quaternion.identity);
			// }
			var perimeter = new ConvexPolygon(projections, origin, normal);
			// foreach (var point in perimeter.Points) {
			// 	Instantiate(dot, point, Quaternion.identity);
			// }
			hull.Add(perimeter.Hull());
		}
	}
}