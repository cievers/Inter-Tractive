using System;
using System.IO;
using Geometry;
using UnityEngine;

namespace Files.Types {
	public record Nii<T>(T[] Values, Index3 Composition, Vector3 Offset, Vector3 Unit, int Measurements=1) : Publication.Nii<T> {
		public T[] Values {get;} = Values;
		public int Measurements {get;} = Measurements;
		public Index3 Composition {get;} = Composition;
		public Vector3 Offset {get;} = Offset;
		public Vector3 Unit {get;} = Unit;

		private static readonly int FLOAT_SIZE = 4;
		private static readonly byte[] READ_BUFFER = new byte[352];

		public static Nii<float> Load(string path) {
			
			// Setup the file reading
			using var filestream = File.OpenRead(path);
			using var reader = new StreamReader(filestream);
			
			// Assumptions
			// No extra data, default header length and minimal padding
			// Only three dimensions
			// Data type float, bytes 70-71 are short 16, and bytes 72-73 are short 32
			// Bytes 108-111 are float 352
			// Bytes 112-115 are float 1
			// Bytes 116-119 are float 0
			// Affine transformation has a 0 rotation matrix, except the diagonal is 1
			
			// Ignore pixel dimensions in bytes 76-107
			// Ignore pixel dimension units in byte 123
			// Ignore quaternion transformation in bytes 256-279
			// Ignore transformation part of affine transformation in bytes 280-291, 296-307, 312-323
			
			// A whole mess of transformation options
			
			var header = filestream.Read(READ_BUFFER, 0, 352);
			var composition = new Index3(LoadShort(42), LoadShort(46), LoadShort(44));
			var offset = new Vector3(LoadFloat(280+12), LoadFloat(280+44), LoadFloat(280+28));
			var size = new Vector3(LoadFloat(280), LoadFloat(280 + 40), LoadFloat(280 + 20));
			var body = new float[composition.x * composition.y * composition.z];
			var read = 0;
			int batch;

			do {
				batch = filestream.Read(READ_BUFFER, 0, READ_BUFFER.Length);
				for (var processed = 0; processed < batch / FLOAT_SIZE; processed++) {
					body[read++] = LoadFloat(processed * FLOAT_SIZE);
				}
			} while (batch > 0);
			
			var transposed = new float[composition.x * composition.y * composition.z];
			var swapped = 0;
			for (var y = 0; y < composition.y; y++) {
				for (var z = 0; z < composition.z; z++) {
					for (var x = 0; x < composition.x; x++) {
						// Both ways around it works, though one of them might be preferable when integrating with buffered loading
						// transposed[swapped++] = body[x + y * composition.x + z * composition.x * composition.y];
						transposed[x + y * composition.x + z * composition.x * composition.y] = body[swapped++];
					}
				}
			}
			
			return new Nii<float>(transposed, composition, offset, size);
		}
		private static short LoadShort(int offset) {
			return BitConverter.ToInt16(READ_BUFFER, offset);
		}
		private static float LoadFloat(int offset) {
			return BitConverter.ToSingle(READ_BUFFER, offset);
		}
		private static float[] LoadFloats(int offset, int count) {
			var result = new float[count];
			for (var i = 0; i < count; i++) {
				result[i] = LoadFloat(offset + i * FLOAT_SIZE);
			}
			return result;
		}
	}
}