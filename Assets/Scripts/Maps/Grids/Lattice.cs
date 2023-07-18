using Geometry;
using UnityEngine;

namespace Maps.Grids {
	public record Lattice(Index3 Anchor, Index3 Size, Vector3 Cell) {
		public Index3 Anchor {get;} = Anchor;
		public Index3 Size {get;} = Size;
		public Vector3 Cell {get;} = Cell;
		public Vector3[,,] Points {get;} = Create(Anchor, Size, Cell);

		public Lattice(Index3 anchor, Index3 size, float cell) : this(anchor, size, new Vector3(cell, cell, cell)) {}

		private static Vector3[,,] Create(Index3 anchor, Index3 size, Vector3 cell) {
			var result = new Vector3[size.x + 1, size.y + 1, size.z + 1];
			for (var x = 0; x <= size.x; x++) {
				for (var y = 0; y <= size.y; y++) {
					for (var z = 0; z <= size.z; z++) {
						result[x, y, z] = new Vector3(x * cell.x + anchor.x, y * cell.y + anchor.y, z * cell.z + anchor.z);
					}
				}
			}
			return result;
		}

		public Vector3 this[int x, int y, int z] => Points[x, y, z];
	}
}