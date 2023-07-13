using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Geometry {
	public readonly struct Wire {
		public Vector3[] Vertices {get;}
		public Vector3[] Normals {get;}
		public int[] Indices {get;}
		
		public Wire(Vector3[] vertices, Vector3[] normals, int[] indices) {
			Vertices = vertices;
			Normals = normals;
			Indices = indices;
		}
		
		public static Wire Join(params Wire[] geometry) => Join((IList<Wire>)geometry);
		public static Wire Join(IList<Wire> geometry) {
			var final = new Wire(
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