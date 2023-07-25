﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Geometry.Tracts {
	public readonly struct ArrayTract : Tract {
		public readonly uint Id;
		public readonly uint GlobalLinePointIndexFirstPoint;
		public readonly Vector3[] points;
		public readonly Vector3 AverageDirection;

		public ArrayTract(Vector3[] points, uint id, Vector3 averageDirection, uint globalLinePointIndexFirstPoint) {
			this.points = points;
			Id = id;
			AverageDirection = averageDirection;
			GlobalLinePointIndexFirstPoint = globalLinePointIndexFirstPoint;
		}
		public IEnumerable<Segment> Segments {
			get {
				var result = new List<Segment> { new(points[0], points[1]) };

				for (var i = 1; i < points.Length; i++) {
					result.Add(new Segment(points[i - 1], points[i]));
				}

				return result;
			}
		}
		public Vector3[] Points => points;
		public Boundaries Boundaries => Boundaries.Join(Segments.Select(segment => segment.Boundaries));
		public float Resolution => Segments.Select(segment => segment.Size.magnitude).Min();
		public float Slack => Segments.Select(segment => segment.Size.magnitude).Max();
	}
}