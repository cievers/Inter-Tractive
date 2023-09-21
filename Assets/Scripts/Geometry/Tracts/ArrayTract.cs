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
		public ArrayTract(Vector3[] points) : this(points, 0, Vector3.zero, 0) {}
		public IEnumerable<Segment> Segments {
			get {
				// var result = new List<Segment> { new(points[0], points[1]) };
				var result = new List<Segment>();

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

		public Tract Sample(int samples) {
			var result = new Vector3[samples];
			var segments = Segments.ToArray();
			var length = segments.Sum(segment => segment.Size.magnitude);
			var interval = length / (samples - 1);

			var traversed = 0f;
			var segment = 0;
			var size = segments[segment].Size;
			for (var i = 0; i < samples; i++) {
				while (i * interval > traversed + size.magnitude && segments.Length > segment + 1) {
					traversed += size.magnitude;
					segment++;
					size = segments[segment].Size;
				}
				result[i] = segments[segment].Start + size * ((float) (i * interval - traversed) / size.magnitude);
			}

			return new ArrayTract(result, 0, Vector3.zero, 0);
		}
	}
}