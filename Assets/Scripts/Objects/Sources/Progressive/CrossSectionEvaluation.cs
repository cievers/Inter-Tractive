using System.Collections.Generic;
using System.Linq;
using Evaluation;
using Evaluation.Coloring;
using Geometry;
using Geometry.Generators;
using Objects.Concurrent;

namespace Objects.Sources.Progressive {
	public class CrossSectionEvaluation : Promise<Model> {
		private List<ConvexPolygon> cuts;
		private readonly Coloring coloring;

		public CrossSectionEvaluation(List<ConvexPolygon> cuts, Coloring coloring) {
			this.cuts = cuts;
			this.coloring = coloring;
			Start();
		}
		public CrossSectionEvaluation(Promise<List<ConvexPolygon>> promisedCuts, Coloring coloring) {
			this.coloring = coloring;
			promisedCuts.Request(cuts => {
				this.cuts = cuts;
				Start();
			});
		}
		
		protected override void Compute() {
			Complete(Model.Join(coloring
				.Color(cuts.ToDictionary(cut => cut, cut => new Vector(cut.Area())))
				.Select(pair => pair.Key.Hull().Color(pair.Value))
				.ToArray()
			));
		}
	}
}