using System;
using System.Collections.Generic;
using System.Linq;
using Geometry;
using Geometry.Generators;
using Objects.Concurrent;
using UnityEngine;

namespace Objects.Sources.Progressive {
	public class Bottleneck : Promise<Tuple<Vector3, Vector3, Walk>> {
		private List<ConvexPolygon> cuts;

		public Bottleneck(Promise<List<ConvexPolygon>> promisedCuts) {
			promisedCuts.Request(cuts => {
				this.cuts = cuts;
				Start();
			});
		}

		protected override void Compute() {
			var (cut, segment) = cuts
				.ToDictionary(cut => cut, cut => cut.Skewer)
				.Aggregate((a, b) => a.Value.Size.magnitude < b.Value.Size.magnitude ? a : b);

			var center = segment.A + segment.Size * 0.5f;
			var radius = segment.Size.magnitude * 0.5f;
				
			Complete(new Tuple<Vector3, Vector3, Walk>(center, cut.Normal, Circle.Walk(Vector3.zero, Vector3.right * radius, Vector3.up * radius, 32)));
		}
	}
}