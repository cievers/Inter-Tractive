using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace Geometry {
	public class Hull {
		public Vector3[] Vertices {get; private set;}
		public Vector3[] Normals {get; private set;}
		public int[] Indices {get; private set;}

		public Hull(Vector3[] vertices, Vector3[] normals, int[] indices) {
			Vertices = vertices;
			Normals = normals;
			Indices = indices;
		}
		public Hull() : this(new Vector3[]{}, new Vector3[]{}, new int[]{}) {}

		public Mesh Mesh() {
			// TODO: Normals aren't used in rendering yet
			var mesh = new Mesh {indexFormat = IndexFormat.UInt32};

			mesh.Clear();
			mesh.SetVertices(Vertices);
			mesh.SetNormals(Normals);
			mesh.SetTriangles(Indices, 0);

			return mesh;
		}
		
		public static Hull Join(params Hull[] geometry) => Join((IList<Hull>) geometry);
		public static Hull Join(IList<Hull> geometry) {
			var final = new Hull(
				new Vector3[geometry.Sum(static g => g.Vertices.Length)],
				new Vector3[geometry.Sum(static g => g.Normals.Length)],
				new int[geometry.Sum(static g => g.Indices.Length)]
			);

			var lastVertexIndex = 0;
			var lastElemIndex = 0;
			foreach (var g in geometry) {
				g.Vertices.CopyTo(final.Vertices, lastVertexIndex);
				g.Normals.CopyTo(final.Normals, lastVertexIndex);
				g.Indices.CopyTo(final.Indices, lastElemIndex);
				
				//offset index buffer
				for (var j = lastElemIndex; j < lastElemIndex + g.Indices.Length; j++) {
					final.Indices[j] += (lastVertexIndex);
				}

				lastVertexIndex += g.Vertices.Length;
				lastElemIndex += g.Indices.Length;
			}

			return final;
		}
	}
}