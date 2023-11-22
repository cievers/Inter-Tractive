using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace Geometry.Topology {
	public readonly struct Graph : Topology {
		// TODO: Inherit from Wire, probably
		public Vector3[] Vertices {get;}
		public Color32[] Colors {get;}
		public int[] Indices {get;}
		
		public Graph(Vector3[] vertices, Color32[] colors, int[] indices) {
			Vertices = vertices;
			Colors = colors;
			Indices = indices;
		}
		
		public Mesh Mesh() {
			// TODO: Maybe check if a specific index format is really needed
			var shape = new Mesh {indexFormat = IndexFormat.UInt32};
		
			// TODO: Works nicely, but always 1 pixel wide
			// Solution is likely within shaders, maybe learn from https://github.com/devOdico/GoodLines
			shape.Clear();
			shape.SetVertices(Vertices);
			shape.SetIndices(Indices, MeshTopology.Lines, 0);
			shape.SetColors(Colors);

			return shape;
		}
		
		public static Graph Join(params Graph[] geometry) => Join((IList<Graph>)geometry);
		public static Graph Join(IList<Graph> geometry) {
			var final = new Graph(
				new Vector3[geometry.Sum(static g => g.Vertices.Length)],
				new Color32[geometry.Sum(static g => g.Colors.Length)],
				new int[geometry.Sum(static g => g.Indices.Length)]
			);

			var lastVertexIndex = 0;
			var lastElemIndex = 0;
			foreach (var g in geometry) {
				g.Vertices.CopyTo(final.Vertices, lastVertexIndex);
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