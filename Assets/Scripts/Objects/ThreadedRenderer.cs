using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Geometry;
using Geometry.Tracts;
using Maps.Cells;
using Maps.Grids;
using Objects.Concurrent;
using Statistics.Geometric;
using UnityEngine;

namespace Objects {
	public class ThreadedRenderer {
		private const byte COLORIZE_TRANSPARENCY = 200;
		
		private readonly ConcurrentPipe<Tuple<Cell, Tract>> input;
		private readonly ConcurrentBag<Dictionary<Cell, float>> measurements;
		private readonly ConcurrentBag<Dictionary<Cell, Color32>> colors;
		private readonly ConcurrentBag<Model> models;
		private readonly ThreadedLattice grid;
		private readonly Length statistic;
		private readonly int batch;
		
		private List<Cell> voxelDelta;
		private Dictionary<Cell, HashSet<Tract>> voxels;
		private Dictionary<Cell, float> statistics;
		
		public ThreadedRenderer(ConcurrentPipe<Tuple<Cell, Tract>> input, ConcurrentBag<Dictionary<Cell, float>> measurements, ConcurrentBag<Dictionary<Cell, Color32>> colors, ConcurrentBag<Model> models, ThreadedLattice grid, Length statistic, int batch) {
			this.input = input;
			this.measurements = measurements;
			this.colors = colors;
			this.models = models;
			this.grid = grid;
			this.statistic = statistic;
			this.batch = batch;
			
			voxels = new Dictionary<Cell, HashSet<Tract>>();
			statistics = new Dictionary<Cell, float>();
		}
		public void Render() {
			while (!input.IsEmpty || !input.IsCompleted) {
				voxelDelta = new List<Cell>();
				for (var i = 0; i < batch && !input.IsEmpty; i++) {
					if (input.TryTake(out var result)) {
						// If it's the first tract for this cell, make sure an entry exists in the dictionary
						if (!voxels.ContainsKey(result.Item1)) {
							voxels.Add(result.Item1, new HashSet<Tract>());
						}
						voxels[result.Item1].Add(result.Item2);
						voxelDelta.Add(result.Item1);
					}
				}
				if (voxelDelta.Count > 0) {
					foreach (var cell in voxelDelta) {
						statistics[cell] = statistic.Measure(voxels[cell]);
					}

					if (statistics.Count > 0) {
						var measured = Colorize(statistics);
						measurements.Add(statistics);
						colors.Add(measured);
						models.Add(grid.Render(measured));
					}
				}
			}
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