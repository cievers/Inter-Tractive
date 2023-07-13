using System.Collections.Generic;
using Maps.Cells;
using UnityEngine;

namespace Maps.Grids {
	public interface Grid {
		public Mesh Render(Dictionary<Cell,Color32> map);
	}
}