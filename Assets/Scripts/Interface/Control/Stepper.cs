using System;
using TMPro;
using UnityEngine;

namespace Interface.Control {
	public class Stepper : MonoBehaviour {
		public TextMeshProUGUI displayName;

		private float value;
		private Data configuration;

		public Stepper Construct(Transform parent, Data stepper) {
			var instance = Instantiate(gameObject, parent);
			var component = instance.GetComponent<Stepper>();

			component.configuration = stepper;
			component.value = stepper.Default;
			component.UpdateDisplay(stepper.Default);
			
			return component;
		}

		public void Decrease() {
			value -= configuration.Interval;
			UpdateValue();
		}
		public void Increase() {
			value += configuration.Interval;
			UpdateValue();
		}

		protected void UpdateDisplay(float value) {
			displayName.text = configuration.Name + ": "+ value;
		}
		protected void UpdateValue() {
			UpdateDisplay(value);
			configuration.Action?.Invoke(value);
		}
		
		public record Data(string Name, float Interval, float Default, float Minimum, float Maximum, Action<float> Action) : Component {
			public string Name {get;} = Name;
			public float Interval {get;} = Interval;
			public float Default {get;} = Default;
			public float Minimum {get;} = Minimum;
			public float Maximum {get;} = Maximum;
			public Action<float> Action {get;} = Action;
		}
	}
}