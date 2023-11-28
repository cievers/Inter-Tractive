using UnityEngine;

namespace Geometry.Planar {
	public struct Line {
		public Vector2 Origin {get;}
		public Vector2 Direction {get;}

		public Line(Vector2 origin, Vector2 direction) {
			Origin = origin;
			Direction = direction;
		}

		public Side Side(Vector2 point) {
			return (Direction.x * (point.y - Origin.y) - Direction.y * (point.x - Origin.x)) switch {
				> 0 => Geometry.Side.Positive,
				< 0 => Geometry.Side.Negative,
				_ => Geometry.Side.Contained
			};
		}
		public Vector2? Intersection(Line line) {
			var denominator = line.Direction.x * Direction.x - line.Direction.y * Direction.y;
			if (denominator == 0) {
				return null;
			}
			var a = Origin.y - line.Origin.y;
			var b = Origin.x - line.Origin.x;
			var c = (line.Direction.x * a - line.Direction.y * b) / denominator;
			
			return new Vector2(Origin.x + c * Direction.x, Origin.y + c * Direction.y);
		}
		public Vector2? Intersection(Segment segment) {
			// If the lines intersect, the result contains the x and y of the intersection (treating the lines as infinite) and booleans for whether line segment 1 or line segment 2 contain the point
			var denominator = 
				(segment.End.y - segment.Start.y) * Direction.x - 
				(segment.End.x - segment.Start.x) * Direction.y;
			if (denominator == 0) {
				return null;
			}
			var a = Origin.y - segment.Start.y;
			var b = Origin.x - segment.Start.x;
			var c = ((segment.End.x - segment.Start.x) * a - (segment.End.y - segment.Start.y) * b) / denominator;
			var d = (Direction.x * a - Direction.y * b) / denominator;

			// If we cast these lines infinitely in both directions, they intersect here:
			var x = Origin.x + c * Direction.x;
			var y = Origin.y + c * Direction.y;
			
			// If line2 is a segment and line1 is infinite, they intersect if:
			if (d is > 0 and < 1) {
				return new Vector2(x, y);
			}
			// If line1 and line2 are segments, they intersect if both of the above are true
			return null;
		}
	}
}