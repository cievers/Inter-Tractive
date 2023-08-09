using System.Collections.Generic;
using System.Linq;
using Geometry.Tracts;

namespace Evaluation.Geometric {
	public abstract class CachedMetric : TractMetric {
		private readonly Dictionary<Tract, float> cache = new();
		
		public override Vector Measure(IEnumerable<Tract> tracts) {
			var array = tracts as Tract[] ?? tracts.ToArray();
			var total = 0f;
			foreach (var tract in array) {
				if (!cache.TryGetValue(tract, out var value)) {
					value = Measure(tract);
					cache[tract] = value;
				}
				total += value;
			}
			return new Vector(total / array.Length);
		}
		public abstract float Measure(Tract tract);
	}
}