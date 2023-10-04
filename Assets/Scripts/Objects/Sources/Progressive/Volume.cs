using System.Collections.Generic;
using System.Linq;
using Geometry.Generators;
using Geometry.Tracts;
using Objects.Concurrent;

namespace Objects.Sources.Progressive {
	public class Volume : Promise<ConvexPolyhedron> {
		private UniformTractogram tractogram;
		private List<ConvexPolygon> cuts;
		
		private bool receivedTractogram;
		private bool receivedCuts;

		public Volume(UniformTractogram tractogram) {
			this.tractogram = tractogram;
			Start();
		}
		public Volume(Promise<UniformTractogram> promisedTractogram, Promise<List<ConvexPolygon>> promisedCuts) {
			promisedTractogram.Request(tractogram => {
				this.tractogram = tractogram;
				receivedTractogram = true;
				Update();
			});
			promisedCuts.Request(cuts => {
				this.cuts = cuts;
				receivedCuts = true;
				Update();
			});
		}
		
		private void Update() {
			if (receivedTractogram && receivedCuts) {
				Start();
			}
		}
		
		protected override void Compute() {
			Complete(new ConvexPolyhedron(tractogram.Slice(0).ToList()));
		}
	}
}