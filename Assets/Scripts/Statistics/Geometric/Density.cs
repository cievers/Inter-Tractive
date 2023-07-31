using System.Collections.Generic;
using System.Linq;
using Geometry.Tracts;

namespace Statistics.Geometric {
	public class Density : TractStatistic<int> {
		public override int Measure(IEnumerable<Tract> tracts) {
			return tracts.Count();
		}
	}
}