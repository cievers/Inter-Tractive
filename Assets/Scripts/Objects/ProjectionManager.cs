using System.Collections.Generic;
using Geometry;
using Interface;
using Maps;
using UnityEngine;
using Utility;

namespace Objects {
	public class ProjectionManager : MonoBehaviour {
		public GameObject templateSlice;
		public GameObject templateProjection;
		private readonly Dictionary<Objects.Sources.Source, List<ArraySlice>> slices = new();
		private readonly Dictionary<ArraySlice, Projection> projections = new();

		public void Slice(Objects.Sources.Source source, Map map) {
			var instance2 = Instantiate(templateProjection, transform);
			var image = instance2.GetComponent<Projection>();
			
			if (!slices.ContainsKey(source)) {
				slices[source] = new List<ArraySlice>();
			}
			var instance = Instantiate(templateSlice, Vector3.zero, Quaternion.identity);
			var slice = instance.GetComponent<ArraySlice>();
			slices[source].Add(slice);
			projections.Add(slice, image);
			image.Update += slice.Update;
			image.Closed += () => Remove(source, slice);
			source.Configured += slice.Initialize;
			slice.Draw += image.Project;
			
			// TODO: Unify with the later updating of a map to slice through
			var colorGrid = Enumeration.ToArray(map.Cells, map.Colors, new Color32(0, 0, 0, 0));
			slice.Initialize(colorGrid, map.Composition, new AxisOrder(Axis.X, Axis.Y, Axis.Z), map.Boundaries);
		}
		public void Remove(Objects.Sources.Source source) {
			if (slices.ContainsKey(source)) {
				foreach (var slice in slices[source]) {
					// Destroy(projections[slice].gameObject);
					// Destroy(slice.gameObject);
					// projections.Remove(slice);
					Remove(source, slice);
				}
				slices.Remove(source);
			}
		}
		public void Remove(Objects.Sources.Source source, ArraySlice slice) {
			Destroy(projections[slice].gameObject);
			Destroy(slice.gameObject);
			projections.Remove(slice);
			source.Configured -= slice.Initialize;
		}
	}
}