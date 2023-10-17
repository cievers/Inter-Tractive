using System;
using System.Collections.Generic;
using System.Linq;
using Evaluation;
using Evaluation.Coloring;
using Evaluation.Coloring.Gradients;
using Evaluation.Geometric;
using TMPro;
using UnityEngine;

namespace Interface.Control {
	public class Evaluation : MonoBehaviour {
		public DropdownStack dimensions;
		public TMP_Dropdown coloring;
		public GameObject error;

		public Data data;

		public Evaluation Construct(RectTransform parent, Data data) {
			var instance = Instantiate(this, parent);

			instance.data = data;
			instance.coloring.options = data.colorings.Keys.Select(name => new TMP_Dropdown.OptionData(name)).ToList();
			instance.dimensions.options = data.measurements.Keys.Select(name => new TMP_Dropdown.OptionData(name)).ToList();
			
			instance.coloring.onValueChanged.AddListener(_ => instance.Invoke());
			instance.dimensions.Configured += _ => instance.Invoke();
			instance.dimensions.forceUpdates.Add(parent);
			
			instance.dimensions.Add();
			
			return instance;
		}

		private void Invoke() {
			try {
				data.update.Invoke(Evaluate());
				error.SetActive(false);
			} catch (ArgumentOutOfRangeException) {
				error.SetActive(true);
			}
		}
		private TractEvaluation Evaluate() {
			return new TractEvaluation(
				new CompoundMetric(
					dimensions
						.Values
						.Select(name => data.measurements[name].Invoke())
						.ToArray()
				), 
				data.colorings[coloring.options[coloring.value].text]
			);
		}

		public record Data : Component {
			public readonly Action<TractEvaluation> update;
			public readonly Dictionary<string, Coloring> colorings = new() {
				{"Grayscale", new Grayscale()},
				{"Temperature", new Temperature()},
				{"Plasma", new Plasma()},
				{"Viridis", new Viridis()},
				{"Hue", new HueSaturation()},
				// {"RGB", new Rgb()}
			};
			public readonly Dictionary<string, Func<TractMetric>> measurements = new() {
				{"Density", () => new Density()}, 
				{"Length", () => new Length()}, 
				{"Span", () => new Span()},
				{"Curl", () => new Curl()},
				{"Roughness", () => new Roughness()}
			};

			public Data(Action<TractEvaluation> update) {
				this.update = update;
			}
		}
	}
}