﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Evaluation.Coloring {
	public class Gradient : Coloring {
		protected readonly UnityEngine.Gradient gradient;
		public override Tuple<int, int> Dimensions => new(1, 1);

		public Gradient(ICollection<Color> colors) {
			var count = colors.Count;
			gradient = new UnityEngine.Gradient();
			gradient.SetKeys(
				colors.Select((color, i) => new GradientColorKey(color, (float) i / (count - 1))).ToArray(), 
				colors.Select((color, i) => new GradientAlphaKey(color.a, (float) i / (count - 1))).ToArray()
			);
		}
		
		public override IEnumerable<Color32> Color(IEnumerable<Vector> measurements) {
			throw new NotImplementedException();
		}
		public override Dictionary<T, Color32> Color<T>(Dictionary<T, Vector> measurements) {
			var floats = Read(measurements, 0);
			var limit = floats.Values.Max();
			return floats
				.ToDictionary(pair => pair.Key, pair => gradient.Evaluate(pair.Value / limit))
				.ToDictionary(pair => pair.Key, pair => new Color32((byte) (pair.Value.r * 255), (byte) (pair.Value.g * 255), (byte) (pair.Value.b * 255), (byte) (pair.Value.a * 255)));
		}
	}
}