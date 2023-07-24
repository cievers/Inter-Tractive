using System;
using UnityEngine;

namespace Geometry {
	public readonly struct Segment {
		public Vector3 Start {get;}
		public Vector3 End {get;}
		public Vector3 Size => End - Start;
		public Vector3[] Points => new[] {Start, End};

		public Segment(Vector3 start, Vector3 end) {
			Start = start;
			End = end;
		}
		public Boundaries Boundaries => new(new Vector3(Math.Min(Start.x, End.x), Math.Min(Start.y, End.y), Math.Min(Start.z, End.z)), new Vector3(Math.Max(Start.x, End.x), Math.Max(Start.y, End.y), Math.Max(Start.z, End.z)));

		public override string ToString() {
			return "From " + Start + " to " + End;
		}
	}
}