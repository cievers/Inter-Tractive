using System;
using System.Collections.Generic;
using System.Linq;
using Camera;
using Files;
using Files.Types;
using Geometry.Generators;
using Geometry.Tracts;
using Interface.Data;
using JetBrains.Annotations;
using Maps;
using Maps.Cells;
using Maps.Grids;
using Objects.Sources;
using Statistics;
using Statistics.Geometric;
using UnityEditor;
using UnityEngine;

namespace Objects {
	public class TractographyInstance : SourceInstance {
		private const byte COLORIZE_TRANSPARENCY = 200;
		
		public MeshFilter tractogramMesh;
		public MeshFilter gridMesh;

		private Tractogram tractogram;
		private ArrayGrid grid;
		private Dictionary<Cell, IEnumerable<Tract>> voxels;
		private Dictionary<Cell, float> measurements; // TODO: Replace with an n-dimensional, unit based, measurement per voxel
		private Focus focus;
		private Map map;

		protected override void New(string path) {
			tractogram = Tck.Load(path);

			UpdateTracts();
			UpdateVoxels(10);
			UpdateMeasurement();
			UpdateMap();

			// var gridBoundaries = grid.Boundaries;
			// var nifti = new Nii<float>(ToArray(grid.Cells, measurement, 0), grid.Size, gridBoundaries.Min + new Vector3(grid.CellSize / 2, grid.CellSize / 2, grid.CellSize / 2), new Vector3(grid.CellSize, grid.CellSize, grid.CellSize));
			// nifti.Write();
		}

		private void UpdateTracts() {
			tractogramMesh.mesh = new WireframeRenderer().Render(tractogram);
		}
		private void UpdateVoxels(float resolution) {
			grid = new ArrayIntersectionGrid(tractogram, resolution);
			voxels = grid.Quantize(tractogram);
			focus = new Focus(grid.Boundaries.Center, grid.Boundaries.Size.magnitude / 2 * 1.5f);
			
			// TODO: Propagate an update to all the slices
			
			UpdateMeasurement();
			UpdateMap();
		}
		private void UpdateMap() {
			var colors = Colorize(measurements);
			gridMesh.mesh = grid.Render(colors);
			
			// TODO: Propagate an update to all the slices
			map = new Map(grid, colors);
		}
		private void UpdateMeasurement() {
			// var measurement = new Density().Measure(map);
			measurements = new Length().Measure(voxels);
		}

		public override Focus Focus() {
			return focus;
		}
		public override Map Map() {
			return map;
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

		public override IEnumerable<Toggle> Controls() {
			return new[] {
				new Toggle("Tracts", true, tractogramMesh.gameObject.SetActive),
				new Toggle("Map", true, gridMesh.gameObject.SetActive)
			};
		}
	}
}