using System;

namespace Interface.Data {
	public record Slider(string Name, float Default, float Minimum, float Maximum, Action<float> Action) : Configuration {
		public string Name {get; private set;} = Name;
		public float Default {get; private set;} = Default;
		public float Minimum {get; private set;} = Minimum;
		public float Maximum {get; private set;} = Maximum;
		public Action<float> Action {get; private set;} = Action;
	}
}