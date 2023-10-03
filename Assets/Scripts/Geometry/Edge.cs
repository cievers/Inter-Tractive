using System;
using UnityEngine;

namespace Geometry {
	public readonly struct Edge {
		public Vector3 A {get;}
		public Vector3 B {get;}
		public Vector3 Size => B - A;

		public Edge(Vector3 a, Vector3 b) {
			// While Segment preserves order, Edge aims to create a non-directional, unambiguous representation of a
			// pair of points by storing them in a defined order (sort by x, y, z)
			if (a.x < b.x) {
				A = a;
				B = b;
			} else if (a.x > b.x) {
				A = b;
				B = a;
			} else {
				if (a.y < b.y) {
					A = a;
					B = b;
				} else if (a.y > b.y) {
					A = b;
					B = a;
				} else {
					if (a.z < b.z) {
						A = a;
						B = b;
					} else {
						A = b;
						B = a;
					}
				}
			}
		}
		public Boundaries Boundaries => new(new Vector3(Math.Min(A.x, B.x), Math.Min(A.y, B.y), Math.Min(A.z, B.z)), new Vector3(Math.Max(A.x, B.x), Math.Max(A.y, B.y), Math.Max(A.z, B.z)));
	}
}