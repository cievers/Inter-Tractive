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
	public class DimensionalEvaluation : MonoBehaviour {
		public DropdownStack dimensions;
		public TMP_Dropdown coloring;
		public GameObject error;

		private Evaluation.Data data;

		public DimensionalEvaluation Construct(RectTransform parent, Evaluation.Data data) {
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
	}
}