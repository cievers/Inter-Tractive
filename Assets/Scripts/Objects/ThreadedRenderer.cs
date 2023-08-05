using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Evaluation;
using Evaluation.Coloring;
using Evaluation.Geometric;
using Geometry;
using Geometry.Tracts;
using Maps.Cells;
using Maps.Grids;
using Objects.Concurrent;
using UnityEngine;

namespace Objects {
	public class ThreadedRenderer {
		private readonly ConcurrentPipe<Tuple<Cell, Tract>> input;
		private readonly ConcurrentBag<Dictionary<Cell, Vector>> measurements;
		private readonly ConcurrentBag<Dictionary<Cell, Color32>> colors;
		private readonly ConcurrentBag<Model> models;
		private readonly ThreadedLattice grid;
		private readonly TractMetric statistic;
		private readonly Coloring coloring;
		private readonly int batch;
		
		private List<Cell> voxelDelta;
		private Dictionary<Cell, HashSet<Tract>> voxels;
		private Dictionary<Cell, Vector> statistics;
		
		public ThreadedRenderer(ConcurrentPipe<Tuple<Cell, Tract>> input, ConcurrentBag<Dictionary<Cell, Vector>> measurements, ConcurrentBag<Dictionary<Cell, Color32>> colors, ConcurrentBag<Model> models, ThreadedLattice grid, TractMetric statistic, Coloring coloring, int batch) {
			this.input = input;
			this.measurements = measurements;
			this.colors = colors;
			this.models = models;
			this.grid = grid;
			this.statistic = statistic;
			this.coloring = coloring;
			this.batch = batch;
			
			voxels = new Dictionary<Cell, HashSet<Tract>>();
			statistics = new Dictionary<Cell, Vector>();
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
						var measured = coloring.Color(statistics);
						measurements.Add(statistics);
						colors.Add(measured);
						models.Add(grid.Render(measured));
					}
				}
			}
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