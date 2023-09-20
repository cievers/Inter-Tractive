using System;
using System.Linq;
using Geometry.Tracts;
using UnityEngine;
using UnityEngine.Rendering;
using Mesh = UnityEngine.Mesh;

namespace Geometry.Generators {
	public class WireframeRenderer : TractogramRenderer {
		public Mesh Render(Tractogram tractogram) {
			return Render(Wire.Join(tractogram.Tracts.Select(Route).ToList()));
		}
		public Mesh Render(Tract tract) {
			return Render(Route(tract));
		}
		private static Mesh Render(Wire wires) {
			// TODO: Maybe check if a specific index format is really needed
			var shape = new Mesh {indexFormat = IndexFormat.UInt32};

			// Debug.Log(wires.Vertices.Length);
			// Debug.Log(wires.Indices.Length);
		
			// TODO: Works nicely, but always 1 pixel wide
			// Solution is likely within shaders, maybe learn from https://github.com/devOdico/GoodLines
			shape.Clear();
			shape.SetVertices(wires.Vertices);
			shape.SetIndices(wires.Indices, MeshTopology.Lines, 0);
			shape.SetColors(wires.Normals.Select(normal => new Color(Math.Abs(normal.x), Math.Abs(normal.z), Math.Abs(normal.y))).ToArray());

			return shape;
		}
		private static Wire Route(Tract tract) {
			var vertices = new Vector3[tract.Points.Length];
			var normals = new Vector3[tract.Points.Length]; // TODO: Normals aren't used in rendering yet
			var indices = new int[(tract.Points.Length - 1) * 2];
			// TODO: Creating indices can be simplified by something such as
			// Enumerable.Range(0, vertices.Length).ToArray();
			// Because we know the pattern needs to be 0, 1, 1, 2, 2, 3, 3, 4, 4, 5, ...
			
			vertices[0] = tract.Points[0];
			normals[0] = Vector3.Normalize(tract.Points[1] - tract.Points[0]);
			
			var indexIndex = 0;
			for (var i = 1; i < tract.Points.Length; i++) {
				vertices[i] = tract.Points[i];
				normals[i] = Vector3.Normalize(tract.Points[i] - tract.Points[i - 1]);
					
				indices[indexIndex++] = i - 1;
				indices[indexIndex++] = i;
			}

			return new Wire(vertices, normals, indices);
		}
	}
}