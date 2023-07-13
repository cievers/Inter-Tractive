using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Geometry {
	public readonly struct Index3 {
		public readonly int x;
		public readonly int y;
		public readonly int z;
			
		public Index3(int x, int y, int z) {
			this.x = x;
			this.y = y;
			this.z = z;
		}
		public Index3(float x, float y, float z, float scale) {
			this.x = (int) Math.Floor(x / scale);
			this.y = (int) Math.Floor(y / scale);
			this.z = (int) Math.Floor(z / scale);
		}
		public Index3(Vector3 vector, float scale) {
			x = (int) Math.Floor(vector.x / scale);
			y = (int) Math.Floor(vector.y / scale);
			z = (int) Math.Floor(vector.z / scale);
		}

		public int Length => Math.Abs(x) + Math.Abs(y) + Math.Abs(z);

		public IEnumerable<Index3> Step(Index3 goal) {
			var result = new List<Index3>(3);
			if (goal.x != x) {
				result.Add(this + new Index3(Math.Sign(goal.x - x), 0, 0));
			}
			if (goal.y != y) {
				result.Add(this + new Index3(0, Math.Sign(goal.y - y), 0));
			}
			if (goal.z != z) {
				result.Add(this + new Index3(0, 0, Math.Sign(goal.z - z)));
			}
			return result;
		}
		public IEnumerable<Directions> Directions(Index3 goal) {
			var result = new List<Directions>(3);
			if (goal.x < x) {
				result.Add(Geometry.Directions.Left);
			} else if (x < goal.x) {
				result.Add(Geometry.Directions.Right);
			}
			if (goal.y < y) {
				result.Add(Geometry.Directions.Down);
			} else if (y < goal.y) {
				result.Add(Geometry.Directions.Up);
			}
			if (goal.z < z) {
				result.Add(Geometry.Directions.Backward);
			} else if (z < goal.z) {
				result.Add(Geometry.Directions.Forward);
			}
			return result;
		}

		public override string ToString() {
			return "("+x+", "+y+", "+z+")";
		}
		public static implicit operator Vector3(Index3 index) => new(index.x, index.y, index.z);
		public static Index3 operator +(Index3 a, Index3 b) => new(a.x + b.x, a.y + b.y, a.z + b.z);
		public static Index3 operator +(Index3 a, Directions b) => a + b.Index();
		public static Index3 operator -(Index3 a, Index3 b) => new(a.x - b.x, a.y - b.y, a.z - b.z);
		public static Index3 operator -(Index3 a, Directions b) => a - b.Index();
		
		public override int GetHashCode() {
			return HashCode.Combine(x, y, z);
		}
		public bool Equals(Index3 other) {
			return x == other.x
				&& y == other.y
				&& z == other.z;
		}
		public override bool Equals(object obj) {
			return obj is Index3 other && Equals(other);
		}
		public static bool operator ==(Index3 a, Index3 b) => a.x == b.x && a.y == b.y && a.z == b.z;
		public static bool operator !=(Index3 a, Index3 b) => !(a == b);
	}
}