﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace Geometry.Topology {
	public class Hull : Topology {
		public Vector3[] Vertices {get;}
		public Vector3[] Normals {get;}
		public int[] Indices {get;}

		public Hull(Vector3[] vertices, Vector3[] normals, int[] indices) {
			Vertices = vertices;
			Normals = normals;
			Indices = indices;
		}

		public Model Color(Color32 color) {
			return new Model(Vertices, Normals, Enumerable.Repeat(color, Vertices.Length).ToArray(), Indices);
		}
		public Mesh Mesh() {
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