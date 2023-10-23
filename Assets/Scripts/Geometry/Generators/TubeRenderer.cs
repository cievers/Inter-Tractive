using System;
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
			var resultColors = Enumerable.Repeat(color, tract.Points.Length * vertices).ToArray();
			var resultTriangles = new int[(tract.Points.Length - 1) * vertices * 6];

			var segments = tract.Segments.ToArray();
			var directions = segments.Select(segment => segment.Size.normalized).ToArray();
			var planes = tract.Normals.ToArray();

			var startingAxisB = Vector3.Cross(directions[0], directions[1]).normalized * radius;
			var startingAxisA = Vector3.Cross(startingAxisB, directions[0]).normalized * radius;

			var startingDirections = Directions(startingAxisA, startingAxisB);
			var currentVertices = startingDirections.Select(direction => tract.Points[0] + direction).ToArray();
			var currentNormals = startingDirections.Select(direction => direction.normalized).ToArray();
			
			currentVertices.CopyTo(resultVertices, 0);
			currentNormals.CopyTo(resultNormals, 0);

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

				var triangles = new int[vertices * 6];
				for (var j = 0; j < vertices; j++) {
					triangles[j * 6] = (i - 1) * vertices + j;
					triangles[j * 6 + 1] = (i - 1) * vertices + (j + 1) % vertices;
					triangles[j * 6 + 2] = i * vertices + j;
					triangles[j * 6 + 3] = i * vertices + j;
					triangles[j * 6 + 4] = (i - 1) * vertices + (j + 1) % vertices;
					triangles[j * 6 + 5] = i * vertices + (j + 1) % vertices;
				}
				
				intersections.CopyTo(resultVertices, i * vertices);
				normals.CopyTo(resultNormals, i * vertices);
				triangles.CopyTo(resultTriangles, (i - 1) * vertices * 6);
				currentVertices = intersections;
			}

			return new Model(resultVertices, resultNormals, resultColors, resultTriangles);
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