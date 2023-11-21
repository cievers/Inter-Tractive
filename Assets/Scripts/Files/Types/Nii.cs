using System;
using System.IO;
using Geometry;
using UnityEngine;

namespace Files.Types {
	public record Nii<T>(T[] Values, Index3 Composition, Affine Transformation, Vector3 Size, int Measurements=1) : Publication.Nii<T> {
		public T[] Values {get;} = Values;
		public int Measurements {get;} = Measurements;
		public Index3 Composition {get;} = Composition;
		public Affine Transformation {get;} = Transformation;
		public Vector3 Size {get;} = Size;

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
			
			// Ignore pixel dimensions in bytes 76-107
			// Ignore pixel dimension units in byte 123
			// Ignore quaternion transformation in bytes 256-279
			
			// A whole mess of transformation options
			
			var header = filestream.Read(READ_BUFFER, 0, 352);
			var composition = new Index3(LoadShort(42), LoadShort(44), LoadShort(46));
			var transformation = LoadAffine(280);
			var size = transformation.Transform(Vector3.one) - transformation.Offset(); // Seems hacky as this would also include rotation and things like shear
			var body = new float[composition.x * composition.y * composition.z];
			var read = 0;
			int batch;

			do {
				batch = filestream.Read(READ_BUFFER, 0, READ_BUFFER.Length);
				for (var processed = 0; processed < batch / FLOAT_SIZE; processed++) {
					// Debug.Log("Reading value "+read+": "+LoadFloat(processed * FLOAT_SIZE));
					body[read] = LoadFloat(processed * FLOAT_SIZE);
					read++;
				}
			} while (batch > 0);
			
			return new Nii<float>(body, composition, transformation, size);
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
		private static Affine LoadAffine(int offset) {
			return new Affine(LoadFloats(offset, 4), LoadFloats(offset + 16, 4), LoadFloats(offset + 32, 4));
		}
	}
}