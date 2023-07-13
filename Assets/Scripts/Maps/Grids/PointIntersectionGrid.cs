using System.Collections.Generic;
using System.Linq;
using Geometry;
using Geometry.Tracts;
using Maps.Cells;
using UnityEngine;

namespace Maps.Grids {
	public class PointIntersectionGrid : Grid {
		private readonly float size;
		private readonly Dictionary<Index3, Cuboid> cells = new();

		public PointIntersectionGrid(Tractogram tractogram, float resolution) {
			size = tractogram.Slack / resolution;
			Debug.Log("Set step size for grid to "+size);
			// TODO: Some assertion that resolution is in the interval 0,1, 0 excluded
		}
		
		public Boundaries Boundaries => Boundaries.Join(cells.Values.Select(cell => new Boundaries(cell.Anchor, cell.Extent)));

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
		public Cuboid Cell(Vector3 vector) {
			var index = new Index3(vector, size);
			if (cells.TryGetValue(index, out var result)) {
				return result;
			}
			var cell = new Cuboid((Vector3) index * size, size);
			cells.Add(index, cell);
			return cell;
		}
		public Dictionary<Cuboid, IEnumerable<Tract>> Quantize(Tractogram tractogram) {
			var result = new Dictionary<Cuboid, HashSet<Tract>>();

			foreach (var tract in tractogram.Tracts) {
				foreach (var point in tract.Points) {
					var cell = Cell(point);
					if (!result.ContainsKey(cell)) {
						result.Add(cell, new HashSet<Tract>());
					}
					result[cell].Add(tract);
				}
			}

			return result.ToDictionary(pair => pair.Key, pair => pair.Value.AsEnumerable());
			
			// // return Cells().ToDictionary(cell => cell, cell => tractogram.Tracts.Where(cell.Intersects));
			// return Cells()
			// 	.ToDictionary(cell => cell, cell => tractogram.Tracts.Where(cell.Intersects))
			// 	.Where(entry => entry.Value.Any())
			// 	.ToDictionary(entry => entry.Key, entry => entry.Value);
		}

		public Mesh Render(Dictionary<Cell,Color32> map) {
			var vertices = new List<Vector3>();
			var normals = new List<Vector3>(); // TODO: Normals aren't used in rendering yet
			var colors = new List<Color32>();
			var indices = new List<int>();
			
			foreach (var cell in cells.Values) {
				indices.AddRange(cell.Indices.Select(index => index + vertices.Count));
				vertices.AddRange(cell.Vertices);
				colors.AddRange(cell.Vertices.Select(_ => map[cell]));
			}

			var shape = new Mesh();
		
			shape.Clear();
			shape.SetVertices(vertices);
			shape.SetColors(colors);
			shape.SetTriangles(indices, 0);
			shape.RecalculateNormals();
			
			return shape;
		}
	}
}