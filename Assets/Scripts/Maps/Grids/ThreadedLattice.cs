using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Geometry;
using Geometry.Tracts;
using Maps.Cells;
using Objects.Concurrent;
using UnityEngine;
using UnityEngine.Rendering;

namespace Maps.Grids {
	public class ThreadedLattice {
		private Tractogram Tractogram {get;}
		private ConcurrentPipe<Tuple<Cell, Tract>> Bag {get;}
		private float Resolution {get;}
		private Lattice Lattice {get;}
		public Cuboid?[] Cells {get;}
		public Index3 Size => Lattice.Size;
		public Boundaries Boundaries => new((Vector3) Lattice.Anchor * Resolution, (Vector3) (Lattice.Anchor + Lattice.Size) * Resolution);

		public ThreadedLattice(Tractogram tractogram, float resolution, ConcurrentPipe<Tuple<Cell, Tract>> bag) {
			var anchor = new Index3(tractogram.Boundaries.Min, resolution);
			var size = new Index3(tractogram.Boundaries.Max, resolution) + new Index3(1, 1, 1) - anchor;
			Tractogram = tractogram;
			Bag = bag;
			Resolution = resolution;
			Lattice = new Lattice(anchor, size, resolution);
			Cells = new Cuboid?[size.x * size.y * size.z];

			Debug.Log("Set step size for grid to " + resolution);
			Debug.Log("And initialized a grid with size " + Lattice.Size);
			Debug.Log("And offset " + Lattice.Anchor);
		}
		
		protected Index3 Index(Vector3 vector) {
			return new Index3(vector, Resolution) - Lattice.Anchor;
		}
		private Cuboid? Get(Index3 index) {
			return Cells[index.x + index.y * Lattice.Size.x + index.z * Lattice.Size.x * Lattice.Size.y];
		}
		private void Set(Index3 index, Cuboid cell) {
			Cells[index.x + index.y * Lattice.Size.x + index.z * Lattice.Size.x * Lattice.Size.y] = cell;
		}
		public Cuboid Quantize(Vector3 vector) {
			return Quantize(Index(vector));
		}
		protected Cuboid Quantize(Index3 index) {
			var cell = Get(index);
			if (cell == null) {
				cell = new Cuboid(Lattice, index);
				Set(index, (Cuboid) cell);
				// throw new IndexOutOfRangeException("There is no cell defined at this position");
			}
			return (Cuboid) cell;
		}
		
		public void Start() {
			// var segments = new Dictionary<Segment, Tract>();
			var segments = new List<DistancedSegment>();

			// First get all cells from tract points which is much easier
			foreach (var tract in Tractogram.Tracts) {
				// Initialize the index of the first point, to avoid having to deal with nulls
				Index3 previous = Index(tract.Points[0]);
				Produce(previous, tract);

				for (var i = 1; i < tract.Points.Length; i++) {
					// Get the index of the cell this point is in
					var index = Index(tract.Points[i]);
					
					// If we're at the same index, no further checks needed
					if (index != previous) {
						Produce(index, tract);

						// If the manhattan distance between the current and previous indices is larger than one, they are connected diagonally (or worse), and we need to check the line segment for intersections
						if ((index - previous).Length > 1) {
							segments.Add(new DistancedSegment(new Segment(tract.Points[i - 1], tract.Points[i]), tract));
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
				// var limit = (end - current).Length;
				try {
					// Iteratively move to neighbouring cells as determined by intersections with the segment to find the total list of cells
					while (current != end) {
						// steps++;
						var directions = current.Directions(end).ToArray();

						// If there is only one direction, we know we must go that way
						if (directions.Length == 1) {
							var next = current + directions[0];
							Produce(next, pair.Tract);
							current = next;
							continue;
						}
						
						// If there are multiple directions, find the one for which the segment crosses the bordering face
						// Also keep track of whether an intersection is found to prevent infinite loops
						var intersected = false;
						foreach (
							// var next in directions
							// 	.Select(direction => current + direction)
							// 	.Where(step => Quantize(step).Intersects(pair.Segment))
							var next in directions
								.ToDictionary(direction => direction, direction => Quantize(current).Face(direction))
								.Where(step => step.Value.Any(triangle => triangle.Intersects(pair.Segment)))
								.Select(step => current + step.Key)
						) {
							Produce(next, pair.Tract);
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
						// if (steps >= limit) {
						// 	throw new ArithmeticException("Exceeded maximum amount of steps to quantize a line segment");
						// }
					}
				} catch (ArithmeticException error) {
					Debug.Log("An error occurred finding corresponding voxels for a line segment from "+pair.Segment.Start+" to "+pair.Segment.End+", and this tract might thus be missing representation in some voxels");
					Debug.Log(error);
					throw;
				}
			}

			Bag.Complete();
		}
		private void Produce(Index3 index, Tract tract) {
			Bag.Add(new Tuple<Cell, Tract>(Quantize(index), tract));
		}
		
		public Model Render(Dictionary<Cell, Color32> map) {
			var model = new Model();

			foreach (var cell in Cells) {
				if (cell != null && map.ContainsKey(cell)) {
					var value = (Cuboid) cell;
					model.Indices.AddRange(value.Indices.Select(index => index + model.Vertices.Count));
					model.Vertices.AddRange(value.Vertices);
					model.Colors.AddRange(value.Vertices.Select(_ => map[cell]));
				}
			}

			return model;
		}
		
		private record DistancedSegment(Segment Segment, Tract Tract) {
			public Segment Segment {get; private set;} = Segment;
			public Tract Tract {get; private set;} = Tract;
		}
	}
}