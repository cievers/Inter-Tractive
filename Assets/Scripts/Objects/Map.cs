using System.Collections.Generic;
using Maps.Cells;
using Maps.Grids;
using UnityEngine;

namespace Objects {
	public struct Map {
		public Voxels Grid {get; private set;}
		public Dictionary<Cell, Color32> Colors {get; private set;}

		public Map(Voxels grid, Dictionary<Cell, Color32> colors) {
			Grid = grid;
			Colors = colors;
		}
	}
}