using System;

namespace Geometry {
	public enum Directions {
		Up,
		Down,
		Left,
		Right,
		Forward,
		Backward
	}
	public static class DirectionExtensions {
		public static Index3 Index(this Directions direction) {
			return direction switch {
				Directions.Up => new Index3(0, 1, 0),
				Directions.Down => new Index3(0, -1, 0),
				Directions.Left => new Index3(-1, 0, 0),
				Directions.Right => new Index3(1, 0, 0),
				Directions.Forward => new Index3(0, 0, 1),
				Directions.Backward => new Index3(0, 0, -1),
				_ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
			};
		}
	}
}