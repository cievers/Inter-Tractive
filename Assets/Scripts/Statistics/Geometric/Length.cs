using System.Collections.Generic;
using System.Linq;
using Geometry.Tracts;

namespace Statistics.Geometric {
	public class Length : TractStatistic<float> {
		protected override float Measure(IEnumerable<Tract> tracts) {
			var array = tracts as Tract[] ?? tracts.ToArray();
			return array.SelectMany(tract => tract.Segments).Sum(segment => segment.Size.magnitude) / array.Length;
		}
	}
}