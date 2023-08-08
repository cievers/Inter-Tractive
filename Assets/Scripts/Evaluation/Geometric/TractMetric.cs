using System.Collections.Generic;
using System.Linq;
using Geometry.Tracts;
using Maps.Cells;

namespace Evaluation.Geometric {
	public abstract class TractMetric {
		public abstract int Dimensions {get;}
		public abstract string[] Units {get;}

		public abstract Vector Measure(IEnumerable<Tract> tracts);
		public virtual Dictionary<Cell, Vector> Measure(Dictionary<Cell, IEnumerable<Tract>> tractogram) {
			return tractogram.ToDictionary(pair => pair.Key, pair => Measure(pair.Value));
		}
	}
}