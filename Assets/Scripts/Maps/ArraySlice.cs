using System;
using System.Collections.Generic;
using Geometry;
using Geometry.Generators;
using Maps.Cells;
using Maps.Grids;
using UnityEngine;
using Utility;
using Grid = Maps.Grids.Grid;

namespace Maps {
	public class ArraySlice : MonoBehaviour {
		public MeshFilter meshFilter;
		public MeshRenderer meshRenderer;
		public Transform image;
		public Axis axis = Axis.X;
		[Range(0, 1)] public float t = 0.5f;
		
		private Color32[] colors;
		private Index3 composition;
		private AxisOrder order;
		private Boundaries boundaries;
		private bool loaded;

		private Axis currentAxis;
		private float currentT;
		private int currentW;

		private void Start() {
			meshFilter.mesh = Quad.Mesh();
		}
		private void Update() {
			if (loaded) {
				if (currentAxis != axis || currentW != Plane()) {
					UpdateTexture();
				}
				if (currentAxis != axis || currentT != t) {
					UpdateTranslation();
				}
				if (currentAxis != axis) {
					UpdateScale();
					UpdateRotation();
				}
				
				currentAxis = axis;
				currentT = t;
			}
		}
		public void Update(Axis axis, float t) {
			this.axis = axis;
			this.t = t;
		}
		private void UpdateTexture() {
			var texture = Sample();
			meshRenderer.material.mainTexture = texture;
			Draw?.Invoke(texture);
		}
		private void UpdateTranslation() {
			transform.position = axis.Interpolate(boundaries, t);
		}
		private void UpdateRotation() {
			image.rotation = axis.Rotation();
		}
		private void UpdateScale() {
			transform.localScale = boundaries.Size;
		}
		
		public void Initialize(IReadOnlyList<Cuboid?> cells, IReadOnlyDictionary<Cell, Color32> values, Index3 resolution, AxisOrder order, Boundaries boundaries) {
			Initialize(Enumeration.ToArray(cells, values, new Color32(0, 0, 0, 0)), resolution, order, boundaries);
		}
		public void Initialize(Color32[] colors, Index3 composition, AxisOrder order, Boundaries boundaries) {
			this.colors = colors;
			this.composition = composition;
			this.order = order;
			this.boundaries = boundaries;
			
			UpdateTexture();
			UpdateTranslation();
			UpdateScale();
			UpdateRotation();

			loaded = true;
		}

		private int Plane() {
			return Math.Min((int) (axis.Select(composition) * t), axis.Select(composition) - 1);
		}
		public Texture2D Sample() {
			// var width = axis.Previous().Select(composition);
			// var height = axis.Next().Select(composition);
			var width = order.Previous(axis).Select(composition);
			var height = order.Next(axis).Select(composition);
			currentW = Plane();

			// Debug.Log("Sampling a slice");
			// Debug.Log(axis.Previous().Select(composition));
			// Debug.Log(order.Previous(axis).Select(composition));
			// Debug.Log(axis.Next().Select(composition));
			// Debug.Log(order.Next(axis).Select(composition));

			var texture = new Texture2D(axis.Select(new Index3(width, width, height)), axis.Select(new Index3(height, height, width))) {filterMode = FilterMode.Point};

			for (var u = 0; u < width; u++) {
				for (var v = 0; v < height; v++) {
					// texture.SetPixels32(axis.Select(new Index3(u, u, v)), axis.Select(new Index3(v, v, u)), 1, 1, new[] {colors[axis.Compose(composition, u, v, currentW)]});
					texture.SetPixels32(axis.Select(new Index3(u, u, v)), axis.Select(new Index3(v, v, u)), 1, 1, new[] {colors[order.Compose(axis, composition, u, v, currentW)]});
				}
			}
			texture.Apply();

			return texture;
		}
		
		public delegate void DrawEvent(Texture2D texture);
		public event DrawEvent Draw;
	}
}