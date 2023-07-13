using UnityEngine;

namespace Camera {
	public struct Focus {
		public Vector3 Origin {get; private set;}
		public float Distance {get; private set;}

		public Focus(Vector3 origin, float distance) {
			Origin = origin;
			Distance = distance;
		}
	}
}