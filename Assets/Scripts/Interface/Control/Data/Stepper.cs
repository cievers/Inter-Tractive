using System;

namespace Interface.Control.Data {
	public record Stepper(string Name, float Interval, float Default, float Minimum, float Maximum, Action<float> Action) : Configuration {
		public string Name {get; private set;} = Name;
		public float Interval {get; private set;} = Interval;
		public float Default {get; private set;} = Default;
		public float Minimum {get; private set;} = Minimum;
		public float Maximum {get; private set;} = Maximum;
		public Action<float> Action {get; private set;} = Action;
	}
}