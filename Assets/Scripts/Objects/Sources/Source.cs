using System;
using System.Collections.Generic;
using Camera;
using Files.Publication;
using Geometry;
using Maps.Cells;
using SFB;
using UnityEngine;

namespace Objects.Sources {
	public abstract class Source : MonoBehaviour {
		public Interface.Source panel;
		public Type[] types;
		
		[Serializable]
		public struct Type {
			public string extension;
			public Voxels template;
		}

		private ProminentPath path;
		private readonly Dictionary<string, Voxels> templates = new();
		protected Voxels instance;
		
		protected void LoadTemplates() {
			foreach (var type in types) {
				templates.Add(type.extension, type.template);
			}
		}
		protected void Load(string path) {
			this.path = new ProminentPath(path.Replace("\\", "/"));
			if (!templates.ContainsKey(this.path.Extension())) {
				throw new ArgumentException("No visualization for source type "+this.path.Extension());
			}
			
			instance = templates[this.path.Extension()].Construct(path);
			instance.Focused += focus => Focused?.Invoke(focus);
			instance.Loaded += Load;
			instance.Configured += (cells, values, resolution, boundaries) => Configured?.Invoke(cells, values, resolution, boundaries);

			panel.UpdateTitle(this.path.Prominence());
			panel.UpdateControls(instance.Controls());
			panel.Sliced += Slice;
			panel.Closed += Close;

			foreach (var export in instance.Exports()) {
				export.Published += Write;
			}
		}
		private void Write(Publication file, string description, string type) {
			file.Write(StandaloneFileBrowser.SaveFilePanel(description, "", path.Name(), type));
		}
		
		public void UpdateContext(Source source) {
			path.UpdateContext(source.path);
			panel.UpdateTitle(path.Prominence());
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
		protected void Slice() {
			Sliced?.Invoke(this, instance.Map());
		}

		public delegate void SourceClosedEvent(Source source);
		public event SourceClosedEvent Closed;
		protected void Close() {
			Destroy(instance.gameObject);
			Destroy(gameObject);
			Closed?.Invoke(this);
		}

		public delegate void SourceLoadedEvent(bool loaded);
		public event SourceLoadedEvent Loaded;
		private void Load(bool loaded) {
			panel.UpdateLoaded(loaded);
			Loaded?.Invoke(loaded);
		}
	}
}