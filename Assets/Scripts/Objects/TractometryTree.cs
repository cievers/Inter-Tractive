using System;
using System.Collections.Generic;
using System.Linq;
using Camera;
using Files;
using Files.Types;
using Geometry;
using Geometry.Generators;
using Geometry.Tracts;
using Interface.Control.Data;
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
	public class TractometryTree : SourceInstance {
		private const byte COLORIZE_TRANSPARENCY = 200;
		
		public MeshFilter tractogramMesh;
		public MeshFilter gridMesh;

		private Tractogram tractogram;
		private Dictionary<Index3, IEnumerable<Tract>> voxels;
		private Focus focus;
		private Map map;

		private List<IntersectionTree> layers;
		private int depth;

		protected override void New(string path) {
			tractogram = Tck.Load(path);

			layers = new List<IntersectionTree> {new(tractogram, 0, 0.01f)};
			depth = 0;

			UpdateTracts();
			UpdateVoxels();
			Focus(new Focus(layers[depth].Boundaries.Center, layers[depth].Boundaries.Size.magnitude / 2 * 1.5f));

			// var gridBoundaries = grid.Boundaries;
			// var nifti = new Nii<float>(ToArray(grid.Cells, measurement, 0), grid.Size, gridBoundaries.Min + new Vector3(grid.CellSize / 2, grid.CellSize / 2, grid.CellSize / 2), new Vector3(grid.CellSize, grid.CellSize, grid.CellSize));
			// nifti.Write();
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
			
			var colors = Colorize(measurements);
			gridMesh.mesh = grid.Render(colors);
			
			Configure(grid.Cells, colors, grid.Size, grid.Boundaries);
			map = new Map(grid, colors);
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

		public override IEnumerable<Configuration> Controls() {
			return new Configuration[] {
				new Toggle("Tracts", true, tractogramMesh.gameObject.SetActive),
				new Toggle("Map", true, gridMesh.gameObject.SetActive),
				new Stepper("Resolution", 1, 0, 0, 10, UpdateScale)
				// new DelayedSlider("Resolution", 0, 0, 10, 1, UpdateScale)
			};
		}
	}
}