using System.Collections.Generic;
using System.Linq;
using Geometry;
using Geometry.Tracts;
using Maps.Cells;
using UnityEngine;
using UnityEngine.Rendering;

namespace Maps.Grids {
	public class ArrayGrid : Grid {
		private readonly Index3 gridAnchor;
		private readonly Index3 gridSize;
		private readonly float cellSize;
		protected readonly Cuboid?[] cells;

		public ArrayGrid(Tractogram tractogram, float resolution) {
			cellSize = resolution;
			var boundaries = tractogram.Boundaries;
			gridAnchor = new Index3(boundaries.Min, cellSize);
			gridSize = new Index3(boundaries.Max, cellSize) + new Index3(1, 1, 1) - gridAnchor;
			cells = new Cuboid?[gridSize.x * gridSize.y * gridSize.z];
		}

		// public Boundaries Boundaries => Boundaries.Join(cells.Values.Select(cell => new Boundaries(cell.Anchor, cell.Extent)));
		public Index3 Anchor => gridAnchor;
		public Index3 Size => gridSize;
		public Cuboid?[] Cells => cells;
		public Boundaries Boundaries => new((Vector3) gridAnchor * cellSize, (Vector3) (gridAnchor + gridSize) * cellSize);
		public float CellSize => cellSize;

		// private IEnumerable<Cuboid> Cells() {
		// 	var result = new List<Cuboid>();
		// 	for (var x = -size; x < size; x++) {
		// 		for (var y = -size; y < size; y++) {
		// 			for (var z = -size; z < size; z++) {
		// 				result.Add(new Cuboid(new Vector3(x * resolution, y * resolution, z * resolution), new Vector3(resolution, resolution, resolution)));
		// 			}
		// 		}
		// 	}
		// 	return result;
		// }
		protected Index3 Index(Vector3 vector) {
			return new Index3(vector, cellSize) - gridAnchor;
		}
		private Cuboid? Get(Index3 index) {
			return cells[index.x + index.y * gridSize.x + index.z * gridSize.x * gridSize.y];
		}
		private void Set(Index3 index, Cuboid cell) {
			cells[index.x + index.y * gridSize.x + index.z * gridSize.x * gridSize.y] = cell;
		}
		public Cuboid Quantize(Vector3 vector) {
			return Quantize(Index(vector));
		}
		protected Cuboid Quantize(Index3 index) {
			var cell = Get(index);
			if (cell == null) {
				cell = new Cuboid((Vector3) (index + gridAnchor) * cellSize, cellSize);
				Set(index, (Cuboid) cell);
				// throw new IndexOutOfRangeException("There is no cell defined at this position");
			}
			return (Cuboid) cell;
		}
		public virtual Dictionary<Cell, IEnumerable<Tract>> Quantize(Tractogram tractogram) {
			var result = new Dictionary<Cuboid, HashSet<Tract>>();

			foreach (var tract in tractogram.Tracts) {
				foreach (var point in tract.Points) {
					var cell = Quantize(point);
					if (!result.ContainsKey(cell)) {
						result.Add(cell, new HashSet<Tract>());
					}
					result[cell].Add(tract);
				}
			}

			return result.ToDictionary(pair => (Cell) pair.Key, pair => pair.Value.AsEnumerable());

			// // return Cells().ToDictionary(cell => cell, cell => tractogram.Tracts.Where(cell.Intersects));
			// return Cells()
			// 	.ToDictionary(cell => cell, cell => tractogram.Tracts.Where(cell.Intersects))
			// 	.Where(entry => entry.Value.Any())
			// 	.ToDictionary(entry => entry.Key, entry => entry.Value);
		}

		public Mesh Render(Dictionary<Cell, Color32> map) {
			var vertices = new List<Vector3>();
			var normals = new List<Vector3>(); // TODO: Normals aren't used in rendering yet
			var colors = new List<Color32>();
			var indices = new List<int>();

			foreach (var cell in cells) {
				if (cell != null && map.ContainsKey(cell)) {
					var value = (Cuboid) cell;
					indices.AddRange(value.Indices.Select(index => index + vertices.Count));
					vertices.AddRange(value.Vertices);
					colors.AddRange(value.Vertices.Select(_ => map[cell]));
				}
			}

			var shape = new Mesh {indexFormat = IndexFormat.UInt32};

			shape.Clear();
			shape.SetVertices(vertices);
			shape.SetColors(colors);
			shape.SetTriangles(indices, 0);
			shape.RecalculateNormals();

			return shape;
		}
	}
}