using UnityEngine;

namespace Geometry {
	public enum Rotations {
		One,
		Degrees,
		Radians,
		Euler
	}

	public static class RotationExtensions {
		public static float Scale(this Rotations scale) {
			return scale switch {
				Rotations.Euler => -360f,
				Rotations.Degrees => 360f,
				Rotations.Radians => Mathf.PI * 2,
				_ => 1
			};
		}
	}
}