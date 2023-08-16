using System;
using System.Collections.Generic;
using Camera;
using Evaluation.Coloring;
using Evaluation.Geometric;
using Files.Types;
using Geometry.Generators;
using Geometry.Tracts;
using Interface.Control;
using Maps.Cells;
using Maps.Grids;
using UnityEngine;

namespace Objects.Sources {
	public class TractometryMap : Voxels {
		private const byte COLORIZE_TRANSPARENCY = 200;
		
		public MeshFilter tractogramMesh;
		public MeshFilter gridMesh;

		private Tractogram tractogram;
		private Coloring coloring;
		private Dictionary<Cell, IEnumerable<Tract>> voxels;
		private Map map;

		protected override void New(string path) {
			tractogram = Tck.Load(path);
			coloring = new Grayscale();

			UpdateTracts();
			UpdateVoxels(1);
		}

		private void UpdateTracts() {
			tractogramMesh.mesh = new WireframeRenderer().Render(tractogram);
		}
		private void UpdateVoxels(float resolution) {
			var grid = new IntersectionLattice(tractogram, resolution);
			voxels = grid.Quantize(tractogram);
			
			Focus(new Focus(grid.Boundaries.Center, grid.Boundaries.Size.magnitude / 2 * 1.5f));
			UpdateMap(grid);
		}
		private void UpdateMap(Maps.Grids.Voxels grid) {
			// var measurements = new Density().Measure(voxels);
			var measurements = new Length().Measure(voxels);
			
			var colors = coloring.Color(measurements);
			gridMesh.mesh = grid.Render(colors);
			
			Configure(grid.Cells, colors, grid.Size, grid.Boundaries);
			map = new Map(colors, grid.Cells, grid.Size, grid.Boundaries);
		}
		
		public override Map Map() {
			return map;
		}
		public override Nii<float> Nifti() {
			// var gridBoundaries = grid.Boundaries;
			// return new Nii<float>(ToArray(grid.Cells, measurement, 0), grid.Size, gridBoundaries.Min + new Vector3(grid.CellSize / 2, grid.CellSize / 2, grid.CellSize / 2), new Vector3(grid.CellSize, grid.CellSize, grid.CellSize));
			throw new NotImplementedException();
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

		public override IEnumerable<Interface.Component> Controls() {
			return new Controller[] {
				new ActionToggle.Data("Tracts", true, tractogramMesh.gameObject.SetActive),
				new ActionToggle.Data("Map", true, gridMesh.gameObject.SetActive),
				new DelayedSlider.Data("Resolution", 1, 0.1f, 10, 1, UpdateVoxels)
			};
		}
	}
}