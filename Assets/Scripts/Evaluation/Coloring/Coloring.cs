using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Evaluation.Coloring {
	public abstract class Coloring {
		public abstract Tuple<int, int> Dimensions {get;}

		public abstract IEnumerable<Color32> Color(IEnumerable<Vector> measurements);
		public abstract Dictionary<T,Color32> Color<T>(Dictionary<T, Vector> measurements);
		
		protected virtual Dictionary<T, float> Read<T>(Dictionary<T, Vector> measurements, int dimension) {
			return measurements.ToDictionary(measurement => measurement.Key, measurement => measurement.Value[dimension]);
		}
		protected virtual Dictionary<T, byte> ReadNormalized<T>(Dictionary<T, Vector> measurements, int dimension) {
			var floats = Read(measurements, dimension);
			var limit = floats.Values.Max();
			return floats.ToDictionary(pair => pair.Key, pair => (byte) (pair.Value / limit * 255));
		}
	}
}