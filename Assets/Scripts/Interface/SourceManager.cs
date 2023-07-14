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
				if (sources.Count == 1) {
					camera.Target(source.Focus());
				}

				source.Sliced += slices.Slice;
				source.Closed += x => sources.Remove(x);
				source.Closed += x => slices.Remove(x);
			} catch (UnityException) {
				
			}
		}
	}
}