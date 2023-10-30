using System;
using System.Collections.Generic;
using System.Linq;
using Geometry;
using Geometry.Tracts;
using Maps.Grids;
using UnityEngine;
using UnityEngine.TextCore;

namespace Maps.Cells {
	public readonly struct Cuboid : Cell {
		private readonly Vector3[] points;
		public Vector3 Anchor => points[0];
		public Vector3 Extent => points[7];
		public Vector3 Size => Extent - Anchor;

		public Cuboid(Vector3 anchor, Vector3 size) {
			points = new[] {
				anchor,
				new Vector3(anchor.x + size.x, anchor.y, anchor.z),
				new Vector3(anchor.x, anchor.y + size.y, anchor.z),
				new Vector3(anchor.x, anchor.y, anchor.z + size.z),
				new Vector3(anchor.x + size.x, anchor.y + size.y, anchor.z),
				new Vector3(anchor.x, anchor.y + size.y, anchor.z + size.z),
				new Vector3(anchor.x + size.x, anchor.y, anchor.z + size.z),
				anchor + size
			};
		}
		public Cuboid(Vector3 anchor, float size) {
			points = new[] {
				anchor,
				new Vector3(anchor.x + size, anchor.y, anchor.z),
				new Vector3(anchor.x, anchor.y + size, anchor.z),
				new Vector3(anchor.x, anchor.y, anchor.z + size),
				new Vector3(anchor.x + size, anchor.y + size, anchor.z),
				new Vector3(anchor.x, anchor.y + size, anchor.z + size),
				new Vector3(anchor.x + size, anchor.y, anchor.z + size),
				new Vector3(anchor.x + size, anchor.y + size, anchor.z + size),
			};
		}
		public Cuboid(Lattice lattice, int x, int y, int z) {
			points = new[] {
				lattice[x, y, z],
				lattice[x+1, y, z],
				lattice[x, y+1, z],
				lattice[x, y, z+1],
				lattice[x+1, y+1, z],
				lattice[x, y+1, z+1],
				lattice[x+1, y, z+1],
				lattice[x+1, y+1, z+1],
			};
		}
		public Cuboid(Lattice lattice, Index3 index) : this(lattice, index.x, index.y, index.z) {}

		public IEnumerable<Vector3> Vertices => points;
		public IEnumerable<int> Indices => new[] {
			// Bottom face
			0, 1, 3,
			1, 6, 3,
			// Front face
			0, 2, 1,
			2, 4, 1,
			// Left face
			0, 3, 2,
			3, 5, 2,
			// Right face
			7, 6, 4,
			6, 1, 4,
			// Back face
			7, 5, 6,
			5, 3, 6,
			// Top face
			7, 4, 5,
			4, 2, 5,
		};
		// public IEnumerable<Triangle> Triangles => new[] {
		// 	// Bottom face
		// 	new Triangle(Anchor, points[1], points[3]),
		// 	new Triangle(points[1], points[6], points[3]),
		// 	// Front face
		// 	new Triangle(Anchor, points[2], points[1]),
		// 	new Triangle(points[2], points[4], points[1]),
		// 	// Left face
		// 	new Triangle(Anchor, points[3], points[2]),
		// 	new Triangle(points[3], points[5], points[2]),
		// 	// Right face
		// 	new Triangle(Extent, points[6], points[4]),
		// 	new Triangle(points[6], points[1], points[4]),
		// 	// Back face
		// 	new Triangle(Extent, points[5], points[6]),
		// 	new Triangle(points[5], points[3], points[6]),
		// 	// Top face
		// 	new Triangle(Extent, points[4], points[5]),
		// 	new Triangle(points[4], points[2], points[5]),
		// };
		public IEnumerable<Triangle> Triangles => new[] {
			Face(Directions.Up, false), Face(Directions.Up, true),
			Face(Directions.Down, false), Face(Directions.Down, true),
			Face(Directions.Left, false), Face(Directions.Left, true),
			Face(Directions.Right, false), Face(Directions.Right, true),
			Face(Directions.Forward, false), Face(Directions.Forward, true),
			Face(Directions.Backward, false), Face(Directions.Backward, true),
		};

		public Triangle Face(Directions direction, bool complement) {
			return direction switch {
				Directions.Up when complement => new Triangle(points[4], points[2], points[5]),
				Directions.Up => new Triangle(Extent, points[4], points[5]),
				Directions.Down when complement => new Triangle(points[1], points[6], points[3]),
				Directions.Down => new Triangle(Anchor, points[1], points[3]),
				Directions.Left when complement => new Triangle(points[3], points[5], points[2]),
				Directions.Left => new Triangle(Anchor, points[3], points[2]),
				Directions.Right when complement => new Triangle(points[6], points[1], points[4]),
				Directions.Right => new Triangle(Extent, points[6], points[4]),
				Directions.Forward when complement => new Triangle(Extent, points[5], points[6]),
				Directions.Forward => new Triangle(points[5], points[3], points[6]),
				Directions.Backward when complement => new Triangle(Anchor, points[2], points[1]),
				Directions.Backward => new Triangle(points[2], points[4], points[1]),
				_ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
			};
		}
		public IEnumerable<Triangle> Face(Directions direction) {
			return direction switch {
				Directions.Up => new[] {Face(Directions.Up, false), Face(Directions.Up, true)},
				Directions.Down => new[] {Face(Directions.Down, false), Face(Directions.Down, true)},
				Directions.Left => new[] {Face(Directions.Left, false), Face(Directions.Left, true)},
				Directions.Right => new[] {Face(Directions.Right, false), Face(Directions.Right, true)},
				Directions.Forward => new[] {Face(Directions.Forward, false), Face(Directions.Forward, true)},
				Directions.Backward => new[] {Face(Directions.Backward, false), Face(Directions.Backward, true)},
				_ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
			};
		}
		public IEnumerable<Triangle> Faces(IEnumerable<Directions> directions) {
			Cuboid cuboid = this;
			return directions.SelectMany(direction => cuboid.Face(direction));
			// return directions.ToDictionary(direction => direction, direction => cuboid.Face(direction));
		}

		public bool Intersects(Tract tract) {
			return tract.Segments.Any(Intersects);
		}
		public bool Intersects(Segment segment) {
			if (Intersects(segment.Start) || Intersects(segment.End)) {
				return true;
			}

			var size = segment.Size;
			var x = new Geometry.Range((Anchor.x - segment.Start.x) / size.x, (Extent.x - segment.Start.x) / size.x);
			var y = new Geometry.Range((Anchor.y - segment.Start.y) / size.y, (Extent.y - segment.Start.y) / size.y);
			var z = new Geometry.Range((Anchor.z - segment.Start.z) / size.z, (Extent.z - segment.Start.z) / size.z);
			var min = Math.Min(1, Math.Min(x.Maximum, Math.Min(y.Maximum, z.Maximum)));
			var max = Math.Max(0, Math.Max(x.Minimum, Math.Max(y.Minimum, z.Minimum)));

			return max <= min;
		}
		public bool Intersects(Vector3 point) {
			// Special case only for axis aligned rectangle-boxes:
			return 
				point.x >= Anchor.x && 
				point.y >= Anchor.y && 
				point.z >= Anchor.z && 
				point.x <= Anchor.x + Size.x && 
				point.y <= Anchor.y + Size.y && 
				point.z <= Anchor.z + Size.z;
			
			var u = points[1] - Anchor;
			var v = points[2] - Anchor;
			var w = points[3] - Anchor;
			// General case for a parallelepiped (using the normal to the face instead of the lattice):
			// var u = Vector3.Cross(points[2] - Anchor, points[3] - Anchor);
			// var v = Vector3.Cross(points[3] - Anchor, points[1] - Anchor);
			// var w = Vector3.Cross(points[1] - Anchor, points[2] - Anchor);
			
			var ux = Vector3.Dot(u, point);
			var vy = Vector3.Dot(v, point);
			var wz = Vector3.Dot(w, point);
			
			return 
				Vector3.Dot(u, Anchor) <= ux && ux <= Vector3.Dot(u, points[1]) &&
				Vector3.Dot(v, Anchor) <= vy && vy <= Vector3.Dot(v, points[2]) &&
				Vector3.Dot(w, Anchor) <= wz && wz <= Vector3.Dot(w, points[3]);
		}
		
		public override int GetHashCode() {
			return HashCode.Combine(Anchor, Extent);
		}
		public bool Equals(Cuboid other) {
			return Anchor == other.Anchor && Extent == other.Extent;
		}
		public override bool Equals(object obj) {
			return obj is Cuboid other && Equals(other);
		}
	}
}