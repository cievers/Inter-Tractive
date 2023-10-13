using System.Linq;
using Geometry.Tracts;
using Objects.Concurrent;
using UnityEngine;

namespace Objects.Sources.Progressive {
	public class Core : Promise<Tract> {
		private UniformTractogram tractogram;

		public Core(UniformTractogram tractogram) {
			this.tractogram = tractogram;
			Start();
		}
		public Core(Promise<UniformTractogram> promise) {
			promise.Request(tractogram => {
				this.tractogram = tractogram;
				Start();
			});
		}
		
		protected override void Compute() {
			Complete(new ArrayTract(
				tractogram
					.Slices()
					.Select(slice => slice as Vector3[] ?? slice.ToArray())
					.Select(array => array.Aggregate(Vector3.zero, (current, point) => current + point) / array.Length)
					.ToArray()
			));
		}
	}
}