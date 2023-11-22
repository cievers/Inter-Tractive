using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Geometry {
	public class Walk {
		public Vector3[] Points {get;}
		public IEnumerable<Vector3> Normals {
			get {
				var result = new Vector3[Points.Length];

				result[0] = Points[1] - Points[^1];
				for (var i = 1; i < Points.Length - 1; i++) {
					result[i] = Points[i + 1] - Points[i - 1];
				}
				result[^1] = Points[0] - Points[^2];

				return result.Select(normal => normal.normalized);
			}
		}
		public IEnumerable<Segment> Segments {
			get {
				var result = new Segment[Points.Length];

				for (var i = 1; i < Points.Length; i++) {
					result[i - 1] = new Segment(Points[i - 1], Points[i]);
				}
				result[^1] = new Segment(Points[^1], Points[0]);

				return result;
			}
		}
		public Edge Skewer => Points
			.SelectMany(u => Points.Select(v => new Edge(u, v)))
			.ToHashSet()
			.ToDictionary(s=> s, s => s.Size.magnitude)
			.Aggregate((x, y) => x.Value > y.Value ? x : y)
			.Key;

		public Walk(Vector3[] points) {
			Points = points;
		}
	}
}