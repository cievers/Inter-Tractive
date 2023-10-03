using System;
using System.Collections.Concurrent;
using System.Linq;
using Geometry.Tracts;
using UnityEngine;

namespace Evaluation {
	public class ThreadedMean {
		private readonly UniformTractogram tractogram;
		private readonly ConcurrentBag<Tuple<UniformTractogram, Tract>> mean;

		public ThreadedMean(UniformTractogram tractogram, ConcurrentBag<Tuple<UniformTractogram, Tract>> mean) {
			this.tractogram = tractogram;
			this.mean = mean;
		}
		
		public void Start() {
			mean.Add(new Tuple<UniformTractogram, Tract>(tractogram, new ArrayTract(
				tractogram
					.Slices()
					.Select(slice => slice as Vector3[] ?? slice.ToArray())
					.Select(array => array.Aggregate(Vector3.zero, (current, point) => current + point) / array.Length)
					.ToArray()
			)));
		}
	}
}