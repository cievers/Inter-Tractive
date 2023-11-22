using System.Collections.Generic;
using System.Linq;
using Files.Publication;
using Geometry.Topology;
using Maps.Cells;
using UnityEngine;

namespace Geometry.Generators {
	public class GridRenderer : VolumeRenderer {
		private readonly float threshold;
		
		public GridRenderer(float threshold) {
			this.threshold = threshold;
		}
		
		public Topology.Topology Render(Nii<float> volume) {
			var vertices = new List<Vector3>();
			var colors = new List<Color32>();
			var indices = new List<int>();
			var pivot = volume.Size / 2;

			for (var x = 0; x < volume.Composition.x; x++) {
				for (var y = 0; y < volume.Composition.y; y++) {
					for (var z = 0; z < volume.Composition.z; z++) {
						var value = volume.Values[x + y * volume.Composition.x + z * volume.Composition.x * volume.Composition.y];

						if (value > threshold) {
							var anchor = volume.Transformation.Transform(new Vector3(x, y, z));
							var voxel = new Cuboid(new Vector3(anchor.x, anchor.z, anchor.y) - pivot, volume.Size);
							indices.AddRange(voxel.Indices.Select(index => index + vertices.Count));
							vertices.AddRange(voxel.Vertices);
							colors.AddRange(voxel.Vertices.Select(_ => new Color32((byte) value, (byte) value, (byte) value, 255)));
						}
					}
				}
			}

			return new Model(vertices.ToArray(), new Vector3[]{}, colors.ToArray(), indices.ToArray());
		}
	}
}