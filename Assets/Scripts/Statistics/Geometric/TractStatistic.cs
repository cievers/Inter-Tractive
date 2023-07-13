using System.Collections.Generic;
using System.Linq;
using Geometry.Tracts;
using Maps.Cells;

namespace Statistics.Geometric {
	public abstract class TractStatistic<T> {
		protected abstract T Measure(IEnumerable<Tract> tracts);
		public Dictionary<Cell, T> Measure(Dictionary<Cell, IEnumerable<Tract>> tractogram) {
			return tractogram.ToDictionary(pair => pair.Key, pair => Measure(pair.Value));
		}
	}
}