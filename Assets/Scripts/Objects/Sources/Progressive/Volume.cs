using System.Collections.Generic;
using System.Linq;
using Geometry;
using Geometry.Generators;
using Geometry.Tracts;
using Objects.Concurrent;

namespace Objects.Sources.Progressive {
	public class Volume : Promise<Hull> {
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
			var result = new List<Hull>();
			result.Add(cuts[0].Surface(false));
			for (var i = 1; i < cuts.Count; i++) {
				result.Add(new ConvexWrap(cuts[i-1], cuts[i]).Hull());
			}
			result.Add(cuts[^1].Surface());
			Complete(Hull.Join(result));
		}
	}
}