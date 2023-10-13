using UnityEngine;

namespace Geometry {
	public struct Line {
		public Vector3 Origin {get;}
		public Vector3 Direction {get;}

		public Line(Vector3 origin, Vector3 direction) {
			Origin = origin;
			Direction = direction;
		}
	}
}