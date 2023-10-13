using System;
using UnityEngine;

namespace Geometry.Planar {
	public readonly struct Segment {
		public Vector2 Start {get;}
		public Vector2 End {get;}
		public Vector2 Size => End - Start;
		public Vector2[] Points => new[] {Start, End};

		public Segment(Vector2 start, Vector2 end) {
			Start = start;
			End = end;
		}
		public Boundaries Boundaries => new(new Vector2(Math.Min(Start.x, End.x), Math.Min(Start.y, End.y)), new Vector2(Math.Max(Start.x, End.x), Math.Max(Start.y, End.y)));

		public override string ToString() {
			return "From " + Start + " to " + End;
		}
	}
}