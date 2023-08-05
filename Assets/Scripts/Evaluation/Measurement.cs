using System.Linq;

namespace Evaluation {
	public record Measurement : Vector {
		private readonly string[] units;

		public Measurement(float value, string unit) : base(value) {
			units = new []{unit};
		}
		public Measurement(float[] vector, string unit) : base(vector) {
			units = Enumerable.Repeat(unit, this.vector.Length).ToArray();;
		}
		public Measurement(float[] vector, string[] units) : base(vector) {
			this.units = units;
		}

		public Measurement Add(Measurement other) {
			return new Measurement(vector.Concat(other.vector).ToArray(), units.Concat(other.units).ToArray());
		}
	}
}