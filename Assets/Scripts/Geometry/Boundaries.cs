using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Geometry {
	public struct Boundaries {
		public Vector3 Min {get;}
		public Vector3 Max {get;}
		public Vector3 Center => Min + Size / 2;
		public Vector3 Size => Max - Min;

		public Boundaries(Vector3 min, Vector3 max) {
			Min = min;
			Max = max;
		}
		
		public static Boundaries Join(params Boundaries[] geometry) => Join((IEnumerable<Boundaries>)geometry);
		public static Boundaries Join(IEnumerable<Boundaries> boundaries) {
			var enumerable = boundaries as Boundaries[] ?? boundaries.ToArray();
			return new Boundaries(new Vector3(
				enumerable.Select(box => box.Min.x).Min(),
				enumerable.Select(box => box.Min.y).Min(),
				enumerable.Select(box => box.Min.z).Min()
			), new Vector3(
				enumerable.Select(box => box.Max.x).Max(),
				enumerable.Select(box => box.Max.y).Max(),
				enumerable.Select(box => box.Max.z).Max()
			));
		}
	}
}