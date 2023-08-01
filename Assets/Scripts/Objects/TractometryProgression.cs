using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Camera;
using Files;
using Files.Types;
using Geometry.Generators;
using Geometry.Tracts;
using Interface.Control.Data;
using JetBrains.Annotations;
using Maps;
using Maps.Cells;
using Maps.Grids;
using Objects.Concurrent;
using Objects.Sources;
using Statistics;
using Statistics.Geometric;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Objects {
	public class TractometryProgression : SourceInstance {
		private const byte COLORIZE_TRANSPARENCY = 200;
		
		public MeshFilter tractogramMesh;
		public MeshFilter gridMesh;

		private Tractogram tractogram;
		private ThreadedLattice grid;
		private new ThreadedRenderer renderer;
		private ConcurrentPipe<Tuple<Cell, Tract>> voxels;
		private ConcurrentBag<Dictionary<Cell, Color32>> colors;
		private ConcurrentBag<Mesh> meshes;
		
		private Map map;
		private Length statistic;

		protected override void New(string path) {
			voxels = new ConcurrentPipe<Tuple<Cell, Tract>>();
			colors = new ConcurrentBag<Dictionary<Cell, Color32>>();
			meshes = new ConcurrentBag<Mesh>();
			
			tractogram = Tck.Load(path);
			statistic = new Length();
			grid = new ThreadedLattice(tractogram, 1, voxels);
			renderer = new ThreadedRenderer(voxels, colors, meshes, grid, statistic);

			Focus(new Focus(grid.Boundaries.Center, grid.Boundaries.Size.magnitude / 2 * 1.5f));
			UpdateTracts();
			new Thread(grid.Start).Start();
			new Thread(renderer.Render).Start();

			// var gridBoundaries = grid.Boundaries;
			// var nifti = new Nii<float>(ToArray(grid.Cells, measurement, 0), grid.Size, gridBoundaries.Min + new Vector3(grid.CellSize / 2, grid.CellSize / 2, grid.CellSize / 2), new Vector3(grid.CellSize, grid.CellSize, grid.CellSize));
			// nifti.Write();
		}

		private void Update() {
			if (colors.TryTake(out var result)) {
				map = new Map(result, grid.Cells, grid.Size, grid.Boundaries);
				Configure(grid.Cells, result, grid.Size, grid.Boundaries);
			}
			if (meshes.TryTake(out var mesh)) {
				gridMesh.mesh = mesh;
			}
		}
		private void UpdateTracts() {
			tractogramMesh.mesh = new WireframeRenderer().Render(tractogram);
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
				// new DelayedSlider("Resolution", 1, 0.1f, 10, 1, UpdateVoxels)
			};
		}
	}
}