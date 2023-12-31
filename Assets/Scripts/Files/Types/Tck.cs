﻿using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Geometry;
using Geometry.Tracts;
using UnityEngine;

namespace Files.Types {
	public record Tck : Tractogram {
		public string path;
		public Meta metadata;
		public Bounds boundaries;
		
		public IEnumerable<Tract> Tracts {get;}
		public IEnumerable<Vector3> Points => Tracts.SelectMany(tract => tract.Points);
		public Boundaries Boundaries => Boundaries.Join(Tracts.Select(segment => segment.Boundaries));
		public float Resolution => Tracts.Select(segment => segment.Resolution).Min();
		public float Slack => Tracts.Select(segment => segment.Slack).Max();

		public Tck(string path, Meta metadata, IEnumerable<Tract> lines, Bounds boundaries) {
			this.path = path;
			this.metadata = metadata;
			Tracts = lines;
			this.boundaries = boundaries;
		}
		
		private const int VECTOR_SIZE = sizeof(float) * 3;
		private static readonly byte[] READ_BUFFER = new byte[VECTOR_SIZE * 10000];

		public static Tck Load(string path, float interpolate=float.MaxValue) {
			if (!BitConverter.IsLittleEndian) {
				throw new Exception("No support yet for big endian architectures");
			}
			
			// Setup the file reading
			using var filestream = File.OpenRead(path);
			using var reader = new StreamReader(filestream);

			// Start by parsing off the metadata
			var meta = LoadMeta(reader);
			if (meta["datatype"] != "Float32LE") {
				throw new DataException("Only supports Float32LE for now");
			}
			
			// Set the file streamer position to the start of the vertex data
			filestream.Position = int.Parse(meta["file"].Replace(".", ""));
			
			// Prepare constants and container variables for reading the vertex data
			var points = new List<Vector3>();
			var lines = new List<Tract>();
			var boundsMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
			var boundsMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);
			var coreStart = Vector3.zero;
			var coreEnd = Vector3.zero;
			var coreWeight = 0;
			var filePointCount = (filestream.Length - filestream.Position) / VECTOR_SIZE;
			
			// lines.EnsureCapacity((int)filePointCount); // At some point it might have been desirable to ensure the list has enough memory available to store all tracts

			for (var read = 0; read < filePointCount;) {
				var batch = filestream.Read(READ_BUFFER, 0, READ_BUFFER.Length);

				for (var processed = 0; processed < batch / VECTOR_SIZE; processed++) {
					Vector3 point = GetVectorInBuffer(processed);

					if (IsTerminal(point)) {
						// If we hit the NaN, NaN, NaN marker for the end of a tract, save this tract
						if (points.Count > 0) {
							// But only if we have some points stored, so ignore successive terminators
							if (coreWeight > 0 && (points[0] - coreStart).magnitude + (points[^1] - coreEnd).magnitude > (points[0] - coreEnd).magnitude + (points[^1] - coreStart).magnitude) {
								points.Reverse();
							}
							lines.Add(new ArrayTract(points.ToArray(), (uint) lines.Count, Vector3.Normalize(points[^1] - points[0]), (uint) read));
							coreStart = (coreStart * coreWeight + points[0]) / (coreWeight + 1);
							coreEnd = (coreEnd * coreWeight + points[^1]) / (coreWeight + 1);
							coreWeight++;
							points.Clear();
						}
					} else {
						// If not, continue expanding the current tract
						UpdateBounds(ref boundsMin, ref boundsMax, point);

						// With an optional parameter, check if line segments need to be sub-sampled with interpolated points depending on this given maximum separation
						if (points.Count >= 2 && (points[^1] - point).magnitude > interpolate) {
							points.AddRange(Interpolate(points[^1], point, interpolate));
						}
						
						// And always add the original point
						points.Add(point);
					}
					read++;
				}
			}

			if (filestream.Position != filestream.Length) {
				throw new FileLoadException("Something went wrong during reading. End of file has not been reached.");
			}

			return new Tck(path, meta, lines.ToArray(), new Bounds(boundsMin, boundsMax));
		}
		private static Meta LoadMeta(TextReader reader) {
			var meta = new Meta();
			
			// First line has to be 'mrtrix tracks'
			if (LoadMetaLine(reader).Trim() != "mrtrix tracks") {
				throw new FileLoadException("File is not in MRtrix format.");
			}

			var metadataLine = LoadMetaLine(reader);

			while (metadataLine != "END") {
				var split = metadataLine.Split(':', StringSplitOptions.RemoveEmptyEntries);
				meta.Add(split[0].Trim(), split[1].Trim());
				
				metadataLine = LoadMetaLine(reader);
			}

			return meta;
		}
		private static string LoadMetaLine(TextReader reader) {
			return reader.ReadLine() ?? throw new FileLoadException("Incorrect metadata format");
		}
		private static IEnumerable<Vector3> Interpolate(Vector3 start, Vector3 end, float maximum) {
			var distance = Vector3.Distance(start, end);
			var interpolationPointCount = (int) MathF.Ceiling(distance / maximum);
			var result = new Vector3[interpolationPointCount];
							
			for (var i = 0; i < interpolationPointCount; i++) {
				// Start and endpoints are real points not interpolated.
				if (i != 0 && i != interpolationPointCount) {
					result[i - 1] = Vector3.Lerp(start, end, i / interpolationPointCount);
				}
			}

			return result;
		}
		private static Vector3 GetVectorInBuffer(int i) {
			var x = BitConverter.ToSingle(READ_BUFFER, i * VECTOR_SIZE);
			var y = BitConverter.ToSingle(READ_BUFFER, i * VECTOR_SIZE + sizeof(float));
			var z = BitConverter.ToSingle(READ_BUFFER, i * VECTOR_SIZE + sizeof(float) * 2);
			var point = new Vector3(x, z, y);
			return point;
		}
		private static bool IsTerminal(Vector3 point) {
			return 
				(float.IsNaN(point.x) && float.IsNaN(point.y) && float.IsNaN(point.z)) || 
				(float.IsInfinity(point.x) && float.IsInfinity(point.y) && float.IsInfinity(point.z));
		}

		private static void UpdateBounds(ref Vector3 boundsMin, ref Vector3 boundsMax, Vector3 point) {
			boundsMin.x = MathF.Min(boundsMin.x, point.x);
			boundsMin.y = MathF.Min(boundsMin.y, point.y);
			boundsMin.z = MathF.Min(boundsMin.z, point.z);

			boundsMax.x = MathF.Max(boundsMax.x, point.x);
			boundsMax.y = MathF.Max(boundsMax.y, point.y);
			boundsMax.z = MathF.Max(boundsMax.z, point.z);
		}
		
		public UniformTractogram Sample(int samples) {
			return new UniformTractogram(Tracts.Select(tract => tract.Sample(samples)));
		}
	}
}