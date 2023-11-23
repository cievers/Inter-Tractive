using System.Collections.Generic;
using System.Drawing;
using Geometry;
using Maps.Cells;
using Maps.Grids;
using UnityEngine;

namespace Objects {
	public record Map(Dictionary<Cell, Color32> Colors, Cuboid?[] Cells, Index3 Composition, Boundaries Boundaries) {
		public Dictionary<Cell, Color32> Colors {get; private set;} = Colors;
		public Cuboid?[] Cells {get;} = Cells;
		public Boundaries Boundaries {get;} = Boundaries;
		public Index3 Composition {get;} = Composition;
	}
}