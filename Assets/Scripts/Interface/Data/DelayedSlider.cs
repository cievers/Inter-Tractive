using System;

namespace Interface.Data {
	public record DelayedSlider(string Name, float Default, float Minimum, float Maximum, float Delay, Action<float> Action) : Slider(Name, Default, Minimum, Maximum, Action) {
		public float Delay {get; private set;} = Delay;
	}
}