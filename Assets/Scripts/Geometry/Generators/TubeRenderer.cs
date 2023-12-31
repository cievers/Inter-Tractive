using System;
using System.Collections.Generic;
using System.Linq;
using Geometry.Topology;
using Geometry.Tracts;
using UnityEngine;

namespace Geometry.Generators {
	public class TubeRenderer : TractogramRenderer, WalkRenderer {
		private readonly int vertices;
		private readonly float radius;
		private readonly Func<Vector3, Vector3, float, Color32> color;

		public TubeRenderer(int vertices, float radius, Func<Vector3, Vector3, float, Color32> color) {
			this.vertices = vertices;
			this.radius = radius;
			this.color = color;
		}
		public TubeRenderer(int vertices, float radius, Color32 color) : this(vertices, radius, (_, _, _) => color) {}
		
		public Topology.Topology Render(Tractogram tractogram) {
			return Model.Join(tractogram.Tracts.Select(Route).ToList());
		}
		public Topology.Topology Render(Tract tract) {
			return Route(tract);
		}
		public Topology.Topology Render(Walk walk) {
			return Route(walk);
		}

		private Model Route(Tract tract) {
			if (tract.Points.Length == 2) {
				return Route(tract.Segments.ToArray()[0]);
			}
			
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
			var currentColor = color.Invoke(tract.Points[0], planes[0], 0);

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
				currentColor = color.Invoke(tract.Points[i], planes[i], i / (tract.Points.Length - 1));

				intersections.CopyTo(resultVertices, i * vertices);
				normals.CopyTo(resultNormals, i * vertices);
				Enumerable.Repeat(currentColor, vertices).ToArray().CopyTo(resultColors, i * vertices);
				triangles.CopyTo(resultTriangles, (i - 1) * vertices * 6);
			}

			return Model.Join(startingCap, new Model(resultVertices, resultNormals, resultColors, resultTriangles), Cap(currentVertices, tract.Points[^1], directions[^1], currentColor));
		}
		private Model Route(Walk walk) {
			if (walk.Points.Length == 2) {
				return Route(walk.Segments.ToArray()[0]);
			}
			
			var resultVertices = new Vector3[walk.Points.Length * vertices];
			var resultNormals = new Vector3[walk.Points.Length * vertices];
			var resultColors = new Color32[walk.Points.Length * vertices];
			var resultTriangles = new int[walk.Points.Length * vertices * 6];

			var segments = walk.Segments.ToArray();
			var planes = walk.Normals.ToArray();

			for (var i = 0; i < walk.Points.Length; i++) {
				var axisB = Vector3.Cross(planes[i], planes[(i+1)%walk.Points.Length]).normalized * radius;
				var axisA = Vector3.Cross(axisB, planes[i]).normalized * radius;

				var currentDirections = Directions(axisA, axisB);
				var currentVertices = currentDirections.Select(direction => walk.Points[i] + direction).ToArray();
				var currentNormals = currentDirections.Select(direction => direction.normalized).ToArray();
				var currentColor = color.Invoke(walk.Points[i], planes[i], i);
				var currentTriangles = Wrap(vertices).Select(j => (j + i * vertices) % resultVertices.Length).ToArray();

				currentVertices.CopyTo(resultVertices, i * vertices);
				currentNormals.CopyTo(resultNormals, i * vertices);
				Enumerable.Repeat(currentColor, vertices).ToArray().CopyTo(resultColors, i * vertices);
				currentTriangles.CopyTo(resultTriangles, i * vertices * 6);
			}

			return new Model(resultVertices, resultNormals, resultColors, resultTriangles);
		}
		private Model Route(Segment segment) {
			var resultVertices = new Vector3[2 * vertices];
			var resultNormals = new Vector3[2 * vertices];
			var resultColors = new Color32[2 * vertices];

			var normal = segment.Size.normalized;

			var axisB = Vector3.Cross(normal, Plane.Any(segment.Start, normal)).normalized * radius;
			var axisA = Vector3.Cross(axisB, normal).normalized * radius;

			var startingDirections = Directions(axisA, axisB);
			var startingVertices = startingDirections.Select(direction => segment.Start + direction).ToArray();
			var endingVertices = startingDirections.Select(direction => segment.End + direction).ToArray();
			var normals = startingDirections.Select(direction => direction.normalized).ToArray();
			var startingColor = color.Invoke(segment.Start, normal, 0);
			var endingColor = color.Invoke(segment.End, normal, 1);

			var startingCap = Cap(startingVertices, segment.Start, -normal, startingColor);
			var endingCap = Cap(endingVertices, segment.End, normal, endingColor);
			
			startingVertices.CopyTo(resultVertices, 0);
			endingVertices.CopyTo(resultVertices, vertices);
			normals.CopyTo(resultNormals, 0);
			normals.CopyTo(resultNormals, vertices);
			Enumerable.Repeat(startingColor, vertices).ToArray().CopyTo(resultColors, 0);
			Enumerable.Repeat(endingColor, vertices).ToArray().CopyTo(resultColors, vertices);

			return Model.Join(startingCap, new Model(resultVertices, resultNormals, resultColors, Wrap(vertices).ToArray()), endingCap);
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
	}
}