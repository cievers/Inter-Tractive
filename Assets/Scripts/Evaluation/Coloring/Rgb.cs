using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Evaluation.Coloring {
	public class Rgb : Coloring {
		private const byte COLORIZE_TRANSPARENCY = 200;

		protected byte ChannelDefault {get;}
		public override Tuple<int, int> Dimensions => new(1, 3);

		public Rgb(byte channelDefault=0) {
			ChannelDefault = channelDefault;
		}
		
		public override IEnumerable<Color32> Color(IEnumerable<Vector> measurements) {
			throw new NotImplementedException();
		}
		public override Dictionary<T, Color32> Color<T>(Dictionary<T, Vector> measurements) {
			var r = ReadNormalized(measurements, 0, ChannelDefault);
			var g = ReadNormalized(measurements, 1, ChannelDefault);
			var b = ReadNormalized(measurements, 2, ChannelDefault);
			return measurements.ToDictionary(pair => pair.Key, pair => new Color32(
				r[pair.Key], 
				g[pair.Key], 
				b[pair.Key], 
				COLORIZE_TRANSPARENCY
			));
		}
	}
}