using System;
using System.Collections.Generic;
using System.Linq;
using Evaluation;
using Evaluation.Coloring;
using Evaluation.Coloring.Gradients;
using Evaluation.Geometric;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace Interface.Control {
	public class Evaluation : MonoBehaviour {
		public TMP_Dropdown measurement;
		public TMP_Dropdown coloring;

		private Data data;

		public Evaluation Construct(RectTransform parent, Data data) {
			var instance = Instantiate(this, parent);

			instance.data = data;
			instance.measurement.options = data.measurements.Keys.Select(name => new TMP_Dropdown.OptionData(name)).ToList();
			instance.coloring.options = data.colorings.Keys.Select(name => new TMP_Dropdown.OptionData(name)).ToList();
			
			instance.measurement.onValueChanged.AddListener(_ => instance.Invoke());
			instance.coloring.onValueChanged.AddListener(_ => instance.Invoke());
			
			return instance;
		}

		private void Invoke() {
			data.update.Invoke(Evaluate());
		}
		private TractEvaluation Evaluate() {
			return new TractEvaluation(
				data.measurements[measurement.options[measurement.value].text].Invoke(),
				data.colorings[coloring.options[coloring.value].text]
			);
		}

		public record Data : Component {
			public readonly Action<TractEvaluation> update;
			public readonly Dictionary<string, Func<TractMetric>> measurements = new() {
				{"Length", () => new Length()}, 
				{"Span", () => new Span()},
				{"Curl", () => new Curl()},
				{"Roughness", () => new Roughness()},
				{"Density", () => new Density()}
			};
			public readonly Dictionary<string, Coloring> colorings = new() {
				{"Grayscale", new Grayscale()},
				{"Viridis", new Viridis()},
				{"Plasma", new Plasma()},
				{"Temperature", new Temperature()},
				{"Hue", new HueSaturation()},
				// {"RGB", new Rgb()}
			};

			public Data(Action<TractEvaluation> update) {
				this.update = update;
			}
		}
	}
}