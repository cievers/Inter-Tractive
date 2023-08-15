using System;
using System.Collections.Generic;
using System.Linq;
using Evaluation;
using Evaluation.Coloring;
using Evaluation.Geometric;
using TMPro;
using UnityEngine;

namespace Interface.Control {
	public class Evaluation : MonoBehaviour {
		public DropdownStack dimensions;
		public TMP_Dropdown coloring;

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
			data.update.Invoke(Evaluate());
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

		public record Data : Controller {
			public readonly Action<TractEvaluation> update;
			public readonly Dictionary<string, Coloring> colorings = new() {
				{"Grayscale", new Grayscale()},
				{"Transparent grayscale", new TransparentGrayscale()},
				{"RGB", new Rgb()}
			};
			public readonly Dictionary<string, Func<TractMetric>> measurements = new() {
				{"Density", () => new Density()}, 
				{"Length", () => new Length()}, 
				{"Span", () => new Span()},
				{"Curl", () => new Curl()}
			};

			public Data(Action<TractEvaluation> update) {
				this.update = update;
			}
		}
	}
}