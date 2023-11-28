using System;
using UnityEngine;

namespace Geometry {
	public enum Axis {
		X, Y, Z
	}
	
	public static class AxisExtensions {
		public static Axis Previous(this Axis axis) {
			return axis switch {
				Axis.X => Axis.Z,
				Axis.Y => Axis.X,
				Axis.Z => Axis.Y,
				_ => throw new ArgumentOutOfRangeException(nameof(axis), axis, "There is no fourth dimension here")
			};
		}
		public static Axis Next(this Axis axis) {
			return axis switch {
				Axis.X => Axis.Y,
				Axis.Y => Axis.Z,
				Axis.Z => Axis.X,
				_ => throw new ArgumentOutOfRangeException(nameof(axis), axis, "There is no fourth dimension here")
			};
		}
		public static float Select(this Axis axis, Vector3 vector) {
			return axis switch {
				Axis.X => vector.x,
				Axis.Y => vector.y,
				Axis.Z => vector.z,
				_ => throw new ArgumentOutOfRangeException(nameof(axis), axis, "There is no fourth dimension here")
			};
		}
		public static int Select(this Axis axis, Index3 index) {
			return axis switch {
				Axis.X => index.x,
				Axis.Y => index.y,
				Axis.Z => index.z,
				_ => throw new ArgumentOutOfRangeException(nameof(axis), axis, "There is no fourth dimension here")
			};
		}
		public static Vector3 Interpolate(this Axis axis, Boundaries boundaries, float ratio) {
			return axis switch {
				Axis.X => new Vector3(boundaries.Min.x + boundaries.Size.x * ratio, boundaries.Min.y + boundaries.Size.y / 2, boundaries.Min.z + boundaries.Size.z / 2),
				Axis.Y => new Vector3(boundaries.Min.x + boundaries.Size.x / 2, boundaries.Min.y + boundaries.Size.y * ratio, boundaries.Min.z + boundaries.Size.z / 2),
				Axis.Z => new Vector3(boundaries.Min.x + boundaries.Size.x / 2, boundaries.Min.y + boundaries.Size.y / 2, boundaries.Min.z + boundaries.Size.z * ratio),
				_ => throw new ArgumentOutOfRangeException(nameof(axis), axis, "There is no fourth dimension here")
			};
		}
		public static Quaternion Rotation(this Axis axis) {
			return axis switch {
				Axis.X => Quaternion.LookRotation(Vector3.left),
				Axis.Y => Quaternion.LookRotation(Vector3.down),
				Axis.Z => Quaternion.LookRotation(Vector3.forward),
				_ => throw new ArgumentOutOfRangeException(nameof(axis), axis, "There is no fourth dimension here")
			};
		}
		public static int Compose(this Axis axis, Index3 size, int u, int v, int w) {
			// Assume u is in the Axis.Previous() axis, v in the Axis.Next() axis, and w on this axis
			return axis switch {
				Axis.X => w + v * size.x + u * size.x * size.y,
				Axis.Y => u + w * size.x + v * size.x * size.y,
				Axis.Z => v + u * size.x + w * size.x * size.y,
				_ => throw new ArgumentOutOfRangeException(nameof(axis), axis, "There is no fourth dimension here")
			};
		}
	}
}