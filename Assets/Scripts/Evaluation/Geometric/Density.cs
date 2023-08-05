using System.Collections.Generic;
using System.Linq;
using Geometry.Tracts;

namespace Evaluation.Geometric {
	public class Density : TractMetric {
		public override int Dimensions => 1;
		public override string[] Units => new[] {"tracts"};
		
		public override Vector Measure(IEnumerable<Tract> tracts) {
			return new Vector(tracts.Count());
		}
	}
}