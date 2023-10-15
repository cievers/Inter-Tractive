using System.Collections.Generic;
using Camera;
using Objects.Sources;
using UnityEngine;

namespace Objects {
	public class SourceSequencer : MonoBehaviour {
		public Source template;
		public ProjectionManager slices;
		public new OrbitingCamera camera;

		private readonly List<Source> sources = new();

		private void Start() {
			Add();
		}
		public void Add() {
			try {
				var source = Instantiate(template, transform);

				sources.ForEach(s => source.UpdateContext(s));
				sources.Add(source);
				sources.ForEach(s => s.UpdateContext(source));

				source.Focused += camera.Target;
				source.Sliced += slices.Slice;
				source.Closed += x => sources.Remove(x);
				source.Closed += x => slices.Remove(x);
				source.Closed += x => sources.ForEach(s => s.UpdateContext(x));
				
				if (sources.Count == 1) {
					source.Focus();
				}
			} catch (UnityException) {
				
			}
		}
	}
}