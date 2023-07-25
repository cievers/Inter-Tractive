using System;
using System.Collections.Generic;
using System.IO;
using Camera;
using UnityEngine;

namespace Interface {
	public class SourceManager : MonoBehaviour {
		public GameObject template;
		public SliceManager slices;
		public new OrbitingCamera camera;

		private readonly List<Source> sources = new();

		private void Start() {
			Add();
		}
		public void Add() {
			try {
				var instance = Instantiate(template, transform);
				var source = instance.GetComponent<Source>();

				sources.Add(source);

				source.Focused += camera.Target;
				source.Sliced += slices.Slice;
				source.Closed += x => sources.Remove(x);
				source.Closed += x => slices.Remove(x);
				
				if (sources.Count == 1) {
					source.Focus();
				}
			} catch (UnityException) {
				
			}
		}
	}
}