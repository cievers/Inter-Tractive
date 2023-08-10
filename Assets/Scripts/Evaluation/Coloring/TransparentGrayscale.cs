using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Evaluation.Coloring {
	public class TransparentGrayscale : Grayscale {
		public override Tuple<int, int> Dimensions => new(2, 2);
		
		public override Dictionary<T, Color32> Color<T>(Dictionary<T, Vector> measurements) {
			var alphas = ReadNormalized(measurements, 0);
			var values = ReadNormalized(measurements, 1);
			return measurements.ToDictionary(pair => pair.Key, pair => new Color32(
				values[pair.Key], 
				values[pair.Key], 
				values[pair.Key],
				alphas[pair.Key]
			));
		}
	}
}