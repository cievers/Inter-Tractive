using System.Collections.Generic;
using Camera;
using Files;
using Files.Types;
using Geometry;
using Maps.Cells;
using UnityEngine;

namespace Objects.Sources {
	public abstract class Voxels : MonoBehaviour {
		private Focus focus;
		
		public Voxels Construct(string path) {
			var source = Instantiate(this);
			source.New(path);
			return source;
		}
		protected abstract void New(string path);

		public abstract Map Map();

		public virtual IEnumerable<Interface.Component> Controls() => new List<Interface.Component>();
		public virtual IEnumerable<Publisher> Exports() => new List<Publisher>();

		public delegate void SourceLoadedEvent(bool loaded);
		public event SourceLoadedEvent Loaded;
		public void Loading(bool done) {
			Loaded?.Invoke(done);
		}

		public delegate void SourceFocusedEvent(Focus focus);
		public event SourceFocusedEvent Focused;
		public void Focus(Focus focus) {
			this.focus = focus;
			Focus();
		}
		public void Focus() {
			Focused?.Invoke(focus);
		}
		
		public delegate void SourceConfiguredEvent(IReadOnlyList<Cuboid?> cells, IReadOnlyDictionary<Cell, Color32> values, Index3 resolution, Boundaries boundaries);
		public event SourceConfiguredEvent Configured;
		public void Configure(IReadOnlyList<Cuboid?> cells, IReadOnlyDictionary<Cell, Color32> values, Index3 resolution, Boundaries boundaries) {
			Configured?.Invoke(cells, values, resolution, boundaries);
		}
	}
}