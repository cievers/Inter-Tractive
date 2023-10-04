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
			Complete(
				tract.Normals
					.Select((normal, i) => new ConvexPolygon(
						Plane.Intersections(new ConvexPolyhedron(tractogram.Slice(i).ToList()).Edges, tract.Points[i], normal).ToList(), 
						tract.Points[i], 
						normal
					))
					.ToList()
			);
		}
	}
}