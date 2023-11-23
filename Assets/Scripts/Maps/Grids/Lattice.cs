using Geometry;
using UnityEngine;

namespace Maps.Grids {
	public record Lattice(Vector3 Origin, Index3 Anchor, Index3 Composition, Vector3 Unit) {
		public Vector3 Origin {get;} = Origin;
		public Index3 Anchor {get;} = Anchor;
		public Index3 Composition {get;} = Composition;
		public Vector3 Unit {get;} = Unit;
		public Vector3[,,] Points {get;} = Create(Origin, Anchor, Composition, Unit);

		public Lattice(Vector3 origin, Index3 anchor, Index3 size, float cell) : this(origin, anchor, size, new Vector3(cell, cell, cell)) {}
		public Lattice(Index3 anchor, Index3 size, float cell) : this(Vector3.zero, anchor, size, new Vector3(cell, cell, cell)) {}

		private static Vector3[,,] Create(Vector3 origin, Index3 anchor, Index3 composition, Vector3 unit) {
			var result = new Vector3[composition.x + 1, composition.y + 1, composition.z + 1];
			for (var x = 0; x <= composition.x; x++) {
				for (var y = 0; y <= composition.y; y++) {
					for (var z = 0; z <= composition.z; z++) {
						result[x, y, z] = new Vector3((x + anchor.x) * unit.x + origin.x, (y + anchor.y) * unit.y + origin.y, (z + anchor.z) * unit.z + origin.z);
					}
				}
			}
			return result;
		}
		public Lattice Divide() {
			return new Lattice(Origin, Anchor * 2, Composition * 2, Unit / 2);
		}

		public Vector3 this[int x, int y, int z] => Points[x, y, z];
	}
}