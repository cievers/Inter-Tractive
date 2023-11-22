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
	public class Endpoints : Promise<Pair<Tuple<Vector3, Vector3, Walk>>> {
		private List<ConvexPolygon> cuts;
		
		public Endpoints(Promise<List<ConvexPolygon>> promisedCuts) {
			promisedCuts.Request(cuts => {
				this.cuts = cuts;
				Start();
			});
		}
		
		protected override void Compute() {
			var result = new List<Tuple<Vector3, Vector3, Walk>>();
			// foreach (var shape in new[] {Cap(cuts[0], tractogram.Slice(0), Side.Negative), Cap(cuts[^1], tractogram.Slice(^1), Side.Positive)}) {
			foreach (var shape in new[] {cuts[0], cuts[^1]}) {
				var segment = shape.Skewer;
				var center = segment.A + segment.Size * 0.5f;
				var radius = segment.Size.magnitude * 0.5f;
				
				result.Add(new Tuple<Vector3, Vector3, Walk>(center, shape.Normal, Circle.Walk(Vector3.zero, Vector3.forward * radius, Vector3.up * radius, 32)));
			}
			Complete(new Pair<Tuple<Vector3, Vector3, Walk>>(result));
		}
		private static ConvexPolyhedron Cap(ConvexPolygon cut, IEnumerable<Vector3> points, Side side) {
			return new ConvexPolyhedron(points.Where(point => cut.Side(point) == side).Concat(cut.Points).ToList());
		}
	}
}