using System.Collections.Generic;
using System.Linq;
using Geometry.Tracts;

namespace Statistics.Geometric {
	public class Length : TractStatistic<float> {
		private readonly Dictionary<Tract, float> cache;

		public Length() {
			cache = new Dictionary<Tract, float>();
		}
		public override float Measure(IEnumerable<Tract> tracts) {
			var array = tracts as Tract[] ?? tracts.ToArray();
			var total = 0f;
			foreach (var tract in array) {
				if (!cache.TryGetValue(tract, out var length)) {
					length = tract.Segments.Sum(segment => segment.Size.magnitude);
					cache[tract] = length;
				}
				total += length;
			}
			return total / array.Length;
		}
		protected float MeasureImmediate(IEnumerable<Tract> tracts) {
			var array = tracts as Tract[] ?? tracts.ToArray();
			return array.SelectMany(tract => tract.Segments).Sum(segment => segment.Size.magnitude) / array.Length;
		}
	}
}