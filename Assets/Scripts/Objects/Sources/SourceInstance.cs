using System.Collections.Generic;
using Camera;
using Interface.Data;
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

		public abstract IEnumerable<Toggle> Controls();
		public abstract void ConfigureResolution(float value);
	}
}