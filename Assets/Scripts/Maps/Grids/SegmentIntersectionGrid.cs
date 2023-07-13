using System.Collections.Generic;
using System.Linq;
using Geometry.Tracts;
using Maps.Cells;
using UnityEngine;

namespace Maps.Grids {
	public class SegmentIntersectionGrid : Grid {
		private readonly int size;
		private readonly float resolution;
		
		public SegmentIntersectionGrid(int size, float resolution) {
			this.size = size;
			this.resolution = resolution;
		}

		private IEnumerable<Cuboid> Cells() {
			var result = new List<Cuboid>();
			for (var x = -size; x < size; x++) {
				for (var y = -size; y < size; y++) {
					for (var z = -size; z < size; z++) {
						result.Add(new Cuboid(new Vector3(x * resolution, y * resolution, z * resolution), new Vector3(resolution, resolution, resolution)));
					}
				}
			}
			return result;
		}
		public Dictionary<Cuboid, IEnumerable<Tract>> Quantize(Tractogram tractogram) {
			// return Cells().ToDictionary(cell => cell, cell => tractogram.Tracts.Where(cell.Intersects));
			return Cells()
				.ToDictionary(cell => cell, cell => tractogram.Tracts.Where(cell.Intersects))
				.Where(entry => entry.Value.Any())
				.ToDictionary(entry => entry.Key, entry => entry.Value);
		}
		
		public Mesh Render(Dictionary<Cell,Color32> map) {
			var vertices = new List<Vector3>();
			var normals = new List<Vector3>(); // TODO: Normals aren't used in rendering yet
			var indices = new List<int>();
			
			foreach (var cell in Cells()) {
				// TODO: Creating indices can be simplified by something such as
				// Enumerable.Range(0, vertices.Length).ToArray();
				// Because we know the pattern needs to be 0, 1, 1, 2, 2, 3, 3, 4, 4, 5, ...

				vertices.AddRange(cell.Vertices);
				indices.AddRange(cell.Indices.Select(index => index + vertices.Count));
			}
			
			var shape = new Mesh();
		
			shape.Clear();
			shape.SetVertices(vertices);
			shape.SetTriangles(indices, 0);
			shape.RecalculateNormals();
			
			return shape;
		}
	}
}