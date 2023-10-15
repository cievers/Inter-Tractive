using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Camera;
using Files.Publication;
using Geometry;
using Maps.Cells;
using SFB;
using UnityEngine;

namespace Objects.Sources {
	public class Source : MonoBehaviour {
		public Interface.Source panel;
		public Type[] types;
		
		[Serializable]
		public struct Type {
			public string extension;
			public Voxels template;
		}

		private string path;
		private bool loaded;
		private readonly List<Source> context = new();
		private readonly Dictionary<string, Voxels> templates = new();
		private Voxels instance;
		
		public void Awake() {
			foreach (var type in types) {
				templates.Add(type.extension, type.template);
			}
			Browse();
		}
		private void Browse() {
			var selection = StandaloneFileBrowser.OpenFilePanel("Tract files", "", "tck", false);
			switch (selection.Length) {
				case 1:
					Load(selection[0]);
					break;
				case > 1:
					throw new InvalidDataException("Please select only one file at a time");
			}
		}
		private void Load(string path) {
			this.path = path.Replace("\\", "/");
			if (!templates.ContainsKey(Extension())) {
				throw new ArgumentException("No visualization for source type "+Extension());
			}
			
			instance = templates[Extension()].Construct(path);
			instance.Focused += focus => Focused?.Invoke(focus);
			instance.Loaded += panel.UpdateLoaded;
			instance.Configured += (cells, values, resolution, boundaries) => Configured?.Invoke(cells, values, resolution, boundaries);

			panel.UpdateTitle(Prominence());
			panel.UpdateControls(instance.Controls());
			panel.Sliced += Slice;
			panel.Closed += Close;
			
			foreach (var export in instance.Exports()) {
				export.Published += Write;
			}
		}
		private void Write(Publication file, string description, string type) {
			file.Write(StandaloneFileBrowser.SaveFilePanel(description, "", Name(), type));
		}
		
		public void UpdateContext(Source source) {
			context.Add(source);
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
		private string Name() {
			return Significance(1).Split(".")[0];
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