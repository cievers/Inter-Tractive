using System;
using UnityEngine;

namespace Interface.Control {
	public class DelayedSlider : Slider {
		private float delay;
		private float delayed;
		private float waited;
		private bool waiting;

		public DelayedSlider Construct(Transform parent, Data slider) {
			var instance = Instantiate(gameObject, parent);
			var component = instance.GetComponent<DelayedSlider>();

			component.name = slider.Name;
			component.action = slider.Action;
			component.delay = slider.Delay;
			component.slider.minValue = slider.Minimum;
			component.slider.maxValue = slider.Maximum;
			component.slider.value = slider.Default;
			component.slider.onValueChanged.AddListener(component.UpdateWaiting);
			component.UpdateDisplay(slider.Default);
			
			return component;
		}

		private void Update() {
			if (waiting) {
				waited += Time.deltaTime;
				if (waited >= delay) {
					waiting = false;
					UpdateValue(delayed);
				}
			}
		}
		private void UpdateWaiting(float value) {
			UpdateDisplay(value);
			delayed = value;
			waited = 0;
			waiting = true;
		}
		
		public new record Data(string Name, float Default, float Minimum, float Maximum, float Delay, Action<float> Action) : Slider.Data(Name, Default, Minimum, Maximum, Action) {
			public float Delay {get; private set;} = Delay;
		}
	}
}