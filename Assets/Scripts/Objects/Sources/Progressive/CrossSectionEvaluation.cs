using System.Collections.Generic;
using System.Linq;
using Geometry;
using Geometry.Generators;
using Objects.Concurrent;
using UnityEngine;

namespace Objects.Sources.Progressive {
	public class CrossSectionEvaluation : Promise<Model> {
		private List<ConvexPolygon> cuts;

		public CrossSectionEvaluation(List<ConvexPolygon> cuts) {
			this.cuts = cuts;
			Start();
		}
		public CrossSectionEvaluation(Promise<List<ConvexPolygon>> promisedCuts) {
			promisedCuts.Request(cuts => {
				this.cuts = cuts;
				Start();
			});
		}
		
		protected override void Compute() {
			var array = cuts.ToArray();
			var areas = cuts.Select(cut => cut.Area()).ToArray();
			var upper = areas.Max();
			var lower = areas.Min();
			var colored = new Model[array.Length];
			for (var i = 0; i < array.Length; i++) {
				var color = (byte) (areas[i] / upper * 255);
				// var color = (byte) ((areas[i] - lower) / (upper - lower) * 255);
				colored[i] = array[i].Hull().Color(new Color32(color, color, color, 255));
			}
			Complete(Model.Join(colored));
		}
	}
}