using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Evaluation.Coloring {
	public class Rgb : Coloring {
		private const byte COLORIZE_TRANSPARENCY = 200;
		
		private byte ChannelDefault {get;}
		public Tuple<int, int> Dimensions => new(1, 3);

		public Rgb(byte channelDefault=0) {
			ChannelDefault = channelDefault;
		}
		
		public IEnumerable<Color32> Color(IEnumerable<Vector> measurements) {
			throw new NotImplementedException();
		}
		public Dictionary<T, Color32> Color<T>(Dictionary<T, Vector> measurements) {
			var r = ColorChannel(measurements, 0);
			var g = ColorChannel(measurements, 1);
			var b = ColorChannel(measurements, 2);
			return measurements.ToDictionary(pair => pair.Key, pair => new Color32(
				r[pair.Key], 
				g[pair.Key], 
				b[pair.Key], 
				COLORIZE_TRANSPARENCY
			));
		}
		private Dictionary<T, byte> ColorChannel<T>(Dictionary<T, Vector> measurements, int channel) {
			try {
				var floats = measurements.ToDictionary(measurement => measurement.Key, measurement => measurement.Value[channel]);
				var limit = floats.Values.Max();
				return floats.ToDictionary(pair => pair.Key, pair => (byte) (pair.Value / limit * 255));
			} catch {
				return measurements.ToDictionary(pair => pair.Key, _ => ChannelDefault);
			}
		}
	}
}