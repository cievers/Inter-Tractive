using System;
using System.Linq;
using Geometry.Tracts;
using UnityEngine;

namespace Evaluation.Geometric {
	public class Roughness : CachedMetric {
		public override int Dimensions => 1;
		public override string[] Units => new[] {""};
		public override float Measure(Tract tract) {
			var segments = tract.Segments.ToArray();
			var sum = 0f;
			for (var i = 1; i < segments.Length; i++) {
				// sum += 1 - Math.Abs(Vector3.Dot(segments[i - 1].Size.normalized, segments[i].Size.normalized));
				sum += (2 - (1 + Vector3.Dot(segments[i - 1].Size.normalized, segments[i].Size.normalized))) / 2;
			}
			return sum / (segments.Length - 1);
		}
	}
}