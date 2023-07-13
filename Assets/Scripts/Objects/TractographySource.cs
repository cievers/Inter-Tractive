using System;
using System.Collections.Generic;
using System.Linq;
using Camera;
using Files;
using Files.Types;
using Geometry.Generators;
using Interface;
using JetBrains.Annotations;
using Maps;
using Maps.Cells;
using Maps.Grids;
using Statistics;
using Statistics.Geometric;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Objects {
	public class TractographySource : Objects.Sources.Source {
		private const byte COLORIZE_TRANSPARENCY = 200;

		public GameObject mesh;
		public new OrbitingCamera camera;
		public ArraySlice[] slices;

		private string path;

		// private void Start() {
		// 	var tractogram = selector.SelectTractogram();
		// 	var boundaries = tractogram.Boundaries;
		// 	Debug.Log(boundaries.Min);
		// 	Debug.Log(boundaries.Max);
		// 	var grid = new PointIntersectionGrid(tractogram, 1);
		// 	var cells = grid.Quantize(tractogram);
		// 	var colors = cells
		// 		.ToDictionary(pair => (Cell)pair.Key, pair => (byte)Math.Min(pair.Value.Count(), 255))
		// 		.ToDictionary(pair => pair.Key, pair => new Color32(pair.Value, pair.Value, pair.Value, 55));
		// 	Debug.Log(cells.Count);
		// 	// foreach (var entry in cells) {
		// 	// Debug.Log(entry.Key.Anchor);
		// 	// Debug.Log(entry.Value.Count());
		// 	// }
		// 	tractogramMesh.mesh = renderer.Render(tractogram);
		// 	gridMesh.mesh = grid.Render(colors);
		// 	slice.Initialize(grid, colors);
		// }
		// public GameObject Instantiate() {
		//
		// 	// var lengths = new Histogram<int>(tractogram.Tracts.Select(tract => tract.Points.Length));
		// 	// foreach (var line in lengths.Log()) {
		// 	// 	Debug.Log(line);
		// 	// }
		//
		// 	// camera.Target(grid.Boundaries.Center);
		//
		// 	// var colorGrid = ToArray(grid.Cells, colors, new Color32(0, 0, 0, 0));
		// 	// foreach (var arraySlice in slices) {
		// 	// 	arraySlice.Initialize(colorGrid, grid.Size, grid.Boundaries);
		// 	// }
		// 	//
		// 	// var gridBoundaries = grid.Boundaries;
		// 	// var nifti = new Nii<float>(ToArray(grid.Cells, measurement, 0), grid.Size, gridBoundaries.Min + new Vector3(grid.CellSize / 2, grid.CellSize / 2, grid.CellSize / 2), new Vector3(grid.CellSize, grid.CellSize, grid.CellSize));
		// 	// nifti.Write();
		// 	
		// 	return root;
		// }
		public TractographySource(string path) {
			this.path = path;
			
			var tractogram = Tck.Load(path);
			var boundaries = tractogram.Boundaries;
			Debug.Log(boundaries.Min);
			Debug.Log(boundaries.Max);

			var grid = new ArrayIntersectionGrid(tractogram, 10);
			var map = grid.Quantize(tractogram);
			// var measurement = new Density().Measure(map);
			var measurement = new Length().Measure(map);
			var colors = Colorize(measurement);
			
			var root = new GameObject();
			
			var tractogramObject = new GameObject();
			var tractogramMesh = tractogramObject.AddComponent<MeshFilter>();
			tractogramMesh.mesh = new WireframeRenderer().Render(tractogram);
			tractogramObject.AddComponent<MeshRenderer>();
			tractogramObject.transform.parent = root.transform;
			
			var gridObject = new GameObject();
			var gridMesh = gridObject.AddComponent<MeshFilter>();
			gridMesh.mesh = grid.Render(colors);
			gridObject.AddComponent<MeshRenderer>();
			gridObject.transform.parent = root.transform;
		}
		
		private Dictionary<Cell, Color32> Colorize(Dictionary<Cell,int> values) {
			var limit = (float) values.Values.Max();
			return values
				.ToDictionary(pair => pair.Key, pair => (byte) (pair.Value / limit * 255))
				.ToDictionary(pair => pair.Key, pair => new Color32(pair.Value, pair.Value, pair.Value, COLORIZE_TRANSPARENCY));
		}
		private Dictionary<Cell, Color32> Colorize(Dictionary<Cell,float> values) {
			var limit = values.Values.Max();
			return values
				.ToDictionary(pair => pair.Key, pair => (byte) (pair.Value / limit * 255))
				.ToDictionary(pair => pair.Key, pair => new Color32(pair.Value, pair.Value, pair.Value, COLORIZE_TRANSPARENCY));
		}

		private T[] ToArray<T>(IReadOnlyList<Cuboid?> cells, IReadOnlyDictionary<Cell, T> values, T fill) {
			var result = new T[cells.Count];
			for (var i = 0; i < cells.Count; i++) {
				if (cells[i] != null && values.ContainsKey(cells[i])) {
					result[i] = values[cells[i]];
				} else {
					result[i] = fill;
				}
			}
			return result;
		}
	}
}