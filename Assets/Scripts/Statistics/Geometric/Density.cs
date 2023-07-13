using System.Collections.Generic;
using System.Linq;
using Geometry.Tracts;

namespace Statistics.Geometric {
	public class Density : TractStatistic<int> {
		protected override int Measure(IEnumerable<Tract> tracts) {
			return tracts.Count();
		}
	}
}