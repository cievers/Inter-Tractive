using System;
using System.Collections.Generic;
using Camera;
using Evaluation.Coloring;
using Evaluation.Geometric;
using Files.Types;
using Geometry;
using Geometry.Generators;
using Geometry.Tracts;
using Interface.Control;
using Maps.Cells;
using Maps.Grids;
using UnityEngine;

namespace Objects.Sources {
	public class TractometryTree : Voxels {
		private const byte COLORIZE_TRANSPARENCY = 200;
		
		public MeshFilter tractogramMesh;
		public MeshFilter gridMesh;

		private Tractogram tractogram;
		private Coloring coloring;
		private Dictionary<Index3, IEnumerable<Tract>> voxels;
		private Focus focus;
		private Map map;

		private List<IntersectionTree> layers;
		private int depth;

		protected override void New(string path) {
			tractogram = Tck.Load(path);
			coloring = new Grayscale();

			layers = new List<IntersectionTree> {new(tractogram, 0, 0.01f)};
			depth = 0;

			UpdateTracts();
			UpdateVoxels();
			Focus(new Focus(layers[depth].Boundaries.Center, layers[depth].Boundaries.Size.magnitude / 2 * 1.5f));

		}

		private void UpdateTracts() {
			tractogramMesh.mesh = new WireframeRenderer().Render(tractogram);
		}
		private void UpdateScale(float resolution) {
			Debug.Log("Stepping to resolution "+resolution);
			depth = (int) Math.Round(resolution);
			UpdateVoxels();
		}
		private void UpdateVoxels() {
			while (depth >= layers.Count) {
				layers.Add(layers[^1].Divide());
			}
			
			UpdateMap(depth);
		}
		private void UpdateMap(int depth) {
			var grid = layers[depth];
			// var measurements = new Density().Measure(voxels);
			var measurements = new Length().Measure(grid.Voxels);
			
			var colors = coloring.Color(measurements);
			gridMesh.mesh = grid.Render(colors);
			
			Configure(grid.Cells, colors, grid.Size, grid.Boundaries);
			map = new Map(colors, grid.Cells, grid.Size, grid.Boundaries);
		}
		
		public override Map Map() {
			return map;
		}
		public override Nii<float> Nifti() {
			// var gridBoundaries = grid.Boundaries;
			// return new Nii<float>(ToArray(grid.Cells, measurement, 0), grid.Size, gridBoundaries.Min + new Vector3(grid.CellSize / 2, grid.CellSize / 2, grid.CellSize / 2), new Vector3(grid.CellSize, grid.CellSize, grid.CellSize));
			throw new NotImplementedException();
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

		public override IEnumerable<Controller> Controls() {
			return new Controller[] {
				new ActionToggle.Data("Tracts", true, tractogramMesh.gameObject.SetActive),
				new ActionToggle.Data("Map", true, gridMesh.gameObject.SetActive),
				// new Stepper.Data("Resolution", 1, 0, 0, 10, UpdateScale)
				new DelayedSlider.Data("Resolution", 0, 0, 10, 1, UpdateScale)
			};
		}
	}
}