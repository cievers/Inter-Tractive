using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Evaluation.Coloring {
	public class Grayscale : Coloring {
		private const byte COLORIZE_TRANSPARENCY = 200;
		
		public override Tuple<int, int> Dimensions => new(1, 1);
		
		public override IEnumerable<Color32> Color(IEnumerable<Vector> measurements) {
			throw new NotImplementedException();
		}
		public override Dictionary<T, Color32> Color<T>(Dictionary<T, Vector> measurements) {
			return ReadNormalized(measurements, 0).ToDictionary(pair => pair.Key, pair => new Color32(pair.Value, pair.Value, pair.Value, COLORIZE_TRANSPARENCY));
		}
	}
}