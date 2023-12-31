﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace Geometry.Topology {
	public class Model : Topology {
		// TODO: Inherit from Hull, probably
		public Vector3[] Vertices {get;}
		public Vector3[] Normals {get;}
		public Color32[] Colors {get;}
		public int[] Indices {get;}

		public Model(Vector3[] vertices, Vector3[] normals, Color32[] colors, int[] indices) {
			Vertices = vertices;
			Normals = normals;
			Colors = colors;
			Indices = indices;
		}

		public Mesh Mesh() {
			var mesh = new Mesh {indexFormat = IndexFormat.UInt32};

			mesh.Clear();
			mesh.SetVertices(Vertices);
			mesh.SetColors(Colors);
			mesh.SetTriangles(Indices, 0);

			if (Normals.Length > 0) {
				mesh.SetNormals(Normals);
			} else {
				mesh.RecalculateNormals();
			}

			return mesh;
		}

		public static Model Join(params Model[] geometry) => Join((IList<Model>) geometry);
		public static Model Join(IList<Model> geometry) {
			var final = new Model(
				new Vector3[geometry.Sum(static g => g.Vertices.Length)],
				new Vector3[geometry.Sum(static g => g.Normals.Length)],
				new Color32[geometry.Sum(static g => g.Colors.Length)],
				new int[geometry.Sum(static g => g.Indices.Length)]
			);

			var lastVertexIndex = 0;
			var lastElemIndex = 0;
			foreach (var g in geometry) {
				g.Vertices.CopyTo(final.Vertices, lastVertexIndex);
				g.Normals.CopyTo(final.Normals, lastVertexIndex);
				g.Colors.CopyTo(final.Colors, lastVertexIndex);
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