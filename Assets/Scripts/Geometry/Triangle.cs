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

		public static float Area(Vector3 a, Vector3 b, Vector3 c) {
			// Compute area from three corners
			return Area(b - a, c - a);
		}
		public static float Area(Vector3 a, Vector3 b) {
			// Compute area from two vectors representing two directed edges sharing a common origin
			return (float) (0.5 * Vector3.Cross(a, b).magnitude);
		}
		public static float Area(float a, float b, float c) {
			// Compute area from three side lengths
			return (float) (0.25 * Math.Sqrt(4 * Math.Pow(a, 2) * Math.Pow(b, 2) - Math.Pow(Math.Pow(a, 2) + Math.Pow(b, 2) - Math.Pow(c, 2), 2)));
		}
		public static float SignedVolume(Vector3 a, Vector3 b, Vector3 c) {
			return Vector3.Dot(a, Vector3.Cross(b, c)) / 6.0f;
		}
	}
}