﻿using System;
using System.Collections.Generic;
using System.Linq;
using Geometry.Tracts;
using UnityEngine;
using UnityEngine.Rendering;

namespace Geometry {
	public class Plane {
		public static Side Side(Vector3 point, Vector3 origin, Vector3 normal) {
			return Vector3.Dot(normal, point - origin) switch {
				> 0 => Geometry.Side.Positive,
				< 0 => Geometry.Side.Negative,
				_ => Geometry.Side.Contained
			};
		}
		public static Vector3 Projection(Vector3 point, Vector3 origin, Vector3 normal) {
			var normalized = normal.normalized;
			return point - normalized * Vector3.Dot(normalized, point - origin);
		}
		public static Planar.Line Projection(Line line, Vector3 origin, Vector3 normal) {
			var projected = Projection(line.Origin, origin, normal);
			var directed = Projection(line.Origin + line.Direction, origin, normal);
			return new Planar.Line(projected, (directed - projected).normalized);
		}
		public static IEnumerable<Vector3> Projections(IEnumerable<Vector3> points, Vector3 origin, Vector3 normal) {
			return points.Select(point => Projection(point, origin, normal));
		}
		public static Vector3? Intersection(Line line, Vector3 origin, Vector3 normal) {
			// origin & normal define the plane
			// origin Is a point on the plane
			// normal Is a normal vector defining the plane direction (does not need to be normalized)
			var u = line.Direction;
			var dot = Vector3.Dot(normal, u);

			if (Mathf.Abs(dot) > 0) {
				// If the line is not parallel to the plane
				// The factor is a multiplication of u representing where the intersection is
				var v = line.Origin - origin;
				return line.Origin + u * (-Vector3.Dot(normal, v) / dot);
			}

			// The line is parallel to plane
			return null;
		}
		public static Vector3? Intersection(Segment segment, Vector3 origin, Vector3 normal) {
			// origin & normal define the plane
		    // origin Is a point on the plane
		    // normal Is a normal vector defining the plane direction (does not need to be normalized)
			var u = segment.Size;
			var dot = Vector3.Dot(normal, u);

			if (Mathf.Abs(dot) > 0) {
				// The factor is a multiplication of u representing where the intersection is
				// if this factor is between (0 - 1) the point intersects with the segment
				var v = segment.Start - origin;
				var fac = -Vector3.Dot(normal, v) / dot;
				if (fac is >= 0 and <= 1) {
					return segment.Start + u * fac;
				}
			}

			// The segment is parallel to plane, or beyond the bounds of the line segment
			return null;
		}
		public static Vector3? Intersection(Edge edge, Vector3 origin, Vector3 normal) {
			// The creation of a Segment instance seems unnecessarily costly here
			return Intersection(new Segment(edge.A, edge.B), origin, normal);
		}
		public static Line? Intersection(Vector3 originA, Vector3 normalA, Vector3 originB, Vector3 normalB) {
			if (normalA == normalB) {
				return null;
			}
			
			// Logically the 3rd plane, but we only use the normal component.
			var direction = Vector3.Cross(normalA, normalB);
			var determinant = direction.sqrMagnitude;
    
			// If the determinant is 0, that means parallel planes, no intersection, but that should have been check beforehand
			return new Line((
				Vector3.Cross(direction, normalB) * -Vector3.Dot(normalA, originA) +
				Vector3.Cross(normalA, direction) * -Vector3.Dot(normalB, originB)
			) / determinant, direction);
		}
		public static IEnumerable<Vector3> Intersections(IEnumerable<Edge> segments, Vector3 origin, Vector3 normal) {
			return segments
				.Select(edge => Intersection(edge, origin, normal))
				.Where(intersection => intersection != null)
				.Select(intersection => (Vector3) intersection);
		}
		public static IEnumerable<Vector3> Intersections(IEnumerable<Segment> segments, Vector3 origin, Vector3 normal) {
			return segments
				.Select(segment => Intersection(segment, origin, normal))
				.Where(intersection => intersection != null)
				.Select(intersection => (Vector3) intersection);
		}
		public static IEnumerable<Vector3> Intersections(Tract tract, Vector3 origin, Vector3 normal) {
			return tract.Segments
				.Select(segment => Intersection(segment, origin, normal))
				.Where(intersection => intersection != null)
				.Select(intersection => (Vector3) intersection);
		}
		public static IEnumerable<Vector3> Intersections(Tractogram tractogram, Vector3 origin, Vector3 normal) {
			return tractogram.Tracts.SelectMany(tract => Intersections(tract, origin, normal));
		}

		public static Vector3 Any(Vector3 origin, Vector3 normal) {
			var projections = Projections(new[] {Vector3.zero, Vector3.right, Vector3.up, Vector3.forward}, origin, normal).Select(projection => projection - origin).ToArray();
			var magnitudes = projections.Select(projection => projection.magnitude).ToArray();
			var maximum = magnitudes.Max();
			for (var i = 0; i < projections.Length; i++) {
				if (magnitudes[i] == maximum) {
					return projections[i];
				}
			}
			throw new ArithmeticException("One of the selected projections should have the maximum magnitude");
		}

		public static Mesh Mesh(Vector3 origin, Vector3 normal, float size) {
			var rotation = Quaternion.FromToRotation(Vector3.forward, normal);
			var mesh = new Mesh {indexFormat = IndexFormat.UInt32};

			mesh.Clear();
			mesh.SetVertices(new Vector3[] {
				new(size, size),
				new(-size, size),
				new(size, -size),
				new(-size, -size),
				new(size, size),
				new(-size, size),
				new(size, -size),
				new(-size, -size)
			}.Select(point => rotation * point + origin).ToArray());
			mesh.SetNormals(Enumerable.Repeat(normal, 4).Concat(Enumerable.Repeat(normal * -1, 4)).ToArray());
			mesh.SetTriangles(new[] {
				0, 1, 3,
				0, 3, 2,
				0, 2, 3,
				0, 3, 1
			}, 0);

			return mesh;
		}
	}
}