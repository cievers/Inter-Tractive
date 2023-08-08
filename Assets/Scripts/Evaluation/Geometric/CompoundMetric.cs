using System;
using System.Collections.Generic;
using System.Linq;
using Geometry.Tracts;

namespace Evaluation.Geometric {
	public class CompoundMetric : TractMetric {
		private readonly TractMetric[] metrics;
		public override int Dimensions => metrics.Sum(metric => metric.Dimensions);
		public override string[] Units => metrics.SelectMany(metric => metric.Units).ToArray();

		public CompoundMetric(TractMetric[] metrics) {
			this.metrics = metrics;
		}
		
		public override Vector Measure(IEnumerable<Tract> tracts) {
			var measured = metrics.Select(metric => metric.Measure(tracts));
			return measured.Aggregate(new Vector(Array.Empty<float>()), (a, b) => a.Add(b));
		}
	}
}