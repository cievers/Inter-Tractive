using System;

namespace Geometry {
	public record AxisOrder(Axis Primary, Axis Secondary, Axis Tertiary) {
		public Axis Primary {get;} = Primary;
		public Axis Secondary {get;} = Secondary;
		public Axis Tertiary {get;} = Tertiary;

		public Axis Index(int axis) {
			return axis switch {
				0 => Primary,
				1 => Secondary,
				2 => Tertiary,
				_ => throw new ArgumentOutOfRangeException(nameof(axis), axis, "There is no fourth dimension here")
			};
		}
		public int Index(Axis axis) {
			if (axis == Primary) {
				return 0;
			}
			if (axis == Secondary) {
				return 1;
			}
			if (axis == Tertiary) {
				return 2;
			}
			throw new ArgumentOutOfRangeException(nameof(axis), axis, "There is no fourth dimension here");
		}
		public Axis Previous(Axis axis) {
			return Index((Index(axis) + 2) % 3);
		}
		public Axis Next(Axis axis) {
			return Index((Index(axis) + 1) % 3);
		}
		public int Compose(Axis axis, Index3 composition, int u, int v, int w) {
			// Assume u is in the Axis.Previous() axis, v in the Axis.Next() axis, and w on this axis
			return axis switch {
				Axis.X => w + v * composition.x + u * composition.x * composition.y,
				Axis.Y => u + w * composition.x + v * composition.x * composition.y,
				Axis.Z => u + v * composition.x + w * composition.x * composition.y,
				_ => throw new ArgumentOutOfRangeException(nameof(axis), axis, "There is no fourth dimension here")
			};
		}
	};
}