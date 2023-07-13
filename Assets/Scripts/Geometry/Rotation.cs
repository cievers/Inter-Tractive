using System;
using System.Globalization;
using UnityEngine;

namespace Geometry {
	public readonly struct Rotation {
		private readonly float rotation;
		
		public float Value => rotation;

		public Rotation(float rotation, Rotations mode = Rotations.One) {
			this.rotation = (rotation / mode.Scale() + ((int) (rotation / mode.Scale()) + 1)) % 1;
		}

		public Rotation Reflect(Rotation reflector) {
			return new Rotation(1 - (rotation - reflector) + reflector);
		}
		public Rotation Interpolate(Rotation other, float t) {
			// Unity's cyclic lerp returns values below the minimum of a and above the maximum of b
			// 'Luckily' the Rotation constructor handles this and normalizes it
			return new Rotation(Mathf.LerpAngle(ToFloat(Rotations.Degrees), other.ToFloat(Rotations.Degrees), t), Rotations.Degrees);
		}
		public Rotation Clamp(Rotation minimum, Rotation maximum) {
			if (!Bounded(minimum, maximum)) {
				return Math.Abs(this - minimum) <= Math.Abs(this - maximum) ? minimum : maximum;
			}
			return this;
		}
		public bool Bounded(Rotation minimum, Rotation maximum) {
			if (minimum <= maximum) {
				// The intuitive case, where dealing with the cyclic boundary is not needed
				return minimum <= this && this <= maximum;
			}
			// The stranger case, where the cyclic boundary interferes with a simple boundary check
			// TODO: Not sure if this works for boundaries that include both 0 and 0.5
			return this <= 0.5f ? maximum >= this : this >= minimum;
		}

		public override string ToString() {
			return ToFloat().ToString(CultureInfo.InvariantCulture);
		}
		public float ToFloat(Rotations mode = Rotations.One) {
			return rotation * mode.Scale();
		}
		
		public static implicit operator float(Rotation rotation) => rotation.rotation;
		public static implicit operator Rotation(float rotation) => new Rotation(rotation);
		// public static explicit operator Rotation(float rotation) => new Rotation(rotation);
	}
}