using System.Linq;
using Maps.Cells;
using Objects.Concurrent;
using UnityEngine;

namespace Objects.Sources.Progressive {
	public class VoxelVolume : Promise<float> {
		private Cuboid?[] cells;
		private Vector3 template;

		public VoxelVolume(Cuboid?[] cells, Vector3 template) {
			this.cells = cells;
			this.template = template;
			Start();
		}
		public VoxelVolume(Cuboid?[] cells, float resolution) : this(cells, new Vector3(resolution, resolution, resolution)) {}
		
		protected override void Compute() {
			Complete(template.x * template.y * template.z * cells.Count(cell => cell != null));
		}
	}
}