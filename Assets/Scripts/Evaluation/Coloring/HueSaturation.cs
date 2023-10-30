using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Evaluation.Coloring {
	public class HueSaturation : Coloring {
		public override Tuple<int, int> Dimensions => new(1, 2);

		public override IEnumerable<Color32> Color(IEnumerable<Vector> measurements) {
			throw new NotImplementedException();
		}
		public override Dictionary<T, Color32> Color<T>(Dictionary<T, Vector> measurements) {
			var hue = Read(measurements, 0);
			var saturation = Read(measurements, 1, 1);
			var hueLimit = hue.Values.Max();
			var saturationLimit = saturation.Values.Max();
			return measurements.ToDictionary(pair => pair.Key, pair => (Color32) UnityEngine.Color.HSVToRGB(hue[pair.Key] / hueLimit, saturation[pair.Key] / saturationLimit, saturation[pair.Key] / saturationLimit));
		}
		private Color32 Color<T>(Dictionary<T, float> hue, Dictionary<T, float> saturation, KeyValuePair<T, Vector> pair) {
			return UnityEngine.Color.HSVToRGB(hue[pair.Key], saturation[pair.Key], 1);
		}
	}
}