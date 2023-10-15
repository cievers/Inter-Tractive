﻿using System.Collections.Generic;
using System.Linq;
using Camera;
using UnityEngine;
using Source = Objects.Sources.Source;

namespace Objects {
	public class SourceManager : MonoBehaviour {
		public GameObject template;
		public ProjectionManager slices;
		public new OrbitingCamera camera;

		private readonly List<Source> sources = new();

		private void Start() {
			Add();
		}
		public void Add() {
			try {
				var instance = Instantiate(template, transform);
				var source = instance.GetComponent<Source>();

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