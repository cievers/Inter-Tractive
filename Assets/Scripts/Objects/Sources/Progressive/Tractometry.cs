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
using Interface.Content;
using Interface.Control;
using Maps.Cells;
using Maps.Grids;
using Objects.Concurrent;
using UnityEngine;

namespace Objects.Sources.Progressive {
	public class Tractometry : Voxels {
		public MeshFilter tractMesh;
		public MeshFilter tractogramMesh;
		public MeshFilter gridMesh;
		public MeshFilter cutMesh;
		public MeshFilter volumeMesh;
		public GameObject dot;

		private Tractogram tractogram;
		private ThreadedLattice grid;
		private new ThreadedRenderer renderer;
		
		private ConcurrentPipe<Tuple<Cell, Tract>> voxels;
		private ConcurrentBag<Dictionary<Cell, Vector>> measurements;
		private ConcurrentBag<Dictionary<Cell, Color32>> colors;
		private ConcurrentPipe<Model> maps;

		private Thread quantizeThread;
		private Thread renderThread;
		
		private PromiseCollector<Tract> promisedMean;
		private PromiseCollector<List<ConvexPolygon>> promisedCut;
		private PromiseCollector<ConvexPolyhedron> promisedVolume;

		private int samples = 32;
		private float resolution = 1;
		private int batch = 4096;
		private TractEvaluation evaluation;
		
		private Map map;
		private Dictionary<Cell, Vector> measurement;

		protected override void New(string path) {
			voxels = new ConcurrentPipe<Tuple<Cell, Tract>>();
			measurements = new ConcurrentBag<Dictionary<Cell, Vector>>();
			colors = new ConcurrentBag<Dictionary<Cell, Color32>>();
			maps = new ConcurrentPipe<Model>();

			promisedMean = new PromiseCollector<Tract>();
			promisedCut = new PromiseCollector<List<ConvexPolygon>>();
			promisedVolume = new PromiseCollector<ConvexPolyhedron>();
			
			tractogram = Tck.Load(path);
			evaluation = new TractEvaluation(new CompoundMetric(new TractMetric[] {new Length()}), new Rgb());

			UpdateSamples();
			UpdateTracts();
			UpdateMap();
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
			if (maps.TryTake(out var result)) {
				gridMesh.mesh = result.Mesh();
				
				if (maps.IsCompleted && maps.IsEmpty) {
					Loading(true);
				}
			}

			if (promisedMean.TryTake(out var mean)) {
				tractMesh.mesh = new WireframeRenderer().Render(mean);
			}
			if (promisedCut.TryTake(out var cuts)) {
				foreach (var point in cuts[0].Points) {
					// Instantiate(dot, point, Quaternion.identity);
				}
				// cutMesh.mesh = cuts[0].Mesh();
				cutMesh.mesh = Hull.Join(cuts.Select(cut => cut.Hull()).ToList()).Mesh();
			}
			if (promisedVolume.TryTake(out var hull)) {
				volumeMesh.mesh = hull.Mesh();
			}
		}
		private void UpdateTracts() {
			tractogramMesh.mesh = new WireframeRenderer().Render(tractogram);
		}
		private void UpdateSamples() {
			var sampler = new Resample(tractogram, samples);
			var mean = new Mean(sampler);
			var cut = new CrossSection(sampler, mean);
			var volume = new Volume(sampler, cut);
			
			promisedMean.Add(mean);
			promisedCut.Add(cut);
			promisedVolume.Add(volume);
		}
		private void UpdateSamples(int samples) {
			this.samples = samples;
			UpdateSamples();
		}
		private void UpdateSamples(float samples) {
			UpdateSamples((int) Math.Round(samples));
		}
		private void UpdateEvaluation(TractEvaluation evaluation) {
			this.evaluation = evaluation;
			// UpdateMap();
			renderer.Evaluate(evaluation);
		}
		private void UpdateResolution(float resolution) {
			this.resolution = resolution;
			UpdateMap();
		}
		private void UpdateBatch(int batch) {
			this.batch = batch;
			renderer.Batch(batch);
		}
		private void UpdateBatch(float batch) {
			UpdateBatch((int) Math.Round(batch));
		}
		private void UpdateMap() {
			Loading(false);
			voxels.Restart();
			maps.Restart();
			grid = new ThreadedLattice(tractogram, resolution, voxels);
			renderer = new ThreadedRenderer(voxels, measurements, colors, maps, grid, evaluation, batch);

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
		
		public override IEnumerable<Interface.Component> Controls() {
			return new Interface.Component[] {
				new ActionToggle.Data("Tracts", true, tractogramMesh.gameObject.SetActive),
				new Divider.Data(),
				new Folder.Data("Global measuring", new List<Controller> {
					new ActionToggle.Data("Mean", true, tractMesh.gameObject.SetActive),
					new ActionToggle.Data("Cross-section", true, cutMesh.gameObject.SetActive),
					new ActionToggle.Data("Volume", true, volumeMesh.gameObject.SetActive),
					new TransformedSlider.Exponential("Resample count", 2, 5, 1, 8, (_, transformed) => ((int) transformed).ToString(), new ValueChangeBuffer<float>(0.1f, UpdateSamples).Request),
				}),
				new Divider.Data(),
				new Folder.Data("Local measuring", new List<Controller> {
					new ActionToggle.Data("Map", false, gridMesh.gameObject.SetActive),
					new Interface.Control.Evaluation.Data(UpdateEvaluation),
				}),
				new Divider.Data(),
				new Folder.Data("Rendering", new List<Controller> {
					new TransformedSlider.Exponential("Resolution", 10, 0, -1, 1, new ValueChangeBuffer<float>(0.1f, UpdateResolution).Request),
					new TransformedSlider.Exponential("Batch size", 2, 12, 1, 20, (_, transformed) => ((int) transformed).ToString(), UpdateBatch),
				}),
				new Divider.Data()
			};
		}
	}
}