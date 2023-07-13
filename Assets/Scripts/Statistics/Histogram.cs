using System.Collections.Generic;
using System.Linq;

namespace Statistics {
	public class Histogram<T> {
		private readonly Dictionary<T, int> frequencies;

		public Histogram(IEnumerable<T> entries) {
			frequencies = new Dictionary<T, int>();
			foreach (var entry in entries) {
				frequencies.TryAdd(entry, 0);
				frequencies[entry] += 1;
			}
		}

		public int Get(T entry) {
			return frequencies.TryGetValue(entry, out var frequency) ? frequency : 0;
		}

		public IEnumerable<string> Log() {
			return frequencies.Select(pair => pair.Key + ": " + pair.Value).OrderBy(line => line);
		}
	}
}