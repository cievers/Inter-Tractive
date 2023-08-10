using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Evaluation.Coloring {
	public class TransparentRgb : Rgb {
		public override Tuple<int, int> Dimensions => new(2, 4);

		public TransparentRgb(byte channelDefault=0) : base(channelDefault) {}
		
		public override Dictionary<T, Color32> Color<T>(Dictionary<T, Vector> measurements) {
			var a = ReadChannel(measurements, 0);
			var r = ReadChannel(measurements, 1);
			var g = ReadChannel(measurements, 2);
			var b = ReadChannel(measurements, 3);
			return measurements.ToDictionary(pair => pair.Key, pair => new Color32(
				r[pair.Key], 
				g[pair.Key], 
				b[pair.Key],
				a[pair.Key]
			));
		}
	}
}