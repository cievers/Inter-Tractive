using System.Collections.Generic;
using Objects.Sources;
using UnityEngine;

namespace Objects {
	public abstract class SourceManager : MonoBehaviour {
		public ProjectionManager slices;

		protected readonly List<Source> sources = new();
		
		protected void Collect(Source source) {
			sources.ForEach(source.UpdateContext);
			sources.Add(source);
			sources.ForEach(s => s.UpdateContext(source));
		}
		protected void Interact(Source source) {
			source.Sliced += slices.Slice;
			source.Closed += x => sources.Remove(x);
			source.Closed += x => slices.Remove(x);
			source.Closed += x => sources.ForEach(s => s.UpdateContext(x));
		}
	}
}