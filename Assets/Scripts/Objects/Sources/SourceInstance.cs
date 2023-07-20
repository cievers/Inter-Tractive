using System.Collections.Generic;
using Camera;
using Geometry;
using Interface.Control.Data;
using Maps.Cells;
using Maps.Grids;
using UnityEngine;

namespace Objects.Sources {
	public abstract class SourceInstance : MonoBehaviour {
		public SourceInstance Construct(string path) {
			var instance = Instantiate(gameObject);
			var source = instance.GetComponent<SourceInstance>();
			source.New(path);
			return source;
		}
		protected abstract void New(string path);

		public abstract Focus Focus();
		public abstract Map Map();

		public abstract IEnumerable<Configuration> Controls();
		public abstract void ConfigureResolution(float value);
		
		public delegate void SourceConfiguredEvent(IReadOnlyList<Cuboid?> cells, IReadOnlyDictionary<Cell, Color32> values, Index3 resolution, Boundaries boundaries);
		public event SourceConfiguredEvent Configured;
		public void Configure(IReadOnlyList<Cuboid?> cells, IReadOnlyDictionary<Cell, Color32> values, Index3 resolution, Boundaries boundaries) {
			Configured?.Invoke(cells, values, resolution, boundaries);
		}
	}
}