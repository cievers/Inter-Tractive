using System;
using System.Collections.Generic;
using Maps.Cells;
using Maps.Grids;
using UnityEngine;

namespace Maps {
	public class Slice : MonoBehaviour {
		private const float PIVOT = 1f / 2;
		
		public int resolution = 16;

		private MeshRenderer meshRenderer;
		private ArrayGrid grid; // TODO: Generalize to interface for grids
		private Dictionary<Cell, Color32> colors;
		private Texture2D texture;
		private Vector3 position;

		private void Start() {
			meshRenderer = gameObject.GetComponent<MeshRenderer>();
			texture = new Texture2D(resolution, resolution) { filterMode = FilterMode.Point };
		}
		private void Update() {
			var current = transform.position;
			if (current != position) {
				meshRenderer.material.mainTexture = Sample();
				position = current;
				// MeasureSample();
				// MeasureTexture();
			}
		}

		public void Initialize(ArrayGrid grid, Dictionary<Cell, Color32> colors) {
			this.grid = grid;
			this.colors = colors;
		}
		public Texture2D Sample() {
			var total = (float)resolution;
			var unit = 1 / total;
			var offset = unit / 2;
			
			
			// texture.SetPixels(0, 0, resolution, resolution, new[] {Color.red});
			
			for (var u = 0; u < resolution; u++) {
				for (var v = 0; v < resolution; v++) {
					try {
						var cell = grid.Quantize(transform.TransformPoint(new Vector3(u / total - PIVOT + offset, v / total - PIVOT + offset)));
						texture.SetPixels32(u, v, 1, 1, new[] {colors[cell]});
					} catch (Exception exception) when (exception is IndexOutOfRangeException or KeyNotFoundException) {
						texture.SetPixels(u, v, 1, 1, new[] { Color.red });
					}
					// texture.SetPixels32(u, v, 1, 1, colors.TryGetValue(cell, out var color) ? new[] {color} : new[] {Color.clear});
					// if (colors.TryGetValue(cell, out var color)) {
					// 	texture.SetPixels32(u, v, 1, 1, new[] { color });
					// } else {
					// 	texture.SetPixels(u, v, 1, 1, new[] { Color.red });
					// }
				}
			}
			texture.Apply();

			return texture;
		}
	}
}