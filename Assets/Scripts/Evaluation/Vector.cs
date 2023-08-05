using System.Linq;

namespace Evaluation {
	public record Vector {
		protected readonly float[] vector;

		public int Dimensions => vector.Length;

		public Vector(float value) {
			vector = new []{value};
		}
		public Vector(float[] vector) {
			this.vector = vector;
		}

		public Vector Add(Vector other) {
			return new Vector(vector.Concat(other.vector).ToArray());
		}
		
		public float this[int dimension] => vector[dimension];
	}
}