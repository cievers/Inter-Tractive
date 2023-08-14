using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Camera;
using Evaluation;
using Evaluation.Coloring;
using Evaluation.Geometric;
using Files.Types;
using Geometry;
using Geometry.Generators;
using Geometry.Tracts;
using Interface.Control;
using Maps.Cells;
using Maps.Grids;
using Objects.Concurrent;
using UnityEngine;

namespace Objects.Sources {
	public class TractometryProgression : Voxels {
		public MeshFilter tractogramMesh;
		public MeshFilter gridMesh;

		private Tractogram tractogram;
		private ThreadedLattice grid;
		private new ThreadedRenderer renderer;
		private ConcurrentPipe<Tuple<Cell, Tract>> voxels;
		private ConcurrentBag<Dictionary<Cell, Vector>> measurements;
		private ConcurrentBag<Dictionary<Cell, Color32>> colors;
		private ConcurrentBag<Model> models;

		private Thread quantizeThread;
		private Thread renderThread;
		
		private Map map;
		private TractEvaluation evaluation;
		private Dictionary<Cell, Vector> measurement;

		protected override void New(string path) {
			voxels = new ConcurrentPipe<Tuple<Cell, Tract>>();
			measurements = new ConcurrentBag<Dictionary<Cell, Vector>>();
			colors = new ConcurrentBag<Dictionary<Cell, Color32>>();
			models = new ConcurrentBag<Model>();
			
			tractogram = Tck.Load(path);
			evaluation = new TractEvaluation(new CompoundMetric(new TractMetric[] {new Density(), new Length()}), new TransparentGrayscale());

			UpdateTracts();
			UpdateMap(1);
			Focus(new Focus(grid.Boundaries.Center, grid.Boundaries.Size.magnitude / 2 * 1.5f));
		}

		private void Update() {
			if (measurements.TryTake(out var measured)) {
				measurement = measured;
			}
			if (colors.TryTake(out var colored)) {
				map = new Map(colored, grid.Cells, grid.Size, grid.Boundaries);
				Configure(grid.Cells, colored, grid.Size, grid.Boundaries);
			}
			if (models.TryTake(out var model)) {
				gridMesh.mesh = model.Mesh();
			}
		}
		private void UpdateTracts() {
			tractogramMesh.mesh = new WireframeRenderer().Render(tractogram);
		}
		private void UpdateMap(float resolution) {
			grid = new ThreadedLattice(tractogram, resolution, voxels);
			renderer = new ThreadedRenderer(voxels, measurements, colors, models, grid, evaluation, 4096);

			quantizeThread?.Abort();
			renderThread?.Abort();
			quantizeThread = new Thread(grid.Start);
			renderThread = new Thread(renderer.Render);
			quantizeThread.Start();
			renderThread.Start();
		}

		public override Map Map() {
			return map;
		}
		public override Nii<float> Nifti() {
			return new Nii<float>(ToArray(grid.Cells, measurement, 0), grid.Size, grid.Boundaries.Min + new Vector3(grid.Resolution / 2, grid.Resolution / 2, grid.Resolution / 2), new Vector3(grid.Resolution, grid.Resolution, grid.Resolution));
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
		private float[] ToArray(IReadOnlyList<Cuboid?> cells, IReadOnlyDictionary<Cell, Vector> values, float fill) {
			var dimensions = values.Values.Select(vector => vector.Dimensions).Min();
			var result = new float[cells.Count * dimensions];
			for (var i = 0; i < cells.Count; i++) {
				if (cells[i] != null && values.ContainsKey(cells[i])) {
					for (var j = 0; j < dimensions; j++) {
						result[i * dimensions + j] = values[cells[i]][j];
					}
				} else {
					for (var j = 0; j < dimensions; j++) {
						result[i * dimensions + j] = fill;
					}
				}
			}
			return result;
		}
		
		public override IEnumerable<Controller> Controls() {
			return new Controller[] {
				new ActionToggle.Data("Tracts", true, tractogramMesh.gameObject.SetActive),
				new ActionToggle.Data("Map", true, gridMesh.gameObject.SetActive),
				new DelayedSlider.Data("Resolution", 1, 0.1f, 10, 0.1f, UpdateMap),
				new Interface.Control.Evaluation.Data()
			};
		}
	}
}