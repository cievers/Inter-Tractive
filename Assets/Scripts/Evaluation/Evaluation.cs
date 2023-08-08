using System;
using System.Collections.Generic;
using Evaluation.Geometric;
using Geometry.Tracts;
using Maps.Cells;
using UnityEngine;

namespace Evaluation {
	public class Evaluation {
		public TractMetric Metric {get;}
		public Coloring.Coloring Coloring {get;}

		public Evaluation(TractMetric metric, Coloring.Coloring coloring) {
			if (coloring.Dimensions.Item1 <= metric.Dimensions && metric.Dimensions <= coloring.Dimensions.Item2) {
				Metric = metric;
				Coloring = coloring;
			} else {
				throw new ArgumentOutOfRangeException("Incompatible number of dimensions from metric "+metric+" to coloration "+coloring);
			}
		}

		public Vector Measure(IEnumerable<Tract> tracts) {
			return Metric.Measure(tracts);
		}
		public Dictionary<Cell, Vector> Measure(Dictionary<Cell, IEnumerable<Tract>> tractogram) {
			return Metric.Measure(tractogram);
		}
		public IEnumerable<Color32> Color(IEnumerable<Vector> measurements) {
			return Coloring.Color(measurements);
		}
		public Dictionary<T, Color32> Color<T>(Dictionary<T, Vector> measurements) {
			return Coloring.Color(measurements);
		}
		public Dictionary<Cell, Color32> Evaluate(Dictionary<Cell, IEnumerable<Tract>> tractogram) {
			return Color(Measure(tractogram));
		}
	}
}