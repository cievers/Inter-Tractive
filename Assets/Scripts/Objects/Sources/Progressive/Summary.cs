using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Evaluation.Geometric;
using Files.Types;
using Geometry.Generators;
using Geometry.Tracts;

namespace Objects.Sources.Progressive {
	public record Summary {
		public float? CoreLength {get; set;}
		public float? CoreSpan {get; set;}
		public float[] Areas {get; set;}
		public float[] Perimeters {get; set;}
		public float? Volume {get; set;}

		public void Core(Tract core) {
			CoreLength = new Length().Measure(core);
			CoreSpan = new Span().Measure(core);
		}
		public void CrossSections(IEnumerable<ConvexPolygon> cuts) {
			var array = cuts as ConvexPolygon[] ?? cuts.ToArray();
			Areas = array.Select(cut => cut.Area()).ToArray();
			Perimeters = array.Select(cut => cut.Perimeter()).ToArray();
		}

		public Json Json() {
			return new Json(JsonSerializer.Serialize(this));
		}
	}
}