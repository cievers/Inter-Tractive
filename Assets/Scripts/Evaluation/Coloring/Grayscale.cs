using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Evaluation.Coloring {
	public class Grayscale : Coloring {
		private const byte COLORIZE_TRANSPARENCY = 200;
		
		public Tuple<int, int> Dimensions => new(1, 1);
		
		public IEnumerable<Color32> Color(IEnumerable<Vector> measurements) {
			throw new NotImplementedException();
		}
		public Dictionary<T, Color32> Color<T>(Dictionary<T, Vector> measurements) {
			var floats = measurements.ToDictionary(measurement => measurement.Key, measurement => measurement.Value[0]);
			var limit = floats.Values.Max();
			return floats
				.ToDictionary(pair => pair.Key, pair => (byte) (pair.Value / limit * 255))
				.ToDictionary(pair => pair.Key, pair => new Color32(pair.Value, pair.Value, pair.Value, COLORIZE_TRANSPARENCY));
		}
	}
}