using System.Linq;
using UnityEngine;

namespace Geometry.Generators {
	public class Lines : MonoBehaviour {
		public MeshFilter filter;

		private void Square() {
			var vertices = new[] {
				new Vector3(0, 0, 0),
				new Vector3(0, 1, 0),
				new Vector3(1, 0, 0),
				new Vector3(1, 0, 0),
				new Vector3(0, 1, 0),
				new Vector3(1, 1, 0),
				new Vector3(2, 0, 0),
				new Vector3(2, 2, 0),
				new Vector3(2f, 1, 0),
			};
			var triangles = Enumerable.Range(0, vertices.Length).ToArray();
			var shape = filter.mesh;
		
			shape.Clear();
			shape.vertices = vertices;
			shape.triangles = triangles;
			shape.RecalculateNormals();
		}
		private void Line() {
			// TODO: Works nicely, but always 1 pixel wide
			// Solution is likely within shaders, maybe learn from https://github.com/devOdico/GoodLines
			var vertices = new[] {
				new Vector3(0, 0, 0),
				new Vector3(0, 1, 0),
				new Vector3(1, 0, 0),
				new Vector3(1, 0, 0),
				new Vector3(0, 1, 0),
				new Vector3(1, 1, 0),
				new Vector3(2, 0, 0),
				new Vector3(2, 2, 0),
			};
			var triangles = Enumerable.Range(0, vertices.Length).ToArray();
			var shape = filter.mesh;
		
			shape.Clear();
			shape.SetVertices(vertices);
			shape.SetIndices(triangles, MeshTopology.Lines, 0);
			// shape.RecalculateNormals();
		}
		private void Start() {
			Line();
		}
	}
}