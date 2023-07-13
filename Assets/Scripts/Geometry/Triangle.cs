using System;
using UnityEngine;

namespace Geometry {
	public readonly struct Triangle {
		public Vector3 A {get;}
		public Vector3 B {get;}
		public Vector3 C {get;}

		public Triangle(Vector3 a, Vector3 b, Vector3 c) {
			A = a;
			B = b;
			C = c;
		}
		public Vector3 Normal => Vector3.Cross(B - A, C - A);

		public bool Intersects(Segment segment) {
			var signStart = PlaneSide(segment.Start, A, B, C);
			var signEnd = PlaneSide(segment.End, A, B, C);
			if (signStart == signEnd) {
				return false;
			}
			var signA = PlaneSide(segment.Start, segment.End, A, B);
			var signB = PlaneSide(segment.Start, segment.End, B, C);
			var signC = PlaneSide(segment.Start, segment.End, C, A);
			return signA == signB && signA == signC;
		}
		private static int PlaneSide(Vector3 a, Vector3 b, Vector3 c, Vector3 d) {
			// This could be a method function of a plane class, but that would (for now) require making a plane class and instances just for this
			return Math.Sign(Vector3.Dot(Vector3.Cross(b - a, c - a), d - a));
		}
	}
}