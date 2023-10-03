using System.Collections.Concurrent;
using System.Linq;
using Geometry;
using Geometry.Generators;
using Geometry.Tracts;
using UnityEngine;

namespace Evaluation {
	public class ThreadedVolume {
		private readonly UniformTractogram tractogram;
		private readonly ConcurrentBag<Hull> hull;

		public ThreadedVolume(UniformTractogram tractogram, ConcurrentBag<Hull> hull) {
			this.tractogram = tractogram;
			this.hull = hull;
		}
		
		public void Start() {
			hull.Add(new ConvexPolyhedron(tractogram.Slice(0).ToList()).Hull());
		}
	}
}