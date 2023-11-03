using System;
using System.Collections.Generic;
using System.Linq;
using Geometry;
using Geometry.Generators;
using Geometry.Tracts;
using Objects.Collection;
using Objects.Concurrent;
using UnityEngine;

namespace Objects.Sources.Progressive {
	public class Endpoints : Promise<Pair<Tuple<Vector3, Walk>>> {
		private UniformTractogram tractogram;
		private List<ConvexPolygon> cuts;
		
		private bool receivedTractogram;
		private bool receivedCuts;

		public Endpoints(UniformTractogram tractogram) {
			this.tractogram = tractogram;
			Start();
		}
		public Endpoints(Promise<UniformTractogram> promisedTractogram, Promise<List<ConvexPolygon>> promisedCuts) {
			promisedTractogram.Request(tractogram => {
				this.tractogram = tractogram;
				receivedTractogram = true;
				Update();
			});
			promisedCuts.Request(cuts => {
				this.cuts = cuts;
				receivedCuts = true;
				Update();
			});
		}
		
		private void Update() {
			if (receivedTractogram && receivedCuts) {
				Start();
			}
		}
		
		protected override void Compute() {
			var result = new List<Tuple<Vector3, Walk>>();
			// foreach (var shape in new[] {Cap(cuts[0], tractogram.Slice(0), Side.Negative), Cap(cuts[^1], tractogram.Slice(^1), Side.Positive)}) {
			foreach (var shape in new[] {cuts[0], cuts[^1]}) {
				var segment = shape.Skewer;
				var center = segment.A + segment.Size * 0.5f;
				var radius = segment.Size.magnitude * 0.5f;
				
				result.Add(new Tuple<Vector3, Walk>(center, Circle.Walk(Vector3.zero, Vector3.forward * radius, Vector3.up * radius, 32)));
			}
			Complete(new Pair<Tuple<Vector3, Walk>>(result));
		}
		private static ConvexPolyhedron Cap(ConvexPolygon cut, IEnumerable<Vector3> points, Side side) {
			return new ConvexPolyhedron(points.Where(point => cut.Side(point) == side).Concat(cut.Points).ToList());
		}
	}
}