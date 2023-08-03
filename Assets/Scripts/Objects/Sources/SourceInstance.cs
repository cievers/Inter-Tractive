using System.Collections.Generic;
using Camera;
using Files.Types;
using Geometry;
using Interface.Control.Data;
using Maps.Cells;
using Maps.Grids;
using UnityEngine;

namespace Objects.Sources {
	public abstract class SourceInstance : MonoBehaviour {
		private Focus focus;
		
		public SourceInstance Construct(string path) {
			var instance = Instantiate(gameObject);
			var source = instance.GetComponent<SourceInstance>();
			source.New(path);
			return source;
		}
		protected abstract void New(string path);

		public abstract Map Map();
		public abstract Nii<float> Nifti();

		public abstract IEnumerable<Configuration> Controls();
		
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