using System;
using System.Globalization;
using UnityEngine;

namespace Interface.Control {
	public class TransformedSlider : Slider {
		private Func<float, float> transformation;
		private Func<float, float, string> representation;

		public TransformedSlider Construct(Transform parent, Data slider) {
			var instance = Instantiate(this, parent);

			instance.name = slider.Name;
			instance.action = slider.Action;
			instance.transformation = slider.Transformation;
			instance.representation = slider.Representation;
			instance.slider.minValue = slider.Minimum;
			instance.slider.maxValue = slider.Maximum;
			instance.slider.value = slider.Default;
			instance.slider.onValueChanged.AddListener(instance.UpdateValue);
			instance.UpdateDisplay(slider.Default);

			return instance;
		}
		
		private new void UpdateValue(float value) {
			var transformed = transformation.Invoke(value);
			UpdateDisplay(value, transformed);
			action?.Invoke(transformed);
		}
		private new void UpdateDisplay(float literal) {
			UpdateDisplay(literal, transformation.Invoke(literal));
		}
		private void UpdateDisplay(float literal, float transformed) {
			displayName.text = name + ": "+ representation.Invoke(literal, transformed);
		}

		public new record Data(string Name, float Default, Func<float, float> Transformation, Func<float, float, string> Representation, Action<float> Action) : Slider.Data(Name, Default, 0, 1, Action) {
			public Func<float, float> Transformation {get;} = Transformation;
			public Func<float, float, string> Representation {get;} = Representation;

			public Data(string name, float @default, Func<float, float> transformation, Action<float> action) : this(name, @default, transformation, (_, transformed) => transformed.ToString(CultureInfo.InvariantCulture), action) {}
		}
		public record Exponential : Data {
			public Exponential(string name, int root, float @default, float minimum, float maximum, Action<float> action) : base(
				name, 
				(@default - minimum) / (maximum - minimum), 
				value => (float) Math.Pow(root, value * (maximum - minimum) + minimum), 
				action
			) {}
			public Exponential(string name, int root, float @default, float minimum, float maximum, Func<float, float, string> representation, Action<float> action) : base(
				name, 
				(@default - minimum) / (maximum - minimum), 
				value => (float) Math.Pow(root, value * (maximum - minimum) + minimum), 
				representation,
				action
			) {}
		}
	}
}