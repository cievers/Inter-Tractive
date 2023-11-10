using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace Geometry.Topology {
	public readonly struct Wire : Topology {
		public Vector3[] Vertices {get;}
		public int[] Indices {get;}
		
		public Wire(Vector3[] vertices, int[] indices) {
			Vertices = vertices;
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

			return shape;
		}
		
		public static Wire Join(params Wire[] geometry) => Join((IList<Wire>)geometry);
		public static Wire Join(IList<Wire> geometry) {
			var final = new Wire(
				new Vector3[geometry.Sum(static g => g.Vertices.Length)],
				new int[geometry.Sum(static g => g.Indices.Length)]
			);

			var lastVertexIndex = 0;
			var lastElemIndex = 0;
			foreach (var g in geometry) {
				g.Vertices.CopyTo(final.Vertices, lastVertexIndex);
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