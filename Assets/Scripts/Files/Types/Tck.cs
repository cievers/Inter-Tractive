using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Geometry;
using Geometry.Tracts;
using UnityEngine;

namespace Files.Types {
	public record Tck : Tractogram {
		public string Path;
		public Dictionary<string, string> Metadata;
		// public Tract[] Lines;
		public Bounds Bounds;
		
		public IEnumerable<Tract> Tracts {get;}
		// public Vector3[] Points {
		// 	get {
		// 		var result = new List<Vector3>();
		// 		foreach (var tract in Tracts) {
		// 			result.AddRange(tract.Points);
		// 		}
		// 		return result.ToArray();
		// 	}
		// }
		public IEnumerable<Vector3> Points => Tracts.SelectMany(tract => tract.Points);
		public Boundaries Boundaries => Boundaries.Join(Tracts.Select(segment => segment.Boundaries));
		public float Resolution => Tracts.Select(segment => segment.Resolution).Min();
		public float Slack => Tracts.Select(segment => segment.Slack).Max();

		public Tck(Dictionary<string, string> metadata, Tract[] lines, Bounds bounds, string path) {
			Metadata = metadata;
			Tracts = lines;
			Bounds = bounds;
			Path = path;
		}
		
		private const int VOXEL_SIZE = 10;
		private const int VECTOR_SIZE = sizeof(float) * 3;
		private static readonly byte[] READ_BUFFER = new byte[VECTOR_SIZE * 10000];

		public static Tck Load(string path) {
			if (!BitConverter.IsLittleEndian) throw new Exception("No support yet for big endian architectures");

			List<Vector3> points = new();
			List<Tract> lines = new();

			var meta = new Dictionary<string, string>();

			using var filestream = File.OpenRead(path);
			using var reader = new StreamReader(filestream);

			//first line has to be 'mrtrix tracks'
			if ((reader.ReadLine() ?? throw new Exception("Incorrect metadata format")).Trim() != "mrtrix tracks") throw new Exception("File is not in MRtrix format.");

			var metadataLine = reader.ReadLine() ?? throw new Exception("Incorrect metadata format");

			while (metadataLine != "END") {
				Debug.Log("Reading metedata: "+metadataLine);
				// var splitted = metadataLine.Split(':', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
				var splitted = metadataLine.Split(':', StringSplitOptions.RemoveEmptyEntries); // TODO: Possibly trim whitespace from resulting splits
				meta.Add(splitted[0].Trim(), splitted[1].Trim());
				metadataLine = reader.ReadLine() ?? throw new Exception("Incorrect metadata format");
			}

			if (meta["datatype"] != "Float32LE") throw new Exception("Only supports Float32LE for now");

			var binaryInfoOffset = int.Parse(meta["file"].Replace(".", ""));
			filestream.Position = binaryInfoOffset;

			var boundsMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
			var boundsMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);

			var maxDistance = VOXEL_SIZE * .98f;
			var maxDistanceSqrt = maxDistance * maxDistance;

			//read all points except last point.
			long filePointCount = (filestream.Length - filestream.Position) / VECTOR_SIZE - 1;
			// lines.EnsureCapacity((int)filePointCount); // TODO: If we get some capacity/out of bounds errors, look here


			for (int v = 0; v < filePointCount - 1;) {
				int readLength = filestream.Read(READ_BUFFER, 0, READ_BUFFER.Length);

				for (int i = 0; i < readLength / VECTOR_SIZE; i++) {
					if (v >= filePointCount - 1) break;

					Vector3 point = GetVectorInBuffer(i);

					if (float.IsNaN(point.x) && float.IsNaN(point.y) && float.IsNaN(point.z)) {
						Vector3[] pointsArray = points.ToArray();
						Vector3 averageDirection = Vector3.Normalize(points[points.Count - 1] - points[0]);
						lines.Add(new Tract(pointsArray, (uint)lines.Count, averageDirection, (uint)v));
						points.Clear();
					} else {
						UpdateBounds(ref boundsMin, ref boundsMax, point);

						if (points.Count >= 2 && (points[points.Count - 1] - point).sqrMagnitude > maxDistanceSqrt) {
							var dis = Vector3.Distance(points[points.Count - 1], point);
							var interpolationPointCount = MathF.Ceiling(dis / maxDistance);
							for (int j = 0; j < interpolationPointCount; j++) {
								//start and endpoints are real points not interpolated.
								if (j == interpolationPointCount || j == 0) continue;

								points.Add(Vector3.Lerp(points[points.Count - 1], point, j / interpolationPointCount));
							}
						}
						points.Add(point);
					}
					v++;
				}
			}
			// //check last point is the correct seperator.
			// if (GetVectorInBuffer(v) != new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity))
			//     throw new Exception("Something went wrong during reading. Final point is not (inf,inf,inf).");

			if (filestream.Position != filestream.Length) throw new Exception("Something went wrong during reading. End of file has not been reached.");

			return new Tck(meta, lines.ToArray(), new Bounds(boundsMin, boundsMax), path);
		}

		private static Vector3 GetVectorInBuffer(int i) {
			float x = BitConverter.ToSingle(READ_BUFFER, i * VECTOR_SIZE);
			float y = BitConverter.ToSingle(READ_BUFFER, i * VECTOR_SIZE + sizeof(float));
			float z = BitConverter.ToSingle(READ_BUFFER, i * VECTOR_SIZE + sizeof(float) * 2);
			var point = new Vector3(x, z, y);
			return point;
		}

		private static void UpdateBounds(ref Vector3 boundsMin, ref Vector3 boundsMax, Vector3 point) {
			boundsMin.x = MathF.Min(boundsMin.x, point.x);
			boundsMin.y = MathF.Min(boundsMin.y, point.y);
			boundsMin.z = MathF.Min(boundsMin.z, point.z);

			boundsMax.x = MathF.Max(boundsMax.x, point.x);
			boundsMax.y = MathF.Max(boundsMax.y, point.y);
			boundsMax.z = MathF.Max(boundsMax.z, point.z);
		}
	}
}