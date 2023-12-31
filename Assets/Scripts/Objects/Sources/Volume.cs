﻿using System.Collections.Generic;
using System.Linq;
using Camera;
using Evaluation;
using Evaluation.Coloring;
using Evaluation.Coloring.Gradients;
using Files.Types;
using Geometry;
using Geometry.Generators;
using Geometry.Topology;
using Interface.Control;
using Maps.Cells;
using UnityEngine;

namespace Objects.Sources {
	public class Volume : Voxels {
		public MeshFilter mesh;

		private float threshold = 0f;
		private Nii<float> volume;
		private Dictionary<Cuboid, Vector> voxels;
		private Coloring coloring;
		private Map map;

		protected override void New(string path) {
			coloring = new Grayscale();
			volume = Nii<float>.Load(path);
			UpdateMap();
			Focus(new Focus(map.Boundaries.Center, map.Boundaries.Size.magnitude / 2 * 1.5f));
		}
		private void Start() {
			Loading(true);
		}
		private void UpdateMap() {
			var cells = new Cuboid?[volume.Values.Length];
			var voxels = new Dictionary<Cuboid, Vector>();
			var pivot = volume.Unit / 2;

			for (var x = 0; x < volume.Composition.x; x++) {
				for (var y = 0; y < volume.Composition.y; y++) {
					for (var z = 0; z < volume.Composition.z; z++) {
						var value = volume.Values[x + y * volume.Composition.x + z * volume.Composition.x * volume.Composition.y];

						if (value > threshold) {
							var anchor = new Vector3(x * volume.Unit.x, y * volume.Unit.y, z * volume.Unit.z);
							var voxel = new Cuboid(anchor - pivot + volume.Offset, volume.Unit);
							voxels[voxel] = new Vector(value);
							cells[x + y * volume.Composition.x + z * volume.Composition.x * volume.Composition.y] = voxel;
						}
					}
				}
			}

			map = new Map(
				coloring
					.Color(voxels)
					.ToDictionary(pair => (Cell) pair.Key, pair => pair.Value), 
				cells, 
				volume.Composition, 
				new Boundaries(
					volume.Offset - pivot,
					volume.Offset + new Vector3(volume.Composition.x * volume.Unit.x, volume.Composition.y * volume.Unit.y, volume.Composition.z * volume.Unit.z) - pivot
				)
			);

			var vertices = new List<Vector3>();
			var colors = new List<Color32>();
			var indices = new List<int>();

			foreach (var pair in coloring.Color(voxels)) {
				indices.AddRange(pair.Key.Indices.Select(index => index + vertices.Count));
				vertices.AddRange(pair.Key.Vertices);
				colors.AddRange(pair.Key.Vertices.Select(_ => pair.Value));
			}

			mesh.mesh = new Model(vertices.ToArray(), new Vector3[]{}, colors.ToArray(), indices.ToArray()).Mesh();
			
			// mesh.mesh = new GridRenderer(0).Render(volume, coloring).Mesh();
		}
		private void UpdateEvaluation(Coloring coloring) {
			this.coloring = coloring;
			UpdateMap();
		}
		
		public override IEnumerable<Interface.Component> Controls() {
			return new Interface.Component[] {
				new ActionToggle.Data("Map", true, mesh.gameObject.SetActive),
				new Interface.Control.Data.Coloring(new Dictionary<string, Coloring> {
					{"Grayscale", new Grayscale()},
					{"Temperature", new Temperature()},
					{"Plasma", new Plasma()},
					{"Viridis", new Viridis()}
				}, UpdateEvaluation)
			};
		}
		
		public override Map Map() {
			return map;
		}
	}
}