using System;
using System.Collections.Generic;
using System.Linq;
using Geometry.Tracts;
using UnityEngine;
using UnityEngine.Rendering;
using Mesh = UnityEngine.Mesh;

namespace Geometry.Generators {
	public class WireframeRenderer : TractogramRenderer {
		public Mesh Render(Tractogram tractogram) {
			
			// var batches = new List<NeuroTrace.Models.Mesh>();
			// batches.Clear();
			var geometries = new List<Wire>();
			geometries.Clear();
			
			// int totalVertCount = tractogram.Tracts.Sum(static l => l.Points.Length);
			// var flags = new FlagsVertexAttr(new uint[totalVertCount]) { Usage = BufferUsageHint.DynamicDraw };
			// var ids = new LineIdVertexAttr(new uint[totalVertCount]);
			// var lineProgress = new LineProgressAttr(new float[totalVertCount]);
			// var lineTangents = new LineTangentVertexAttr(new Vector3[totalVertCount]);

			foreach (var line in tractogram.Tracts) {
				var vertices = new Vector3[line.Points.Length];
				var normals = new Vector3[line.Points.Length]; // TODO: Normals aren't used in rendering yet
				var indices = new int[(line.Points.Length - 1) * 2];
				// TODO: Creating indices can be simplified by something such as
				// Enumerable.Range(0, vertices.Length).ToArray();
				// Because we know the pattern needs to be 0, 1, 1, 2, 2, 3, 3, 4, 4, 5, ...
			
				vertices[0] = line.Points[0];
				normals[0] = Vector3.Normalize(line.Points[1] - line.Points[0]);
			
				var indexIndex = 0;
				for (var i = 1; i < line.Points.Length; i++) {
					vertices[i] = line.Points[i];
					normals[i] = Vector3.Normalize(line.Points[i] - line.Points[i - 1]);
					
					indices[indexIndex++] = i - 1;
					indices[indexIndex++] = i;
					
					// ids.Data[vertIndex] = line.Id;
					// lineTangents.Data[vertIndex] = direction;
					// lineProgress.Data[vertIndex] = (i / (float)(line.Points.Length - 1));
				}

				geometries.Add(new Wire(vertices, normals, indices));
			}

			var batch = Wire.Join(geometries);
			// batches.Add(new NeuroTrace.Models.Mesh(batch, false, false, flags, ids, lineProgress, lineTangents) { PrimitiveType = PrimitiveType.Lines });

			var shape = new Mesh {indexFormat = IndexFormat.UInt32};

			Debug.Log(batch.Vertices.Length);
			Debug.Log(batch.Indices.Length);
		
			// TODO: Works nicely, but always 1 pixel wide
			// Solution is likely within shaders, maybe learn from https://github.com/devOdico/GoodLines
			shape.Clear();
			shape.SetVertices(batch.Vertices);
			shape.SetIndices(batch.Indices, MeshTopology.Lines, 0);
			shape.SetColors(batch.Normals.Select(normal => new Color(Math.Abs(normal.x), Math.Abs(normal.z), Math.Abs(normal.y))).ToArray());

			return shape;
		}
	}
}