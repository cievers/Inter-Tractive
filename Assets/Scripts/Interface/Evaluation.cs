using System;
using System.Collections.Generic;
using System.Linq;
using Evaluation;
using Evaluation.Coloring;
using Evaluation.Geometric;
using Interface.Construction;
using Interface.Control;
using TMPro;
using UnityEngine;

namespace Interface {
	public class Evaluation : MonoBehaviour {
		public DropdownStack dimensions;
		public TMP_Dropdown coloring;

		private readonly Dictionary<string, Coloring> colorings = new() {
			{"Grayscale", new Grayscale()},
			{"Transparent grayscale", new TransparentGrayscale()},
			{"RGB", new Rgb()}
		};
		private readonly Dictionary<string, Func<TractMetric>> measurements = new() {
			{"Density", () => new Density()}, 
			{"Length", () => new Length()}, 
			{"Span", () => new Span()},
			{"Curl", () => new Curl()}
		};

		public void Start() {
			coloring.options = colorings.Keys.Select(name => new TMP_Dropdown.OptionData(name)).ToList();
			dimensions.options = measurements.Keys.Select(name => new TMP_Dropdown.OptionData(name)).ToList();
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
	}
}