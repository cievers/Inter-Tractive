using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
			
			new Thread(Start).Start();
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
				new Thread(Start).Start();
			}
		}
		
		protected override void Compute() {
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
			var projections = Plane.Projections(tractogram.Slice(1), origin, normal).ToList();
			// foreach (var projection in projections) {
			// 	Instantiate(dot, projection, Quaternion.identity);
			// }
			var perimeter = new ConvexPolygon(projections, origin, normal);
			// foreach (var point in perimeter.Points) {
			// 	Instantiate(dot, point, Quaternion.identity);
			// }
			// hull.Add(perimeter.Hull());
			Complete(new List<ConvexPolygon> {perimeter});
		}
	}
}