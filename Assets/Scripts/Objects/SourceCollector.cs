﻿using Camera;
using Objects.Sources;
using UnityEngine;

namespace Objects {
	public class SourceCollector : SourceManager {
		public Source template;
		public new OrbitingCamera camera;
		
		private void Start() {
			Add();
		}
		public void Add() {
			try {
				var source = Instantiate(template, transform);

				Collect(source);
				Interact(source);
				
				source.Focused += camera.Target;
				if (sources.Count == 1) {
					source.Focus();
				}
			} catch (UnityException) {
				
			}
		}
	}
}