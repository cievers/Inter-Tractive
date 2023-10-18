using System;
using Files;
using Files.Types;
using Geometry;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Interface {
	public class Projection : MonoBehaviour {
		public Image image;
		public AspectRatioFitter fitter;
		public TMP_Dropdown selector;
		public Slider slider;

		private float t;
		private Axis axis;

		private void Start() {
			selector.onValueChanged.AddListener(Select);
			slider.onValueChanged.AddListener(Slide);
		}
		public void Project(Texture texture) {
			image.sprite = Sprite.Create(texture as Texture2D, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
			fitter.aspectRatio = (float) texture.width / texture.height;
		}
		public void Capture() {
			Artifact.Write(new Png(image.sprite.texture), "png");
		}
		private void Select(int option) {
			axis = option switch {
				0 => Axis.X,
				1 => Axis.Z,
				2 => Axis.Y,
				_ => throw new ArgumentOutOfRangeException(nameof(option), "There are only three axis to select from")
			};
			Invoke();
		}
		private void Slide(float value) {
			t = value;
			Invoke();
		}
		private void Invoke() {
			Update?.Invoke(axis, t);
		}
		public void Close() {
			Closed?.Invoke();
		}

		public delegate void ProjectionUpdate(Axis axis, float t);
		public event ProjectionUpdate Update;
		
		public delegate void ProjectionClose();
		public event ProjectionClose Closed;
	}
}