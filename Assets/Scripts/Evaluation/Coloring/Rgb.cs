using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Evaluation.Coloring {
	public class Rgb : Coloring {
		private const byte COLORIZE_TRANSPARENCY = 200;
		
		private byte ChannelDefault {get;}
		public override Tuple<int, int> Dimensions => new(1, 3);

		public Rgb(byte channelDefault=0) {
			ChannelDefault = channelDefault;
		}
		
		public override IEnumerable<Color32> Color(IEnumerable<Vector> measurements) {
			throw new NotImplementedException();
		}
		public override Dictionary<T, Color32> Color<T>(Dictionary<T, Vector> measurements) {
			var r = ReadChannel(measurements, 0);
			var g = ReadChannel(measurements, 1);
			var b = ReadChannel(measurements, 2);
			return measurements.ToDictionary(pair => pair.Key, pair => new Color32(
				r[pair.Key], 
				g[pair.Key], 
				b[pair.Key], 
				COLORIZE_TRANSPARENCY
			));
		}
		protected Dictionary<T, byte> ReadChannel<T>(Dictionary<T, Vector> measurements, int channel) {
			try {
				return ReadNormalized(measurements, channel);
			} catch {
				return measurements.ToDictionary(pair => pair.Key, _ => ChannelDefault);
			}
		}
	}
}