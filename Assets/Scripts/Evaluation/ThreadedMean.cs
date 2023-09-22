using System.Collections.Concurrent;
using System.Linq;
using Geometry.Tracts;
using UnityEngine;

namespace Evaluation {
	public class ThreadedMean {
		private readonly Tractogram tractogram;
		private readonly int samples;
		private readonly ConcurrentBag<Tract> mean;

		public ThreadedMean(Tractogram tractogram, int samples, ConcurrentBag<Tract> mean) {
			this.tractogram = tractogram;
			this.samples = samples;
			this.mean = mean;
		}
		
		public void Start() {
			mean.Add(new ArrayTract(
			tractogram
				.Sample(samples)
				.Slices()
				.Select(slice => slice as Vector3[] ?? slice.ToArray())
				.Select(array => array.Aggregate(Vector3.zero, (current, point) => current + point) / array.Length)
				.ToArray()
			));
		}
	}
}