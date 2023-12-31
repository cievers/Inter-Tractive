﻿using System;
using System.Collections.Generic;
using System.Linq;
using Geometry;
using Geometry.Tracts;
using Maps.Cells;
using UnityEngine;
using UnityEngine.Rendering;

namespace Maps.Grids {
	public class IntersectionTree : Voxels {
		public float CellSize {get;}
		public Lattice Lattice {get;}
		public Cuboid?[] Cells {get;}

		private Dictionary<Index3, IEnumerable<Tract>> quantization;

		public IntersectionTree(Tractogram tractogram, int depth, float padding) {
			var resolution = (new[] {
				Math.Abs(tractogram.Boundaries.Size.x),
				Math.Abs(tractogram.Boundaries.Size.y),
				Math.Abs(tractogram.Boundaries.Size.z)
			}.Max() + padding) / (depth + 1);
			// var size = new Index3(tractogram.Boundaries.Max, resolution) + new Index3(1, 1, 1) - anchor;
			var size = new Index3(depth + 1, depth + 1, depth + 1);
			Lattice = new Lattice(tractogram.Boundaries.Min, new Index3(0, 0, 0), size, resolution);
			Cells = new Cuboid?[size.x * size.y * size.z];
			CellSize = resolution;

			quantization = Quantize(tractogram);
		}
		public IntersectionTree(Lattice lattice, Dictionary<Index3, IEnumerable<Tract>> cells) {
			Lattice = lattice;
			Cells = new Cuboid?[lattice.Composition.x * lattice.Composition.y * lattice.Composition.z];
			CellSize = lattice.Unit.x; // TODO: This is a horrible assumption. Somehow unify that this thing uses universal cell size in all dimensions, and Lattices can deal with size differences
			quantization = cells;
		}
		
		public Index3 Anchor => Lattice.Anchor;
		public Index3 Size => Lattice.Composition;
		public Boundaries Boundaries => new((Vector3) Anchor * CellSize + Lattice.Origin, (Vector3) (Anchor + Size) * CellSize + Lattice.Origin);

		public Dictionary<Index3, IEnumerable<Tract>> Quantization => quantization;
		public Dictionary<Cell, IEnumerable<Tract>> Voxels => Voxelize(quantization);
		
		protected Index3 Index(Vector3 vector) {
			var result = new Index3(vector - Lattice.Origin, CellSize) - Anchor;
			// if (result != new Index3(0, 0, 0)) {
			// 	// TODO: This is mathematically expected, as we set the total grid boundaries to match exactly the
			// 	// boundaries of the tractogram, but then expect the points on that exact upper boundary to still
			// 	// somehow round down to an index inside the grid.
			// 	Debug.Log("Error when getting index for vector");
			// 	Debug.Log(vector);
			// 	Debug.Log(vector - Lattice.Origin);
			// 	Debug.Log(Lattice.Origin);
			// 	Debug.Log(Lattice.Anchor);
			// 	Debug.Log(Lattice.Cell);
			// 	Debug.Log(CellSize);
			// 	Debug.Log(result);
			// 	throw new ArithmeticException();
			// }
			return result;
		}
		private Cuboid? Get(Index3 index) {
			return Cells[index.x + index.y * Size.x + index.z * Size.x * Size.y];
		}
		private void Set(Index3 index, Cuboid cell) {
			Cells[index.x + index.y * Size.x + index.z * Size.x * Size.y] = cell;
		}
		public Cuboid Voxelize(Vector3 vector) {
			return Voxelize(Index(vector));
		}
		protected Cuboid Voxelize(Index3 index) {
			var cell = Get(index);
			if (cell == null) {
				cell = new Cuboid(Lattice, index);
				Set(index, (Cuboid) cell);
				// throw new IndexOutOfRangeException("There is no cell defined at this position");
			}
			return (Cuboid) cell;
		}
		
		public Dictionary<Cell, IEnumerable<Tract>> Voxelize(Tractogram tractogram) {
			return Voxelize(Quantize(tractogram));
		}
		public Dictionary<Cell, IEnumerable<Tract>> Voxelize(Dictionary<Index3, IEnumerable<Tract>> quantization) {
			return quantization.ToDictionary(pair => (Cell) Voxelize(pair.Key), pair => pair.Value.AsEnumerable());
		}
		public Dictionary<Index3, IEnumerable<Tract>> Quantize(Tractogram tractogram) {
			var result = new Dictionary<Index3, HashSet<Tract>>();

			// var segments = new Dictionary<Segment, Tract>();
			var distanced = new List<RepresentativeSegment>();

			// First get all cells from tract points which is much easier
			// foreach (var tract in tractogram.Tracts) {
			// 	// Initialize the index of the first point, to avoid having to deal with nulls
			// 	Index3 previous = Index(tract.Points[0]);
			// 	// If it's the first time at this index, make sure an entry exists in the dictionary
			// 	if (!result.ContainsKey(previous)) {
			// 		result.Add(previous, new HashSet<Tract>());
			// 	}
			// 	result[previous].Add(tract);
			//
			// 	for (var i = 1; i < tract.Points.Length; i++) {
			// 		// Get the index of the cell this point is in
			// 		var index = Index(tract.Points[i]);
			// 		
			// 		// If we're at the same index, no further checks needed
			// 		if (index != previous) {
			// 			// If it's the first time at this index, make sure an entry exists in the dictionary
			// 			if (!result.ContainsKey(index)) {
			// 				result.Add(index, new HashSet<Tract>());
			// 			}
			// 			result[index].Add(tract);
			//
			// 			// If the manhattan distance between the current and previous indices is larger than one, they are connected diagonally (or worse), and we need to check the line segment for intersections
			// 			if ((index - previous).Length > 1) {
			// 				distanced.Add(new RepresentativeSegment(new Segment(tract.Points[i - 1], tract.Points[i]), tract));
			// 			}
			// 		}
			// 		previous = index;
			// 	}
			// }

			// TODO: Without adjustments in/in the use of the resulting voxels, voxels with a point from a tract are represented by both segments, and the tract would thus be measured twice
			// First get all cells from tract points which is much easier
			// Because we later want to scale we need to know the exact intersections with each voxel, so for each point add both segments
			foreach (var tract in tractogram.Tracts) {
				foreach (var segment in tract.Segments) {
					foreach (var point in segment.Points) {
						var index = Index(point);
						// If it's the first time at this index, make sure an entry exists in the dictionary
						if (!result.ContainsKey(index)) {
							result.Add(index, new HashSet<Tract>());
						}
						result[index].Add(new RepresentativeSegment(segment, tract));
					}
					// If the manhattan distance between the current and previous indices is larger than one, they are connected diagonally (or worse), and we need to check the line segment for intersections
					if ((Index(segment.Start) - Index(segment.End)).Length > 1) {
						distanced.Add(new RepresentativeSegment(segment, tract));
					}
				}
			}

			// And for the marked segments get more accurate cells through 3D intersections
			foreach (var pair in distanced) {
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
							if (!result.ContainsKey(next)) {
								result.Add(next, new HashSet<Tract>());
							}
							result[next].Add(pair);
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
								.ToDictionary(direction => direction, direction => Voxelize(current).Face(direction))
								.Where(step => step.Value.Any(triangle => triangle.Intersects(pair.Segment)))
								.Select(step => current + step.Key)
						) {
							if (!result.ContainsKey(next)) {
								result.Add(next, new HashSet<Tract>());
							}
							result[next].Add(pair);
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

			return result.ToDictionary(pair => pair.Key, pair => pair.Value.AsEnumerable());
		}
		public static IntersectionTree Divide(Dictionary<Index3, IEnumerable<Tract>> voxels, Lattice lattice) {
			var points = lattice.Divide();
			var result = new Dictionary<Index3, HashSet<Tract>>();

			foreach (var voxel in voxels) {
				var divisions = new[] {
					voxel.Key * 2,
					voxel.Key * 2 + new Index3(1, 0, 0),
					voxel.Key * 2 + new Index3(0, 1, 0),
					voxel.Key * 2 + new Index3(0, 0, 1),
					voxel.Key * 2 + new Index3(1, 1, 0),
					voxel.Key * 2 + new Index3(0, 1, 1),
					voxel.Key * 2 + new Index3(1, 0, 1),
					voxel.Key * 2 + new Index3(1, 1, 1)
				};
				foreach (var tract in voxel.Value) {
					var representation = (RepresentativeSegment) tract;
					foreach (var division in divisions) {
						if (new Cuboid(points, division).Intersects(representation.Segment)) {
							if (!result.ContainsKey(division)) {
								result.Add(division, new HashSet<Tract>());
							}
							result[division].Add(representation);
						}
					}
				}
			}
			
			return new IntersectionTree(points, result.ToDictionary(pair => pair.Key, pair => pair.Value.AsEnumerable()));
		}
		public IntersectionTree Divide() {
			return Divide(quantization, Lattice);
		}
		
		public Mesh Render(Dictionary<Cell, Color32> map) {
			var vertices = new List<Vector3>();
			var normals = new List<Vector3>(); // TODO: Normals aren't used in rendering yet
			var colors = new List<Color32>();
			var indices = new List<int>();

			foreach (var cell in map.Keys) {
				var value = (Cuboid) cell;
				indices.AddRange(value.Indices.Select(index => index + vertices.Count));
				vertices.AddRange(value.Vertices);
				colors.AddRange(value.Vertices.Select(_ => map[cell]));
			}

			var shape = new Mesh {indexFormat = IndexFormat.UInt32};

			shape.Clear();
			shape.SetVertices(vertices);
			shape.SetColors(colors);
			shape.SetTriangles(indices, 0);
			shape.RecalculateNormals();

			return shape;
		}
		
		private record RepresentativeSegment(Segment Segment, Tract Tract) : Tract {
			public Segment Segment {get; private set;} = Segment;
			public Tract Tract {get; private set;} = Tract;

			public Vector3[] Points => Tract.Points;
			public IEnumerable<Vector3> Normals => Tract.Normals;
			public IEnumerable<Segment> Segments => Tract.Segments;
			public Boundaries Boundaries => Tract.Boundaries;
			public float Resolution => Tract.Resolution;
			public float Slack => Tract.Slack;
			
			public Tract Sample(int samples) {
				return Tract.Sample(samples);
			}
		}
	}
}