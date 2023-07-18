using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Geometry;
using Geometry.Tracts;
using Maps.Cells;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Maps.Grids {
	public class ArrayIntersectionGrid : ArrayGrid {
		public ArrayIntersectionGrid(Tractogram tractogram, float resolution) : base(tractogram, resolution) {}
		
		public override Dictionary<Cell, IEnumerable<Tract>> Quantize(Tractogram tractogram) {
			var result = new Dictionary<Index3, HashSet<Tract>>();

			// var segments = new Dictionary<Segment, Tract>();
			var segments = new List<DistancedSegment>();

			// First get all cells from tract points which is much easier
			foreach (var tract in tractogram.Tracts) {
				// Initialize the index of the first point, to avoid having to deal with nulls
				Index3 previous = Index(tract.Points[0]);
				// If it's the first time at this index, make sure an entry exists in the dictionary
				if (!result.ContainsKey(previous)) {
					result.Add(previous, new HashSet<Tract>());
				}
				result[previous].Add(tract);

				for (var i = 1; i < tract.Points.Length; i++) {
					// Get the index of the cell this point is in
					var index = Index(tract.Points[i]);
					
					// If we're at the same index, no further checks needed
					if (index != previous) {
						// If it's the first time at this index, make sure an entry exists in the dictionary
						if (!result.ContainsKey(index)) {
							result.Add(index, new HashSet<Tract>());
						}
						result[index].Add(tract);

						// If the manhattan distance between the current and previous indices is larger than one, they are connected diagonally (or worse), and we need to check the line segment for intersections
						if ((index - previous).Length > 1) {
							try {
								segments.Add(new DistancedSegment(new Segment(tract.Points[i - 1], tract.Points[i]), tract));
							} catch (ArgumentException error) {
								Debug.Log(error);
								Debug.Log(segments.Count);
								foreach (var segment in segments) {
									Debug.Log(segment);
								}
								Debug.Log("----");
								Debug.Log(new Segment(tract.Points[i - 1], tract.Points[i]));
								throw;
							}
						}
					}
					previous = index;
				}
			}

			// And for the marked segments get more accurate cells through 3D intersections
			foreach (var pair in segments) {
				var current = Index(pair.Segment.Start);
				var end = Index(pair.Segment.End);
				// var steps = 0;
				try {
					// Iteratively move to neighbouring cells as determined by intersections with the segment to find the total list of cells
					while (current != end) {
						// steps++;
						var directions = current.Directions(end).ToArray();
						
						// If there is only one direction, we know we must go that way
						if (directions.Length == 1) {
							var next = current + directions[0];
							if (!result.ContainsKey(next)) {
								result.Add(next, new HashSet<Tract>());
							}
							result[next].Add(pair.Tract);
							current = next;
							continue;
						}
						
						// If there are multiple directions, find the one for which the segment crosses the bordering face
						// Also keep track of whether an intersection is found to prevent infinite loops
						var intersected = false;
						foreach (
							var next in directions
								.ToDictionary(direction => direction, direction => Quantize(current).Face(direction))
								.Where(step => step.Value.Any(triangle => triangle.Intersects(pair.Segment)))
								.Select(step => current + step.Key)
						) {
							if (!result.ContainsKey(next)) {
								result.Add(next, new HashSet<Tract>());
							}
							result[next].Add(pair.Tract);
							current = next;
							intersected = true;
							break;
						}
						// foreach (var step in current.Step(end)) {
						// 	// TODO: If the segment does not intersect, this Quantize call creates an empty cell
						// 	if (Quantize(step).Intersects(pair.Key)) {
						// 		if (!result.ContainsKey(step)) {
						// 			result.Add(step, new HashSet<Tract>());
						// 		}
						// 		result[step].Add(pair.Value);
						// 		current = step;
						// 		intersected = true;
						// 		break;
						// 	}
						// }
						if (!intersected) {
							// Somehow this segment does not intersect any face towards its endpoint
							// This likely occurs because of floating point errors, where the segment just intersected with an entering face, but barely misses intersecting with an exiting face
							throw new ArithmeticException("Segment " + pair.Segment + " did not intersect with any neighbouring cells towards the goal");
						}
						// TODO: It should be possible to place a bound on the number of steps through knowing the cell size and maximum separation of points in a tract
						// if (steps >= 10) {
						// 	throw new ArithmeticException("Exceeded maximum amount of steps to quantize a line segment");
						// }
					}
				} catch (ArithmeticException) {
					Debug.Log("An error occurred finding corresponding voxels for a line segment from "+pair.Segment.Start+" to "+pair.Segment.End+", and this tract might thus be missing representation in some voxels");
				}
			}

			return result.ToDictionary(pair => (Cell) Quantize(pair.Key), pair => pair.Value.AsEnumerable());
		}
		
		private record DistancedSegment(Segment Segment, Tract Tract) {
			public Segment Segment {get; private set;} = Segment;
			public Tract Tract {get; private set;} = Tract;
		}
	}
}