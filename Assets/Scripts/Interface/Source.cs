using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Camera;
using Maps.Grids;
using Objects;
using Objects.Sources;
using TMPro;
using UnityEngine;

using AnotherFileBrowser.Windows;
using Geometry;
using Interface.Control;
using Maps.Cells;

namespace Interface {
	public class Source : MonoBehaviour {
		public TextMeshProUGUI text;
		public GameObject controls;
		public Type[] types;
		public ActionToggle toggle;
		public Slider slider;
		public DelayedSlider delayedSlider;
		public Stepper stepper;
		
		[Serializable]
		public struct Type {
			public string extension;
			public SourceInstance template;
		}

		private string path;
		private readonly List<Source> context = new();
		private readonly Dictionary<string, SourceInstance> templates = new();
		private SourceInstance instance;
		
		public void Awake() {
			foreach (var type in types) {
				templates.Add(type.extension, type.template);
			}
			Browse();
			
			// If the file selection was abandoned, remove the UI panel that was already created
			if (path == null) {
				DestroyImmediate(gameObject);
			}
		}
		private void Browse() {
			var properties = new BrowserProperties {filter = "Tract files (*.tck) | *.tck", filterIndex = 0};
			new FileBrowser().OpenFileBrowser(properties, Load);
		}
		private void Load(string path) {
			this.path = path.Replace("\\", "/");
			try {
				instance = templates[Extension()].Construct(path);
			} catch (Exception exception) {
				Debug.Log("Exception during loading of tck instance");
				Debug.Log(exception);
				DestroyImmediate(gameObject);
				return;
			}

			instance.Focused += focus => Focused?.Invoke(focus);
			instance.Configured += (cells, values, resolution, boundaries) => Configured?.Invoke(cells, values, resolution, boundaries);
			UpdateName();

			foreach (var data in instance.Controls()) {
				switch (data) {
					case Control.Data.Toggle toggleData:
						toggle.Construct(controls.transform, toggleData);
						break;
					case Control.Data.DelayedSlider delayedSliderData:
						delayedSlider.Construct(controls.transform, delayedSliderData);
						break;
					case Control.Data.Slider sliderData:
						slider.Construct(controls.transform, sliderData);
						break;
					case Control.Data.Stepper stepperData:
						stepper.Construct(controls.transform, stepperData);
						break;
				}
			}
		}
		public void UpdateContext(Source source) {
			context.Add(source);
		}
		private void UpdateName() {
			text.text = Prominence();
		}

		private string Prominence() {
			for (var i = 1; i < Significance(); i++) {
				var display = Significance(i);
				if (context.Any(source => source.Significance(i) == display)) {
					continue;
				}
				return display;
			}
			return Significance(Significance());
		}
		private int Significance() {
			return path.Split("/").Length;
		}
		private string Significance(int length) {
			var result = "";
			var split = path.Split("/");
			for (var i = 0; i < split.Length && i < length; i++) {
				result = split[^(i+1)] + "/" + result;
			}
			return result.TrimEnd('/');
		}
		private string Extension() {
			return path.Split(".")[^1];
		}

		public delegate void SourceFocusedEvent(Focus focus);
		public event SourceFocusedEvent Focused;
		
		public void Focus() {
			instance.Focus();
		}
		
		public delegate void SourceConfiguredEvent(IReadOnlyList<Cuboid?> cells, IReadOnlyDictionary<Cell, Color32> values, Index3 resolution, Boundaries boundaries);
		public event SourceConfiguredEvent Configured;

		public delegate void SourceSlicedEvent(Source source, Map map);
		public event SourceSlicedEvent Sliced;
		public void Slice() {
			Sliced?.Invoke(this, instance.Map());
		}

		public delegate void SourceClosedEvent(Source source);
		public event SourceClosedEvent Closed;
		public void Close() {
			Destroy(instance.gameObject);
			Destroy(gameObject);
			Closed?.Invoke(this);
		}
	}
}