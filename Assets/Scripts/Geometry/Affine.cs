using System.Collections.Generic;
using UnityEngine;

namespace Geometry {
	public class Affine {
		private readonly float x0;
		private readonly float x1;
		private readonly float x2;
		private readonly float x3;
		private readonly float y0;
		private readonly float y1;
		private readonly float y2;
		private readonly float y3;
		private readonly float z0;
		private readonly float z1;
		private readonly float z2;
		private readonly float z3;

		public Affine(IReadOnlyList<float> x, IReadOnlyList<float> y, IReadOnlyList<float> z) {
			// TODO: Assert each array is length 4
			// Or actually, a better input method
			x0 = x[0];
			x1 = x[1];
			x2 = x[2];
			x3 = x[3];
			y0 = y[0];
			y1 = y[1];
			y2 = y[2];
			y3 = y[3];
			z0 = z[0];
			z1 = z[1];
			z2 = z[2];
			z3 = z[3];
		}
		public Affine(Vector3 scale, Vector3 offset) {
			x0 = scale.x;
			y1 = scale.y;
			z2 = scale.z;
			x3 = offset.x;
			y3 = offset.y;
			z3 = offset.z;
		}
		public Affine(Vector3 scale, Vector3 offset, bool zIsUp) {
			x0 = scale.x;
			y2 = scale.z;
			z1 = scale.y;
			x3 = offset.x;
			y3 = offset.z;
			z3 = offset.y;
		}

		public Vector3 Transform(Vector3 input) {
			return new Vector3(
				input.x * x0 + input.y * x1 + input.z * x2 + x3,
				input.x * y0 + input.y * y1 + input.z * y2 + y3,
				input.x * z0 + input.y * z1 + input.z * z2 + z3
			);
		}
		public Vector3 Scale() {
			return new Vector3(x0 + x1 + x2 + x3, y0 + y1 + y2 + y3, z0 + z1 + z2 + z3);
		}
		public Vector3 Offset() {
			return new Vector3(x3, y3, z3);
		}

		public float[][] Matrix() {
			return new[] {new[] {
				x0, x1, x2, x3
			}, new[] {
				y0, y1, y2, y3
			}, new[] {
				z0, z1, z2, z3
			}};
		}
	}
}