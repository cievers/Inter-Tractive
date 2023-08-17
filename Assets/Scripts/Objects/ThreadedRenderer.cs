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
		private readonly ConcurrentPipe<Model> models;
		private readonly ThreadedLattice grid;
		private TractEvaluation evaluation;
		private int batch;
		
		private List<Cell> voxelDelta;
		private Dictionary<Cell, HashSet<Tract>> voxels;
		private Dictionary<Cell, Vector> statistics;
		
		public ThreadedRenderer(ConcurrentPipe<Tuple<Cell, Tract>> input, ConcurrentBag<Dictionary<Cell, Vector>> measurements, ConcurrentBag<Dictionary<Cell, Color32>> colors, ConcurrentPipe<Model> models, ThreadedLattice grid, TractEvaluation evaluation, int batch) {
			this.input = input;
			this.measurements = measurements;
			this.colors = colors;
			this.models = models;
			this.grid = grid;
			this.evaluation = evaluation;
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
					Measure(voxelDelta);
					Publish();
				}
				if (input.IsCompleted) {
					models.Complete();
				}
			}
		}
		private void Measure() {
			Measure(voxels.Keys.ToList());
		}
		private void Measure(IEnumerable<Cell> delta) {
			foreach (var cell in delta) {
				statistics[cell] = evaluation.Measure(voxels[cell]);
			}
		}
		private void Publish() {
			if (statistics.Count > 0) {
				var copy = new Dictionary<Cell, Vector>(statistics); // This publish call can run from the main thread when the evaluation was changed, but rendering is still running
				var measured = evaluation.Color(copy);
				measurements.Add(copy);
				colors.Add(measured);
				models.Add(grid.Render(measured));
			}
		}
		public void Evaluate(TractEvaluation evaluation) {
			this.evaluation = evaluation;
			Measure();
			Publish();
		}
		public void Batch(int batch) {
			this.batch = batch;
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