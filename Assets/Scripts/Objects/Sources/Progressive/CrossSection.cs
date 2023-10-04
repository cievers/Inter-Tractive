using System.Collections.Generic;
using System.Linq;
using Geometry;
using Geometry.Generators;
using Geometry.Tracts;
using Objects.Concurrent;

namespace Objects.Sources.Progressive {
	public class CrossSection : Promise<List<ConvexPolygon>> {
		private UniformTractogram tractogram;
		private Tract tract;

		private bool receivedTractogram;
		private bool receivedTract;

		public CrossSection(UniformTractogram tractogram, Tract tract) {
			this.tractogram = tractogram;
			this.tract = tract;
			
			Start();
		}
		public CrossSection(Promise<UniformTractogram> promisedTractogram, Promise<Tract> promisedTract) {
			promisedTractogram.Request(tractogram => {
				this.tractogram = tractogram;
				receivedTractogram = true;
				Update();
			});
			promisedTract.Request(tract => {
				this.tract = tract;
				receivedTract = true;
				Update();
			});
		}

		private void Update() {
			if (receivedTractogram && receivedTract) {
				Start();
			}
		}
		
		protected override void Compute() {
			// Complete(
			// 	tract.Normals
			// 		.Select((normal, i) => new ConvexPolygon(
			// 			Plane.Projections(tractogram.Slice(i), tract.Points[i], normal).ToList(), 
			// 			tract.Points[i], 
			// 			normal
			// 		))
			// 		.ToList()
			// 	);
			// return;
			var result = new List<ConvexPolygon>();
			var points = tract.Points;
			var normals = tract.Normals.ToArray();
			for (var i = 0; i < tract.Points.Length; i++) {
				// var origin = tract.Points[1];
				// var normal = tract.Points[2] - tract.Points[0];
				// var points = new List<Vector3>();
				// var tracts = tractogram.Tracts.ToArray();
				// for (var i = 0; i < 100 ; i++) {
				// 	points.AddRange(Geometry.Plane.Intersections(tracts[i], origin, normal));
				// }
				// foreach (var point in points) {
				// 	Instantiate(dot, point, Quaternion.identity);
				// }
				var projections = Plane.Projections(tractogram.Slice(i), points[i], normals[i]).ToList();
				var hull = new ConvexPolyhedron(tractogram.Slice(i).ToList());
				var intersections = Plane.Intersections(hull.Edges, points[i], normals[i]).ToList();
				// foreach (var projection in projections) {
				// 	Instantiate(dot, projection, Quaternion.identity);
				// }
				var perimeter = new ConvexPolygon(intersections, points[i], normals[i]);
				// foreach (var point in perimeter.Points) {
				// 	Instantiate(dot, point, Quaternion.identity);
				// }
				// hull.Add(perimeter.Hull());
				result.Add(perimeter);
			}
			Complete(result);
		}
	}
}