using UnityEngine;

namespace Geometry.Generators {
	public class Quad {
		public static Mesh Mesh() {
			var vertices = new Vector3[] {
				new(-0.5f, -0.5f), new(0.5f, -0.5f), new(-0.5f, 0.5f), new(0.5f, 0.5f),
				new(-0.5f, -0.5f), new(0.5f, -0.5f), new(-0.5f, 0.5f), new(0.5f, 0.5f)
			};
			var normals = new[] {
				Vector3.back, Vector3.back, Vector3.back, Vector3.back,
				Vector3.forward, Vector3.forward, Vector3.forward, Vector3.forward
			};
			var indices = new[] {
				0, 2, 1,
				1, 2, 3,
				4, 5, 6,
				5, 7, 6
			};
			var uvs = new Vector2[] {
				new(0, 0), new(1, 0), new(0, 1), new(1, 1),
				new(0, 0), new(1, 0), new(0, 1), new(1, 1)
			};

			var shape = new Mesh();
		
			shape.Clear();
			shape.SetVertices(vertices);
			shape.SetTriangles(indices, 0);
			shape.SetNormals(normals);
			shape.SetUVs(0, uvs);
			
			return shape;
		}
	}
}