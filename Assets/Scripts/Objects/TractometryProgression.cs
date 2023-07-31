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
		private ConcurrentBag<Tuple<Cell, Tract>> bag;
		private Dictionary<Cell, HashSet<Tract>> voxels;
		private List<Cell> voxelDelta;
		private Dictionary<Cell, float> measurements;
		private Map map;
		private Length statistic;

		protected override void New(string path) {
			tractogram = Tck.Load(path);
			bag = new ConcurrentBag<Tuple<Cell, Tract>>();
			grid = new ThreadedLattice(tractogram, 1, bag);
			voxels = new Dictionary<Cell, HashSet<Tract>>();
			measurements = new Dictionary<Cell, float>();
			statistic = new Length();

			Focus(new Focus(grid.Boundaries.Center, grid.Boundaries.Size.magnitude / 2 * 1.5f));
			UpdateTracts();
			new Thread(grid.Start).Start();

			// var gridBoundaries = grid.Boundaries;
			// var nifti = new Nii<float>(ToArray(grid.Cells, measurement, 0), grid.Size, gridBoundaries.Min + new Vector3(grid.CellSize / 2, grid.CellSize / 2, grid.CellSize / 2), new Vector3(grid.CellSize, grid.CellSize, grid.CellSize));
			// nifti.Write();
		}

		private void Update() {
			voxelDelta = new List<Cell>();
			if (!bag.IsEmpty) {
				Debug.Log(bag.Count);
				for (var i = 0; i < 1000 && !bag.IsEmpty; i++) {
				// while (!bag.IsEmpty) {
					if (bag.TryTake(out var result)) {
						// If it's the first tract for this cell, make sure an entry exists in the dictionary
						if (!voxels.ContainsKey(result.Item1)) {
							voxels.Add(result.Item1, new HashSet<Tract>());
						}
						voxels[result.Item1].Add(result.Item2);
						voxelDelta.Add(result.Item1);
					}
				}
				if (voxelDelta.Count > 0) {
					UpdateMap();
				}
			}
		}
		private void UpdateTracts() {
			tractogramMesh.mesh = new WireframeRenderer().Render(tractogram);
		}
		private void UpdateMap() {
			// var measurements = new Density().Measure(voxels);
			Debug.Log("Making measurements");
			var watch = new Stopwatch();
			watch.Start();
			foreach (var cell in voxelDelta) {
				measurements[cell] = statistic.Measure(voxels[cell]);
			}
			// var measurements = statistic.Measure(voxels.ToDictionary(pair => pair.Key, pair => pair.Value.AsEnumerable()));
			watch.Stop();
			Debug.Log(watch.Elapsed);

			if (measurements.Count > 0) {
				Debug.Log("Coloring measurements");
				watch = new Stopwatch();
				watch.Start();
				var colors = Colorize(measurements);
				watch.Stop();
				Debug.Log(watch.Elapsed);
				Debug.Log("Making mesh");
				watch = new Stopwatch();
				watch.Start();
				gridMesh.mesh = grid.Render(colors);
				watch.Stop();
				Debug.Log(watch.Elapsed);
			
				Configure(grid.Cells, colors, grid.Size, grid.Boundaries);
				map = new Map(colors, grid.Cells, grid.Size, grid.Boundaries);
			} else {
				// Debug.Log("Still no cells with a measurement");
				// Debug.Log(measurements.Count);
				// Debug.Log(voxels.Count);
			}
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