namespace Geometry {
	public record Range {
		public float Minimum {get;}
		public float Maximum {get;}

		public Range(float a, float b) {
			if (a <= b) {
				Minimum = a;
				Maximum = b;
			} else {
				Maximum = a;
				Minimum = b;
			}
		}
	}
}