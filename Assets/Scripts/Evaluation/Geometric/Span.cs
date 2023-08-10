using Geometry.Tracts;

namespace Evaluation.Geometric {
	public class Span : CachedMetric {
		public override int Dimensions => 1;
		public override string[] Units => new[] {"mm"};
		
		public override float Measure(Tract tract) {
			var points = tract.Points;
			return (points[0] - points[^1]).magnitude;
		}
	}
}