using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Evaluation.Coloring {
	public class TransparentGradient : Gradient {
		public override Tuple<int, int> Dimensions => new(2, 2);

		public TransparentGradient(ICollection<Color> colors) : base(colors) {}
		
		public override Dictionary<T, Color32> Color<T>(Dictionary<T, Vector> measurements) {
			var alphas = ReadNormalized(measurements, 0);
			var floats = Read(measurements, 1);
			var limit = floats.Values.Max();
			return floats
				.ToDictionary(pair => pair.Key, pair => gradient.Evaluate(pair.Value / limit))
				.ToDictionary(pair => pair.Key, pair => new Color32(
				(byte) (pair.Value.r * 255), 
				(byte) (pair.Value.r * 255), 
				(byte) (pair.Value.r * 255),
				alphas[pair.Key]
			));
		}
	}
}