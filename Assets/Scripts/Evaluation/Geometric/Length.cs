using System.Collections.Generic;
using System.Linq;
using Geometry.Tracts;

namespace Evaluation.Geometric {
	public class Length : CachedMetric {
		public override int Dimensions => 1;
		public override string[] Units => new[] {"mm"};
		
		public override float Measure(Tract tract) {
			return tract.Segments.Sum(segment => segment.Size.magnitude);
		}
		protected float MeasureImmediate(IEnumerable<Tract> tracts) {
			var array = tracts as Tract[] ?? tracts.ToArray();
			return array.SelectMany(tract => tract.Segments).Sum(segment => segment.Size.magnitude) / array.Length;
		}
	}
}