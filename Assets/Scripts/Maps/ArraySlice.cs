using System;
using System.Collections.Generic;
using Geometry;
using Geometry.Generators;
using Maps.Cells;
using Maps.Grids;
using UnityEngine;
using Grid = Maps.Grids.Grid;

namespace Maps {
	public class ArraySlice : MonoBehaviour {
		public MeshFilter meshFilter;
		public MeshRenderer meshRenderer;
		public Transform image;
		public Axis axis = Axis.X;
		[Range(0, 1)] public float t = 0.5f;
		
		private Color32[] colors;
		private Index3 resolution;
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

		public void Initialize(Color32[] colors, Index3 resolution, Boundaries boundaries) {
			this.colors = colors;
			this.resolution = resolution;
			this.boundaries = boundaries;
			
			UpdateTexture();
			UpdateTranslation();
			UpdateScale();
			UpdateRotation();

			loaded = true;
		}

		private int Plane() {
			return Math.Min((int) (axis.Select(resolution) * t), axis.Select(resolution) - 1);
		}
		public Texture2D Sample() {
			var width = axis.Previous().Select(resolution);
			var height = axis.Next().Select(resolution);
			currentW = Plane();
			
			var texture = new Texture2D(width, height) {filterMode = FilterMode.Point};

			for (var u = 0; u < width; u++) {
				for (var v = 0; v < height; v++) {
					texture.SetPixels32(u, v, 1, 1, new[] {colors[axis.Compose(resolution, u, v, currentW)]});
				}
			}
			texture.Apply();

			return texture;
		}
		
		public delegate void DrawEvent(Texture2D texture);
		public event DrawEvent Draw;
	}
}