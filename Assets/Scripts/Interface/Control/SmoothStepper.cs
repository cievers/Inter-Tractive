using System;
using TMPro;
using UnityEngine;

namespace Interface.Control {
	public class SmoothStepper : MonoBehaviour {
		public TextMeshProUGUI displayName;
		public UnityEngine.UI.Slider slider;

		private Stepper.Data configuration;

		public SmoothStepper Construct(Transform parent, Stepper.Data stepper) {
			var instance = Instantiate(gameObject, parent);
			var component = instance.GetComponent<SmoothStepper>();

			component.configuration = stepper;

			component.name = stepper.Name;
			component.slider.minValue = stepper.Minimum;
			component.slider.maxValue = stepper.Maximum;
			component.slider.value = stepper.Default;
			component.slider.onValueChanged.AddListener(component.UpdateSlider);
			component.UpdateDisplay(stepper.Default);
			
			return component;
		}

		protected void UpdateDisplay(float value) {
			displayName.text = configuration.Name + ": "+ value;
		}
		protected void UpdateSlider(float value) {
			UpdateValue((float) (Math.Round(value / configuration.Interval) * configuration.Interval));
		}
		protected void UpdateValue(float value) {
			UpdateDisplay(value);
			configuration.Action?.Invoke(value);
		}
	}
}