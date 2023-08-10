using Geometry.Tracts;

namespace Evaluation.Geometric {
	public class Curl : CachedMetric {
		private readonly Length length = new();
		private readonly Span span = new();
		
		public override int Dimensions => 1;
		public override string[] Units => new[] {"mm"};
		
		public override float Measure(Tract tract) {
			return length.Measure(tract) / span.Measure(tract);
		}
	}
}