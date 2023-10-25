using System;
using System.Collections.Generic;
using System.Linq;
using Geometry.Tracts;
using UnityEngine;

namespace Geometry.Generators {
	public class TubeRenderer : TractogramRenderer {
		private readonly int vertices;
		private readonly float radius;
		private readonly Color32 color;

		public TubeRenderer(int vertices, float radius, Color32 color) {
			this.vertices = vertices;
			this.radius = radius;
			this.color = color;
		}
		
		public Mesh Render(Tractogram tractogram) {
			return Model.Join(tractogram.Tracts.Select(Route).ToList()).Mesh();
		}
		public Mesh Render(Tract tract) {
			return Route(tract).Mesh();
		}

		private Model Route(Tract tract) {
			var resultVertices = new Vector3[tract.Points.Length * vertices];
			var resultNormals = new Vector3[tract.Points.Length * vertices];
			var resultColors = new Color32[tract.Points.Length * vertices];
			// var resultColors = Enumerable.Repeat(color, tract.Points.Length * vertices).ToArray();
			var resultTriangles = new int[(tract.Points.Length - 1) * vertices * 6];

			var segments = tract.Segments.ToArray();
			var directions = segments.Select(segment => segment.Size.normalized).ToArray();
			var planes = tract.Normals.ToArray();

			var startingAxisB = Vector3.Cross(directions[0], directions[1]).normalized * radius;
			var startingAxisA = Vector3.Cross(startingAxisB, directions[0]).normalized * radius;

			var startingDirections = Directions(startingAxisA, startingAxisB);
			var currentVertices = startingDirections.Select(direction => tract.Points[0] + direction).ToArray();
			var currentNormals = startingDirections.Select(direction => direction.normalized).ToArray();
			var currentColor = Color(planes[0]);

			var startingCap = Cap(currentVertices, tract.Points[0], -directions[0], currentColor);
			
			currentVertices.CopyTo(resultVertices, 0);
			currentNormals.CopyTo(resultNormals, 0);
			Enumerable.Repeat(currentColor, vertices).ToArray().CopyTo(resultColors, 0);

			for (var i = 1; i < tract.Points.Length; i++) {
				var origin = tract.Points[i];
				var intersections = currentVertices
					.Select(vertex => Plane.Intersection(new Line(vertex, directions[i - 1]), origin, planes[i]))
					.Where(intersection => intersection != null)
					.Select(intersection => (Vector3) intersection)
					.ToArray();
				var normals = intersections.Select(point => (point - origin).normalized).ToArray();

				if (intersections.Length != vertices) {
					throw new ArithmeticException("There should always be the same number of intersections with a plane along the tract as there are edges");
				}

				var triangles = Wrap(vertices).Select(j => j + (i - 1) * vertices).ToArray();
				// var triangles = new int[vertices * 6];
				// for (var j = 0; j < vertices; j++) {
				// 	triangles[j * 6] = (i - 1) * vertices + j;
				// 	triangles[j * 6 + 1] = (i - 1) * vertices + (j + 1) % vertices;
				// 	triangles[j * 6 + 2] = i * vertices + j;
				// 	triangles[j * 6 + 3] = i * vertices + j;
				// 	triangles[j * 6 + 4] = (i - 1) * vertices + (j + 1) % vertices;
				// 	triangles[j * 6 + 5] = i * vertices + (j + 1) % vertices;
				// }
				currentVertices = intersections;
				currentColor = Color(planes[i]);

				intersections.CopyTo(resultVertices, i * vertices);
				normals.CopyTo(resultNormals, i * vertices);
				Enumerable.Repeat(currentColor, vertices).ToArray().CopyTo(resultColors, i * vertices);
				triangles.CopyTo(resultTriangles, (i - 1) * vertices * 6);
			}

			return Model.Join(startingCap, new Model(resultVertices, resultNormals, resultColors, resultTriangles), Cap(currentVertices, tract.Points[^1], directions[^1], currentColor));
		}
		private Model Cap(Vector3[] points, Vector3 origin, Vector3 normal, Color32 color) {
			return Cap(points, origin, normal).Color(color);
		}
		private Hull Cap(Vector3[] points, Vector3 origin, Vector3 normal) {
			if (Vector3.Dot(normal, Vector3.Cross(points[1] - points[0], points[2] - points[0])) <= 0) {
				points = points.Reverse().ToArray();
			}
			var length = points.Length;
			var axisA = (points[0] - origin).normalized;
			var axisB = Vector3.Cross(normal, axisA).normalized;
			var layers = length / 4;
			var cone = length % 4 == 0;
			
			var resultVertices = new List<Vector3>();
			var resultNormals = new List<Vector3>();
			var resultTriangles = new List<int>();
			
			resultVertices.AddRange(points);
			resultNormals.AddRange(points.Select(point => (point - origin).normalized));

			for (var i = 1; i <= layers; i++) {
				if (cone && i == layers) {
					break;
				}
				var interval = (float) i / (cone ? layers : layers + 1);
				var layerRadius = radius * Mathf.Cos(Mathf.PI * 0.5f * interval);
				var layer = Circle(origin + normal.normalized * (radius * Mathf.Sin(Mathf.PI * 0.5f * interval)), axisA * layerRadius, axisB * layerRadius);
				
				resultVertices.AddRange(layer);
				resultNormals.AddRange(layer.Select(point => (point - origin).normalized));
				resultTriangles.AddRange(Wrap(length).Select(j => j + (i - 1) * length));
			}
			if (cone) {
				resultTriangles.AddRange(Cone(length).Select(v => v + resultVertices.Count - length));
				resultVertices.Add(origin + normal.normalized * radius);
				resultNormals.Add(normal.normalized);
			} else {
				resultTriangles.AddRange(Polygon(length).Select(v => v + resultVertices.Count - length));
			}
			
			return new Hull(resultVertices.ToArray(), resultNormals.ToArray(), resultTriangles.ToArray());
		}
		private static IEnumerable<int> Cone(int vertices) {
			var triangles = new int[vertices * 3];
			for (var i = 0; i < vertices; i++) {
				triangles[i * 3] = i;
				triangles[i * 3 + 1] = (i + 1) % vertices;
				triangles[i * 3 + 2] = vertices;
			}
			return triangles;
		}
		private static IEnumerable<int> Wrap(int vertices) {
			var triangles = new int[vertices * 6];
			for (var i = 0; i < vertices; i++) {
				triangles[i * 6] = i;
				triangles[i * 6 + 1] = (i + 1) % vertices;
				triangles[i * 6 + 2] = vertices + i;
				triangles[i * 6 + 3] = vertices + i;
				triangles[i * 6 + 4] = (i + 1) % vertices;
				triangles[i * 6 + 5] = vertices + (i + 1) % vertices;
			}
			return triangles;
		}
		private static IEnumerable<int> Polygon(int vertices) {
			var triangles = new int[vertices * 3];
			for (var i = 0; i < vertices; i++) {
				triangles[i * 3] = 0;
				triangles[i * 3 + 1] = i;
				triangles[i * 3 + 2] = (i + 1) % vertices;
			}
			return triangles;
		}
		
		private Tuple<Vector3, Vector3> IntersectionAxis(Vector3 u, Vector3 v) {
			// Assume U is the direction vector of the incoming point, and V is the direction vector of the outgoing point
			return new Tuple<Vector3, Vector3>((u + v).normalized * radius, Vector3.Cross(u, v).normalized * radius);
		}
		private Vector3[] Directions(Vector3 axisA, Vector3 axisB) {
			var result = new Vector3[vertices];
			for(var i = 0; i < vertices; i++) {
				var t = (float) i / vertices;
				result[i] = axisA * Mathf.Cos(2 * Mathf.PI * t) + axisB * Mathf.Sin(2 * Mathf.PI * t);
			}
			return result;
		}
		private Vector3[] Directions(Tuple<Vector3, Vector3> axis) {
			return Directions(axis.Item1, axis.Item2);
		}
		private Vector3[] Circle(Vector3 origin, Vector3 axisA, Vector3 axisB) {
			var result = new Vector3[vertices];
			for(var i = 0; i < vertices; i++) {
				var t = (float) i / vertices;
				result[i] = origin + axisA * Mathf.Cos(2 * Mathf.PI * t) + axisB * Mathf.Sin(2 * Mathf.PI * t);
			}
			return result;
		}
		private Vector3[] Circle(Vector3 origin, Tuple<Vector3, Vector3> axis) {
			return Circle(origin, axis.Item1, axis.Item2);
		}

		private Color32 Color(Vector3 normal) {
			return new Color(Math.Abs(normal.x), Math.Abs(normal.z), Math.Abs(normal.y));
		}
	}
}