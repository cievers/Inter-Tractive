using System.Collections.Generic;
using UnityEngine;

namespace Geometry.Tracts {
	public interface UniformTractogram : Tractogram {
		public int Samples {get;}
		
		public IEnumerable<Vector3> Slice(int index);
		public IEnumerable<IEnumerable<Vector3>> Slices() {
			var result = new IEnumerable<Vector3>[Samples];
			for (var i = 0; i < Samples; i++) {
				result[i] = Slice(i);
			}
			return result;
		}
	}
}