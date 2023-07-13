using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Geometry.Tracts {
	public readonly struct Tract {
		public readonly uint Id;
		public readonly uint GlobalLinePointIndexFirstPoint;
		public readonly Vector3[] Points;
		public readonly Vector3 AverageDirection;

		public Tract(Vector3[] points, uint id, Vector3 averageDirection, uint globalLinePointIndexFirstPoint) {
			Points = points;
			Id = id;
			AverageDirection = averageDirection;
			GlobalLinePointIndexFirstPoint = globalLinePointIndexFirstPoint;
		}
		public IEnumerable<Segment> Segments {
			get {
				var result = new List<Segment> { new(Points[0], Points[1]) };

				for (var i = 1; i < Points.Length; i++) {
					result.Add(new Segment(Points[i - 1], Points[i]));
				}

				return result;
			}
		}
		public Boundaries Boundaries => Boundaries.Join(Segments.Select(segment => segment.Boundaries));
		public float Resolution => Segments.Select(segment => segment.Size.magnitude).Min();
		public float Slack => Segments.Select(segment => segment.Size.magnitude).Max();
	}
}