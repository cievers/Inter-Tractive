using System.Collections.Generic;
using Geometry.Generators;
using Objects.Concurrent;

namespace Objects.Sources.Progressive {
	public class CrossSectionExtrema : Promise<List<ConvexPolygon>> {
		private List<ConvexPolygon> cuts;
		private int prominence;

		private bool receivedCuts;

		public CrossSectionExtrema(List<ConvexPolygon> cuts, int prominence) {
			this.cuts = cuts;
			this.prominence = prominence;
			Start();
		}
		public CrossSectionExtrema(Promise<List<ConvexPolygon>> promisedCuts, int prominence) {
			this.prominence = prominence;
			promisedCuts.Request(cuts => {
				this.cuts = cuts;
				receivedCuts = true;
				Start();
			});
		}

		public void UpdateProminence(int prominence) {
			this.prominence = prominence;
			if (receivedCuts) {
				Start();
			}
		}
		
		protected override void Compute() {
			var result = new List<ConvexPolygon>();
			for (var i = 0; i < cuts.Count; i+=prominence) {
				result.Add(cuts[i]);
			}
			Complete(result);
		}
	}
}