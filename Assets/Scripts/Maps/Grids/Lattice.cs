using Geometry;
using UnityEngine;

namespace Maps.Grids {
	public record Lattice(Vector3 Origin, Index3 Anchor, Index3 Size, Vector3 Cell) {
		public Vector3 Origin {get;} = Origin;
		public Index3 Anchor {get;} = Anchor;
		public Index3 Size {get;} = Size;
		public Vector3 Cell {get;} = Cell;
		public Vector3[,,] Points {get;} = Create(Origin, Anchor, Size, Cell);

		public Lattice(Vector3 origin, Index3 anchor, Index3 size, float cell) : this(origin, anchor, size, new Vector3(cell, cell, cell)) {}
		public Lattice(Index3 anchor, Index3 size, float cell) : this(Vector3.zero, anchor, size, new Vector3(cell, cell, cell)) {}

		private static Vector3[,,] Create(Vector3 origin, Index3 anchor, Index3 size, Vector3 cell) {
			var result = new Vector3[size.x + 1, size.y + 1, size.z + 1];
			for (var x = 0; x <= size.x; x++) {
				for (var y = 0; y <= size.y; y++) {
					for (var z = 0; z <= size.z; z++) {
						result[x, y, z] = new Vector3((x + anchor.x) * cell.x + origin.x, (y + anchor.y) * cell.y + origin.y, (z + anchor.z) * cell.z + origin.z);
					}
				}
			}
			return result;
		}
		public Lattice Divide() {
			return new Lattice(Origin, Anchor * 2, Size * 2, Cell / 2);
		}

		public Vector3 this[int x, int y, int z] => Points[x, y, z];
	}
}