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
using Geometry.Topology;
using Geometry.Tracts;
using Interface.Content;
using Interface.Control;
using Logic.Eventful;
using Maps.Cells;
using Maps.Grids;
using Objects.Collection;
using Objects.Concurrent;
using UnityEngine;
using UnityEngine.Serialization;
using Boolean = Logic.Eventful.Boolean;
using Exporter = Interface.Control.Exporter;

namespace Objects.Sources.Progressive {
	public class Tractometry : Voxels {
		public MeshFilter tractogramMesh;
		public MeshFilter gridMesh;
		public MeshFilter coreMesh;
		public MeshFilter coreOutlineMesh;
		public MeshFilter spanMesh;
		public MeshFilter spanOutlineMesh;
		public MeshFilter cutMesh;
		public MeshFilter volumeMesh;
		public MeshFilter minimumRadius;
		public MeshFilter startRadius;
		public MeshFilter endRadius;
		public GameObject dot;

		private Tck tractogram;
		private ThreadedLattice grid;
		private new ThreadedRenderer renderer;
		private CrossSection cuts;
		private CrossSectionExtrema prominentCuts;
		private Tract core;
		private Summary summary;

		private TractogramRenderer wireRenderer;
		private TractogramRenderer outlineRenderer;

		private ConcurrentPipe<Tuple<Cell, Tract>> voxels;
		private ConcurrentBag<Dictionary<Cell, Vector>> measurements;
		private ConcurrentBag<Dictionary<Cell, Color32>> colors;
		private ConcurrentPipe<Model> maps;
		private ConcurrentPipe<float> voxelSurfaces;
		private ConcurrentPipe<float> voxelVolumes;

		private Thread quantizeThread;
		private Thread renderThread;
		
		private PromiseCollector<Tract> promisedCore;
		private PromiseCollector<Model> promisedCut;
		private PromiseCollector<Hull> promisedVolume;
		private PromiseCollector<Triple<Tuple<Vector3, Vector3, Topology>>> promisedDiameters;

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

		private const int TUBE_VERTICES = 16;
		private const float TUBE_RADIUS = 0.5f;

		protected override void New(string path) {
			voxels = new ConcurrentPipe<Tuple<Cell, Tract>>();
			measurements = new ConcurrentBag<Dictionary<Cell, Vector>>();
			colors = new ConcurrentBag<Dictionary<Cell, Color32>>();
			maps = new ConcurrentPipe<Model>();
			summary = new Summary();

			wireRenderer = new WireframeRenderer();
			outlineRenderer = new TubeRenderer(TUBE_VERTICES, TUBE_RADIUS, (_, normal, _) => new Color(Math.Abs(normal.x), Math.Abs(normal.z), Math.Abs(normal.y)));

			promisedCore = new PromiseCollector<Tract>();
			promisedCut = new PromiseCollector<Model>();
			promisedVolume = new PromiseCollector<Hull>();
			promisedDiameters = new PromiseCollector<Triple<Tuple<Vector3, Vector3, Topology>>>();

			exportMap = new Files.Exporter("Save as NIFTI", "nii", Nifti);
			exportCore = new Files.Exporter("Save as TCK", "tck", () => new SimpleTractogram(new[] {core}));
			exportSummary = new Files.Exporter("Save numeric summary", "json", summary.Json);

			tractogram = Tck.Load(path);
			evaluation = new TractEvaluation(new CompoundMetric(new TractMetric[] {new Density()}), new Grayscale());
			
			loading = new Any(new Boolean[] {maps, promisedCore, promisedCut, promisedVolume, promisedDiameters});
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
				
				// A bit of a hack checking for this here, as these evaluations also make some sense before the entire map is done
				if (maps.IsCompleted) {
					UpdateMapEvaluation();
				}
			}

			if (promisedCore.TryTake(out var tract)) {
				core = tract;
				var span = new ArrayTract(new[] {tract.Points[0], tract.Points[^1]});
				
				coreMesh.mesh = wireRenderer.Render(tract).Mesh();
				spanMesh.mesh = wireRenderer.Render(span).Mesh();
				coreOutlineMesh.mesh = outlineRenderer.Render(tract).Mesh();
				spanOutlineMesh.mesh = outlineRenderer.Render(span).Mesh();
			}
			if (promisedCut.TryTake(out var cuts)) {
				cutMesh.mesh = cuts.Mesh();
			}
			if (promisedVolume.TryTake(out var hull)) {
				volumeMesh.mesh = hull.Mesh();
				summary.Volume(hull);
			}
			if (promisedDiameters.TryTake(out var diameters)) {
				minimumRadius.transform.position = diameters.Item1.Item1;
				minimumRadius.transform.rotation = Quaternion.LookRotation(diameters.Item1.Item2);
				minimumRadius.mesh = diameters.Item1.Item3.Mesh();
				startRadius.transform.position = diameters.Item2.Item1;
				startRadius.mesh = diameters.Item2.Item3.Mesh();
				endRadius.transform.position = diameters.Item3.Item1;
				endRadius.mesh = diameters.Item3.Item3.Mesh();
			}
		}
		private void UpdateTracts() {
			tractogramMesh.mesh = new WireframeRenderer().Render(tractogram).Mesh();
		}
		private void UpdateSamples() {
			var sampler = new Resample(tractogram, samples);
			var core = new Core(sampler);
			
			cuts = new CrossSection(sampler, core);
			prominentCuts = new CrossSectionExtrema(cuts, prominence);
			
			var volume = new Volume(sampler, cuts);

			promisedCore.Add(core);
			promisedVolume.Add(volume);
			core.Request(summary.Core);
			cuts.Request(summary.CrossSections);
			// TODO: Some thing to add endpoint radius to the summary
			// cut.Request(cuts => core.Request(tract => summary.CrossSectionsVolume(tract, cuts)));
			UpdateDiameterEvaluation();
			UpdateCutEvaluation();
		}
		private void UpdateSamples(int samples) {
			this.samples = samples;
			UpdateSamples();
		}
		private void UpdateSamples(float samples) {
			UpdateSamples((int) Math.Round(samples));
		}
		private void UpdateDiameterEvaluation() {
			var bottleneck = new Bottleneck(cuts);
			var endpoints = new Endpoints(cuts);
			promisedDiameters.Add(new DiameterEvaluation(bottleneck, endpoints, color => new TubeRenderer(TUBE_VERTICES, TUBE_RADIUS, color), evaluation.Coloring));
		}
		private void UpdateCutProminence(float prominence) {
			this.prominence = prominence;
			prominentCuts.UpdateProminence(prominence);
			UpdateCutEvaluation();
		}
		private void UpdateCutEvaluation() {
			promisedCut.Add(new CrossSectionEvaluation(prominentCuts, evaluation.Coloring));
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
		private void UpdateMapEvaluation() {
			new VoxelSurface(grid.Cells, grid.Size, grid.Resolution).Request(surface => summary.SurfaceVoxels = surface);
			new VoxelVolume(grid.Cells, grid.Resolution).Request(volume => summary.VolumeVoxels = volume);
			UpdateDiameterEvaluation();
			UpdateCutEvaluation();
		}

		public override Map Map() {
			return map;
		}
		
		public override IEnumerable<Interface.Component> Controls() {
			return new Interface.Component[] {
				new ActionToggle.Data("Tracts", true, tractogramMesh.gameObject.SetActive),
				new Divider.Data(),
				new Folder.Data("Global measuring", new List<Interface.Component> {
					new TransformedSlider.Exponential("Resample count", 2, 5, 1, 8, (_, transformed) => ((int) Math.Round(transformed)).ToString(), new ValueChangeBuffer<float>(0.1f, UpdateSamples).Request),
					new Loader.Data(promisedCore, new ActionToggle.Data("Mean", true, state => {
							coreMesh.gameObject.SetActive(state);
							coreOutlineMesh.gameObject.SetActive(state);
					})),
					new Loader.Data(promisedCore, new ActionToggle.Data("Span", false, state => {
						spanMesh.gameObject.SetActive(state);
						spanOutlineMesh.gameObject.SetActive(state);
					})),
					new Loader.Data(promisedDiameters, new ActionToggle.Data("Minimum diameter", false, minimumRadius.gameObject.SetActive)),
					new Loader.Data(promisedDiameters, new ActionToggle.Data("Endpoint diameter", false, state => {
						startRadius.gameObject.SetActive(state);
						endRadius.gameObject.SetActive(state);
					})),
					new Loader.Data(promisedCut, new ActionToggle.Data("Cross-section", false, cutMesh.gameObject.SetActive)),
					new TransformedSlider.Data("Cross-section prominence", 0, value => value, (_, transformed) => ((int) Math.Round(transformed * 100)).ToString() + '%', new ValueChangeBuffer<float>(0.1f, UpdateCutProminence).Request),
					new Loader.Data(promisedVolume, new ActionToggle.Data("Volume", false, volumeMesh.gameObject.SetActive))
				}),
				new Divider.Data(),
				new Folder.Data("Local measuring", new List<Interface.Component> {
					new Loader.Data(maps, new ActionToggle.Data("Map", true, gridMesh.gameObject.SetActive)),
					new TransformedSlider.Exponential("Resolution", 10, 0, -1, 1, new ValueChangeBuffer<float>(0.1f, UpdateResolution).Request),
					new TransformedSlider.Exponential("Batch size", 2, 12, 1, 20, (_, transformed) => ((int) Math.Round(transformed)).ToString(), UpdateBatch),
				}),
				new Divider.Data(),
				new Folder.Data("Evaluation", new List<Interface.Component> {
					new Interface.Control.Evaluation.Data(UpdateEvaluation)
				}),
				new Divider.Data(),
				new Folder.Data("Exporting", new List<Interface.Component> {
					new Exporter.Data("Numerical summary", exportSummary),
					new Exporter.Data("Core tract", exportCore),
					new Exporter.Data("Map", exportMap)
				}),
			};
		}
		public override IEnumerable<Files.Publisher> Exports() {
			return new Publisher[] {exportMap, exportCore, exportSummary};
		}
		
		private Nii<float> Nifti() {
			// TODO: This uses only the grid's properties, so the grid should probably know by itself how to perform this formatting
			return new Nii<float>(ToArray(grid.Cells, measurement, 0), grid.Size, new Affine(new Vector3(grid.Resolution, grid.Resolution, grid.Resolution), new Vector3(grid.Resolution / 2, grid.Resolution / 2, grid.Resolution / 2) + grid.Boundaries.Min, true), new Vector3(grid.Resolution, grid.Resolution, grid.Resolution));
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