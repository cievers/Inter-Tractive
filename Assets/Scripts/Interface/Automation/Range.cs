using System;

namespace Interface.Automation {
	public class Range {
		private readonly float minimum;
		private readonly float maximum;
		private readonly Action<float> action;
		private readonly Func<float, float> transformation;
		
		public Range(Paradigm.Range<float> paradigm) {
			minimum = paradigm.Minimum;
			maximum = paradigm.Maximum;
			action = paradigm.Action;
			transformation = paradigm.Transformation;
		}

		public void Simulate(float fraction) {
			Automate(transformation.Invoke(minimum + (maximum - minimum) * fraction));
		}
		public void Automate(float value) {
			action.Invoke(value);
		}
	}
}