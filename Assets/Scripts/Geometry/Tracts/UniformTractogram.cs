using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Geometry.Tracts {
	public class UniformTractogram : SimpleTractogram {
		public int Samples => Tracts.Min(tract => tract.Points.Length);

		public UniformTractogram(IEnumerable<Tract> lines) : base(lines) {}
		public IEnumerable<Vector3> Slice(int index) {
			return Tracts.Select(tract => tract.Points[index]);
		}
		public IEnumerable<IEnumerable<Vector3>> Slices() {
			var result = new IEnumerable<Vector3>[Samples];
			for (var i = 0; i < Samples; i++) {
				result[i] = Slice(i);
			}
			return result;
		}
	}
}