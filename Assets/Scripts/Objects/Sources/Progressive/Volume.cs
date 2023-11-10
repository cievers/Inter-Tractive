using System.Collections.Generic;
using System.Linq;
using Geometry;
using Geometry.Generators;
using Geometry.Topology;
using Geometry.Tracts;
using Objects.Concurrent;
using UnityEngine;

namespace Objects.Sources.Progressive {
	public class Volume : Promise<Hull> {
		private UniformTractogram tractogram;
		private List<ConvexPolygon> cuts;
		
		private bool receivedTractogram;
		private bool receivedCuts;

		public Volume(UniformTractogram tractogram) {
			this.tractogram = tractogram;
			Start();
		}
		public Volume(Promise<UniformTractogram> promisedTractogram, Promise<List<ConvexPolygon>> promisedCuts) {
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
			var result = new List<Hull>();
			result.Add(Cap(cuts[0], tractogram.Slice(0), Side.Negative));
			for (var i = 1; i < cuts.Count; i++) {
				result.Add(new ConvexWrap(cuts[i-1], cuts[i]).Hull());
			}
			result.Add(Cap(cuts[^1], tractogram.Slice(^1), Side.Positive));
			Complete(Hull.Join(result));
		}
		private static Hull Cap(ConvexPolygon cut, IEnumerable<Vector3> points, Side side) {
			var hull = new ConvexPolyhedron(points.Where(point => cut.Side(point) == side).Concat(cut.Points).ToList()).Hull();
			
			var set = new HashSet<Vector3>(cut.Points);
			var result = new List<int>();
			for (var i = 0; i < hull.Indices.Length; i += 3) {
				if (!(
					set.Contains(hull.Vertices[hull.Indices[i]]) && 
					set.Contains(hull.Vertices[hull.Indices[i+1]]) && 
					set.Contains(hull.Vertices[hull.Indices[i+2]])
				)) {
					result.Add(i);
					result.Add(i+1);
					result.Add(i+2);
				}
			}
			return new Hull(hull.Vertices, hull.Normals, result.ToArray());
		}
	}
}