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
		private bool evaluationChanged;
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
			while (!input.IsCompleted) {
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
					if (evaluationChanged) {
						// If the evaluation method changed through user input, re-evaluate all cells
						Measure();
						evaluationChanged = false;
					} else {
						Measure(voxelDelta);
					}
					Publish();
				}
				if (input.IsCompleted) {
					models.Sent();
				}
			}
		}
		private void Measure() {
			Measure(voxels.Keys);
		}
		private void Measure(IEnumerable<Cell> delta) {
			foreach (var cell in delta) {
				statistics[cell] = evaluation.Measure(voxels[cell]);
			}
		}
		private void Publish() {
			if (statistics.Count > 0) {
				var measured = evaluation.Color(statistics);
				measurements.Add(statistics); // Does this mean we just feed it the same dictionary instance every time, just filled with different data?
				colors.Add(measured);
				models.Add(grid.Render(measured));
			}
		}
		public void Evaluate(TractEvaluation evaluation) {
			this.evaluation = evaluation;
			if (input.IsCompleted) {
				Measure();
				Publish();
			} else {
				// If the thread is still running it will automatically publish again, but we do need to signal it that it needs to re-evaluate past cells
				evaluationChanged = true;
			}
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