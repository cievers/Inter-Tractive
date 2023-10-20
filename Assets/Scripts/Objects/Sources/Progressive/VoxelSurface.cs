using System;
using System.Linq;
using Geometry;
using Maps.Cells;
using Objects.Concurrent;
using UnityEngine;

namespace Objects.Sources.Progressive {
	public class VoxelSurface : Promise<float> {
		private Cuboid?[] cells;
		private Index3 size;
		private float template;

		public VoxelSurface(Cuboid?[] cells, Index3 size, float template) {
			this.cells = cells;
			this.size = size;
			this.template = template;
			Start();
		}
		
		protected override void Compute() {
			var count = 0;
			for (var x = 0; x < size.x; x++) {
				for (var y = 0; y < size.y; y++) {
					for (var z = 0; z < size.z; z++) {
						var index = new Index3(x, y, z);
						if (Filled(index)) {
							count += 6 - Neighbours(index);
						}
					}
				}
			}
			Complete(template * template * count);
		}
		private int Neighbours(Index3 index) {
			return Enum.GetValues(typeof(Directions)).Cast<Directions>().Count(direction => Filled(index + direction));
		}
		private bool Filled(Index3 index) {
			// Debug.Log("Checking if cell is filled at "+index);
			// Debug.Log(size);
			return index.x > 0 && index.y > 0 && index.z > 0 && index.x < size.x && index.y < size.y && index.z < size.z && cells[index.Index(size)] != null;
		}
	}
}