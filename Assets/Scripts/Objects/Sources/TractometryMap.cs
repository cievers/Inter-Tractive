using System;
using System.Collections.Generic;
using Camera;
using Evaluation.Coloring;
using Evaluation.Geometric;
using Files.Types;
using Geometry;
using Geometry.Generators;
using Geometry.Tracts;
using Interface.Control;
using Maps.Cells;
using Maps.Grids;
using UnityEngine;

namespace Objects.Sources {
	public class TractometryMap : Voxels {
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
			Loading(true);
		}

		private void UpdateTracts() {
			tractogramMesh.mesh = new WireframeRenderer().Render(tractogram).Mesh();
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
			
			Configure(grid.Cells, colors, grid.Size, new AxisOrder(Axis.X, Axis.Y, Axis.Z), grid.Boundaries);
			map = new Map(colors, grid.Cells, grid.Size, new AxisOrder(Axis.X, Axis.Y, Axis.Z), grid.Boundaries);
		}
		
		public override Map Map() {
			return map;
		}

		public override IEnumerable<Interface.Component> Controls() {
			return new Interface.Component[] {
				new ActionToggle.Data("Tracts", true, tractogramMesh.gameObject.SetActive),
				new ActionToggle.Data("Map", true, gridMesh.gameObject.SetActive),
				new Slider.Data("Resolution", 1, 0.1f, 10, UpdateVoxels)
			};
		}
	}
}