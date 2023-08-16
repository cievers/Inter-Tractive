using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Evaluation.Coloring {
	public class TransparentRgb : Rgb {
		public override Tuple<int, int> Dimensions => new(2, 4);

		public TransparentRgb(byte channelDefault=0) : base(channelDefault) {}
		
		public override Dictionary<T, Color32> Color<T>(Dictionary<T, Vector> measurements) {
			var a = ReadNormalized(measurements, 0, ChannelDefault);
			var r = ReadNormalized(measurements, 1, ChannelDefault);
			var g = ReadNormalized(measurements, 2, ChannelDefault);
			var b = ReadNormalized(measurements, 3, ChannelDefault);
			return measurements.ToDictionary(pair => pair.Key, pair => new Color32(
				r[pair.Key], 
				g[pair.Key], 
				b[pair.Key],
				a[pair.Key]
			));
		}
	}
}