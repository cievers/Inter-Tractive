using System;
using System.Collections.Generic;
using System.Linq;
using Evaluation.Coloring;
using Evaluation.Geometric;
using TMPro;
using UnityEngine;

namespace Interface.Control {
	public class Evaluation : MonoBehaviour {
		public DropdownStack dimensions;
		public TMP_Dropdown coloring;

		public Evaluation Construct(Transform parent, Data data) {
			var instance = Instantiate(this, parent);
			
			instance.coloring.options = data.colorings.Keys.Select(name => new TMP_Dropdown.OptionData(name)).ToList();
			instance.dimensions.options = data.measurements.Keys.Select(name => new TMP_Dropdown.OptionData(name)).ToList();
			instance.dimensions.Add();
			
			return instance;
		}

		// public TractEvaluation Evaluate() {
		// 	return new TractEvaluation(
		// 		new CompoundMetric(
		// 			dimensions
		// 				.Result<TMP_Dropdown>()
		// 				.Select(dropdown => measurements[dropdown.options[dropdown.value].text].Invoke())
		// 				.ToArray()
		// 		), 
		// 		colorings[coloring.options[coloring.value].text]
		// 	);
		// }

		public record Data : Controller {
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
		}
	}
}