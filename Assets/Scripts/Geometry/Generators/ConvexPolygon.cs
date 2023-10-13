using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace Geometry.Generators {
	public class ConvexPolygon {
		public IEnumerable<Vector3> Points {get;}
		public IEnumerable<Triangle> Faces => throw new NotImplementedException();
		public IEnumerable<int> Indices => Triangulate(true, false);
		public IEnumerable<Vector3> Normals => Enumerable.Repeat(normal.normalized, Points.Count());

		private readonly Vector3 origin;
		private readonly Vector3 normal;

		public ConvexPolygon(List<Vector3> points, Vector3 origin, Vector3 normal) {
			var rotation = Quaternion.FromToRotation(normal, Vector3.forward);
			var mapping = points
				.ToDictionary(point => rotation * Plane.Projection(point, origin, normal), point => point)
				.ToDictionary(projection => new Vector2(projection.Key.x, projection.Key.y), point => point.Value);
			var perimeter = new ConvexPerimeter(mapping.Keys.ToList());
			this.origin = origin;
			this.normal = normal;
			Points = perimeter.Points.Select(point => mapping[point]);
		}
		private ConvexPolygon(List<Vector3> points, Vector3 normal) {
			Points = points;
			origin = points[0];
			this.normal = normal;
		}

		public float Perimeter() {
			var points = Points.ToArray();
			var sum = (points[^1] - points[0]).magnitude;
			for (var i = 0; i < points.Length - 1; i++) {
				sum += (points[i + 1] - points[i]).magnitude;
			}
			return sum;
		}
		public float Area() {
			var sum = 0f;
			var points = Points.ToArray();
			for (var i = 0; i < points.Length - 2; i++) {
				sum += Triangle.Area(points[0], points[i + 1], points[i + 2]);
			}
			return sum;
		}

		public Side Side(Vector3 point) {
			return Plane.Side(point, origin, normal);
		}
		public Line? IntersectionLine(ConvexPolygon other) {
			// This is a bit of a misnomer, as it returns the intersecting line of the planes of the two polygons, which might not intersect either polygon at all
			return Plane.Intersection(origin, normal, other.origin, other.normal);
		}
		public ConvexPolygon[] Split(Line line) {
			// Assume the line lies in the plane defined by this polygon's origin and normal
			// Also definitively projects the given set of points to the given plane
			var rotation = Quaternion.FromToRotation(normal, Vector3.forward);
			var inverse = Quaternion.Inverse(rotation);
			var projected = Geometry.Plane.Projection(line, origin, normal);
			var perimeter = new ConvexPerimeter(Points
				.Select(point => rotation * Plane.Projection(point, origin, normal))
				.Select(projection => new Vector2(projection.x, projection.y))
				.ToList()
			);
			return perimeter.Split(projected).Select(p => new ConvexPolygon(p.Points.Select(point => origin + inverse * point).ToList(), normal)).ToArray();
		}

		private List<int> Triangulate(bool front = true, bool back = true) {
			var length = Points.Count();
			var result = new List<int>();
			for (var i = 0; i < length - 2; i++) {
				switch (front) {
					case true: {
						result.Add(0);
						result.Add(i+2);
						result.Add(i+1);
						if (back) {
							result.Add(length);
							result.Add(length+i+1);
							result.Add(length+i+2);
						}
						break;
					}
					case false when back:
						result.Add(0);
						result.Add(i+1);
						result.Add(i+2);
						break;
				}
			}
			return result;
		}
		public Hull Surface(bool front = true) {
			return new Hull(Points.ToArray(), (front ? Normals : Normals.Select(normal => -normal)).ToArray(), Triangulate(front, !front).ToArray());
		}
		public Hull Hull() {
			return new Hull(Points.Concat(Points).ToArray(), Normals.Concat(Normals.Select(normal => -normal)).ToArray(), Triangulate().ToArray());
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