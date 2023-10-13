using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Camera;
using Evaluation;
using Evaluation.Coloring;
using Evaluation.Geometric;
using Files;
using Files.Types;
using Geometry;
using Geometry.Generators;
using Geometry.Tracts;
using Interface.Content;
using Interface.Control;
using Logic.Eventful;
using Maps.Cells;
using Maps.Grids;
using Objects.Concurrent;
using UnityEngine;

using Boolean = Logic.Eventful.Boolean;
using Exporter = Interface.Control.Exporter;

namespace Objects.Sources.Progressive {
	public class Tractometry : Voxels {
		public MeshFilter tractMesh;
		public MeshFilter tractogramMesh;
		public MeshFilter gridMesh;
		public MeshFilter spanMesh;
		public MeshFilter cutMesh;
		public MeshFilter volumeMesh;
		public GameObject dot;

		private Tractogram tractogram;
		private ThreadedLattice grid;
		private new ThreadedRenderer renderer;
		private CrossSectionExtrema prominentCuts;
		private Summary summary;

		private ConcurrentPipe<Tuple<Cell, Tract>> voxels;
		private ConcurrentBag<Dictionary<Cell, Vector>> measurements;
		private ConcurrentBag<Dictionary<Cell, Color32>> colors;
		private ConcurrentPipe<Model> maps;

		private Thread quantizeThread;
		private Thread renderThread;
		
		private PromiseCollector<Tract> promisedCore;
		private PromiseCollector<Model> promisedCut;
		private PromiseCollector<Hull> promisedVolume;

		private Files.Exporter exportMap;
		private Files.Exporter exportCore;
		private Files.Exporter exportSummary;

		private Any loading;

		private int samples = 32;
		private float prominence = 0;
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
			summary = new Summary();

			promisedCore = new PromiseCollector<Tract>();
			promisedCut = new PromiseCollector<Model>();
			promisedVolume = new PromiseCollector<Hull>();

			exportMap = new Files.Exporter("Save as NIFTI", "nii", Nifti);
			exportCore = new Files.Exporter("Save as TCK", "tck", () => throw new NotImplementedException());
			exportSummary = new Files.Exporter("Save numeric summary", "json", summary.Json);

			tractogram = Tck.Load(path);
			evaluation = new TractEvaluation(new CompoundMetric(new TractMetric[] {new Length()}), new Rgb());
			
			loading = new Any(new Boolean[] {maps, promisedCore, promisedCut, promisedVolume});
			loading.Change += state => Loading(!state);

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
			}

			if (promisedCore.TryTake(out var core)) {
				var wires = new WireframeRenderer();
				tractMesh.mesh = wires.Render(core);
				spanMesh.mesh = wires.Render(new ArrayTract(new[] {core.Points[0], core.Points[^1]}));
			}
			if (promisedCut.TryTake(out var cuts)) {
				cutMesh.mesh = cuts.Mesh();
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
			var core = new Core(sampler);
			var cut = new CrossSection(sampler, core);
			var volume = new Volume(sampler, cut);

			prominentCuts = new CrossSectionExtrema(cut, prominence);

			promisedCore.Add(core);
			promisedVolume.Add(volume);
			core.Request(summary.Core);
			cut.Request(summary.CrossSections);
			// cut.Request(cuts => core.Request(tract => summary.CrossSectionsVolume(tract, cuts)));
			UpdateCutEvaluation();
		}
		private void UpdateSamples(int samples) {
			this.samples = samples;
			UpdateSamples();
		}
		private void UpdateSamples(float samples) {
			UpdateSamples((int) Math.Round(samples));
		}
		private void UpdateCutProminence(float prominence) {
			this.prominence = prominence;
			prominentCuts.UpdateProminence(prominence);
			UpdateCutEvaluation();
		}
		private void UpdateCutEvaluation() {
			promisedCut.Add(new CrossSectionEvaluation(prominentCuts));
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
		
		public override IEnumerable<Interface.Component> Controls() {
			return new Interface.Component[] {
				new ActionToggle.Data("Tracts", true, tractogramMesh.gameObject.SetActive),
				new Divider.Data(),
				new Folder.Data("Global measuring", new List<Controller> {
					new Loader.Data(promisedCore, new ActionToggle.Data("Mean", true, tractMesh.gameObject.SetActive)),
					new Loader.Data(promisedCore, new ActionToggle.Data("Span", false, spanMesh.gameObject.SetActive)),
					new Loader.Data(promisedCut, new ActionToggle.Data("Cross-section", false, cutMesh.gameObject.SetActive)),
					new TransformedSlider.Data("Cross-section prominence", 0, value => value, (_, transformed) => ((int) Math.Round(transformed * 100)).ToString() + '%', new ValueChangeBuffer<float>(0.1f, UpdateCutProminence).Request),
					new Loader.Data(promisedVolume, new ActionToggle.Data("Volume", false, volumeMesh.gameObject.SetActive)),
					new TransformedSlider.Exponential("Resample count", 2, 5, 1, 8, (_, transformed) => ((int) Math.Round(transformed)).ToString(), new ValueChangeBuffer<float>(0.1f, UpdateSamples).Request),
					new Exporter.Data("Export summary", exportSummary)
				}),
				new Divider.Data(),
				new Folder.Data("Local measuring", new List<Controller> {
					new Loader.Data(maps, new ActionToggle.Data("Map", true, gridMesh.gameObject.SetActive)),
					new Interface.Control.Evaluation.Data(UpdateEvaluation),
					new Exporter.Data("Export map", exportMap)
				}),
				new Divider.Data(),
				new Folder.Data("Rendering", new List<Controller> {
					new TransformedSlider.Exponential("Resolution", 10, 0, -1, 1, new ValueChangeBuffer<float>(0.1f, UpdateResolution).Request),
					new TransformedSlider.Exponential("Batch size", 2, 12, 1, 20, (_, transformed) => ((int) Math.Round(transformed)).ToString(), UpdateBatch),
				}),
				new Divider.Data()
			};
		}
		public override IEnumerable<Files.Publisher> Exports() {
			return new Publisher[] {exportMap, exportCore, exportSummary};
		}
		
		private Nii<float> Nifti() {
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
	}
}