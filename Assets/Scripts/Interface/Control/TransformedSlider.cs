using System;
using UnityEngine;

namespace Interface.Control {
	public class TransformedSlider : Slider {
		private Func<float, float> transformation;

		public TransformedSlider Construct(Transform parent, Data slider) {
			var instance = Instantiate(this, parent);

			instance.name = slider.Name;
			instance.action = slider.Action;
			instance.transformation = slider.Transformation;
			instance.slider.minValue = slider.Minimum;
			instance.slider.maxValue = slider.Maximum;
			instance.slider.value = slider.Default;
			instance.slider.onValueChanged.AddListener(instance.UpdateValue);
			instance.UpdateDisplay(slider.Default);

			return instance;
		}
		
		private new void UpdateValue(float value) {
			var transformed = transformation.Invoke(value);
			UpdateDisplay(transformed);
			action?.Invoke(transformed);
		}

		public new record Data(string Name, float Default, Func<float, float> Transformation, Action<float> Action) : Slider.Data(Name, Default, 0, 1, Action) {
			public Func<float, float> Transformation {get;} = Transformation;
		}
		public record Exponential : Data {
			public Exponential(string name, int root, float @default, float minimum, float maximum, Action<float> action) : base(
				name, 
				(@default - minimum) / (maximum - minimum), 
				value => (float) Math.Pow(root, value * (maximum - minimum) + minimum), 
				action
			) {}
		}
	}
}