using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace Geometry.Generators {
	public class ConvexPolygon {
		public IEnumerable<Vector3> Points {get;}
		public IEnumerable<Triangle> Faces => throw new NotImplementedException();
		public IEnumerable<int> Indices {
			get {
				var result = new List<int>();
				for (var i = 0; i < Points.Count() - 2; i++) {
					result.Add(0);
					result.Add(i+1);
					result.Add(i+2);
				}
				return result;
			}
		}
		public IEnumerable<Vector3> Normals => Enumerable.Repeat(normal, Points.Count());

		private readonly Vector3 normal;

		public ConvexPolygon(List<Vector3> points, Vector3 origin, Vector3 normal) {
			var rotation = Quaternion.FromToRotation(normal, Vector3.forward);
			var mapping = points
				.ToDictionary(point => rotation * Plane.Projection(point, origin, normal), point => point)
				.ToDictionary(projection => new Vector2(projection.Key.x, projection.Key.y), point => point.Value);
			var perimeter = new ConvexPerimeter(mapping.Keys.ToList());
			this.normal = normal;
			Points = perimeter.Points.Select(point => mapping[point]);
		}

		public Hull Hull() {
			return new Hull(Points.ToList(), Normals.ToList(), Indices.ToList());
		}
		public Mesh Mesh() {
			var mesh = new Mesh {indexFormat = IndexFormat.UInt32};

			mesh.Clear();
			mesh.SetVertices(Points.ToList());
			mesh.SetTriangles(Indices.ToList(), 0);
			mesh.SetNormals(Normals.ToList());

			return mesh;
		}
	}
}