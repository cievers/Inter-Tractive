using System;
using System.Collections.Generic;
using System.Linq;
using Geometry.Generators;
using Objects.Concurrent;

namespace Objects.Sources.Progressive {
	public class CrossSectionExtrema : Promise<List<ConvexPolygon>> {
		private List<ConvexPolygon> cuts;
		private float prominence;

		private bool receivedCuts;

		public CrossSectionExtrema(List<ConvexPolygon> cuts, float prominence) {
			this.cuts = cuts;
			this.prominence = prominence;
			Start();
		}
		public CrossSectionExtrema(Promise<List<ConvexPolygon>> promisedCuts, float prominence) {
			this.prominence = prominence;
			promisedCuts.Request(cuts => {
				this.cuts = cuts;
				receivedCuts = true;
				Start();
			});
		}
		
		public void UpdateProminence(float prominence) {
			this.prominence = prominence;
			if (receivedCuts) {
				Start();
			}
		}
		
		protected override void Compute() {
			var array = cuts.ToArray();
			var areas = cuts.Select(cut => cut.Area()).ToArray();
			var range = (int) Math.Round(prominence * array.Length);
			var result = new List<ConvexPolygon>();
			for (var i = 0; i < array.Length; i++) {
				var min = float.MaxValue;
				var minIndex = i;
				var max = float.MinValue;
				var maxIndex = i;
				for (var j = Math.Max(i - range, 0); j < Math.Min(i + range, array.Length-1); j++) {
					if (areas[j] < min) {
						min = areas[j];
						minIndex = j;
					}
					if (areas[j] > max) {
						max = areas[j];
						maxIndex = j;
					}
				}
				if (minIndex == i || maxIndex == i) {
					result.Add(array[i]);
				}
			}
			Complete(result);
		}
	}
}