using System;
using TMPro;
using UnityEngine;

namespace Interface.Control {
	public class Slider : MonoBehaviour {
		public TextMeshProUGUI displayName;
		public UnityEngine.UI.Slider slider;

		protected new string name;
		protected Action<float> action;

		public Slider Construct(Transform parent, Data.Slider slider) {
			var instance = Instantiate(gameObject, parent);
			var component = instance.GetComponent<Slider>();

			component.name = slider.Name;
			component.action = slider.Action;
			component.slider.minValue = slider.Minimum;
			component.slider.maxValue = slider.Maximum;
			component.slider.value = slider.Default;
			component.slider.onValueChanged.AddListener(component.UpdateValue);
			component.UpdateDisplay(slider.Default);
			
			return component;
		}

		protected void UpdateDisplay(float value) {
			displayName.text = name + ": "+ value;
		}
		protected void UpdateValue(float value) {
			UpdateDisplay(value);
			action?.Invoke(value);
		}
	}
}