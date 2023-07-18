using System.Collections.Generic;
using Geometry;
using Maps.Cells;
using UnityEngine;

namespace Maps.Grids {
	public interface Voxels {
		Mesh Render(Dictionary<Cell, Color32> colors);
		Index3 Size {get;}
		Boundaries Boundaries {get;}
		Cuboid?[] Cells {get;}
	}
}