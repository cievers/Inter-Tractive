using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Geometry {
	public class Hull {
		public List<Vector3> Vertices {get; private set;}
		public List<Vector3> Normals {get; private set;}
		public List<int> Indices {get; private set;}

		public Hull(List<Vector3> vertices, List<Vector3> normals, List<int> indices) {
			Vertices = vertices;
			Normals = normals;
			Indices = indices;
		}
		public Hull() : this(new List<Vector3>(), new List<Vector3>(), new List<int>()) {}

		public void AddVertices(Vector3 vertex) {
			Vertices.Add(vertex);
		}
		public void AddVertices(IEnumerable<Vector3> vertices) {
			Vertices.AddRange(vertices);
		}
		public void AddNormals(Vector3 normal) {
			Normals.Add(normal);
		}
		public void AddNormals(IEnumerable<Vector3> normals) {
			Normals.AddRange(normals);
		}
		public void AddIndices(int index) {
			Indices.Add(index);
		}
		public void AddIndices(IEnumerable<int> indices) {
			Indices.AddRange(indices);
		}

		public Mesh Mesh() {
			// TODO: Normals aren't used in rendering yet
			var mesh = new Mesh {indexFormat = IndexFormat.UInt32};

			mesh.Clear();
			mesh.SetVertices(Vertices);
			mesh.SetTriangles(Indices, 0);
			mesh.RecalculateNormals();

			return mesh;
		}
	}
}