using System;
using System.Collections.Generic;
using System.Linq;
using Geometry;
using UnityEngine;

namespace Files.Publication {
	public interface Nii<T> : Publication {
		public T[] Values {get;}
		public int Measurements {get;}
		public Index3 Composition {get;}
		public Affine Transformation {get;}
		public Vector3 Unit {get;}
		
		void Publication.Write(string path) {
			Write(path, Header().Concat(Padding()).Concat(Body()).ToArray());
		}
		
		//https://brainder.org/2012/09/23/the-nifti-file-format/
		private byte[] Header() {
			var header = new byte[348];
			header = Fill(header, Int(348)); // Fixed header size
			header = Fill(header, Dimensions(), 40);
			header = Fill(header, DataType(), 70);
			header = Fill(header, Sizes(), 76);
			header = Fill(header, Float(352), 108); // Offset for data start (multiples of 16, fixed if assuming no extra padding)
			header = Fill(header, Float(1), 112); // Voxel value scaling
			header = Fill(header, Float(0), 116); // Voxel value offset
			header = Fill(header, Short(2), 254); // Indicate how to use the transformations below
			// header = Fill(header, Quaternion(), 256);
			header = Fill(header, Affine(), 280);
			header = Fill(header, new byte[] {0x6E}, 344);
			header = Fill(header, new byte[] {0x2B}, 345);
			header = Fill(header, new byte[] {0x31}, 346);
			return header;
		}
		private byte[] Dimensions() {
			var dimensions = new byte[16];
			dimensions = Fill(dimensions, Short(3));
			dimensions = Fill(dimensions, Short(Composition.x), 2);
			dimensions = Fill(dimensions, Short(Composition.y), 4);
			dimensions = Fill(dimensions, Short(Composition.z), 6);
			dimensions = Fill(dimensions, Short(Measurements), 8);
			dimensions = Fill(dimensions, Short(1), 10);
			dimensions = Fill(dimensions, Short(1), 12);
			dimensions = Fill(dimensions, Short(1), 14);
			return dimensions;
		}
		private byte[] Sizes() {
			// The X, y, z sizes at offsets 4, 8, and 12 are overwritten by the scaling in the affine transform. These likely still exist here for backwards compatibility or other transformation methods
			var sizes = new byte[32];
			sizes = Fill(sizes, Float(-1));
			sizes = Fill(sizes, Float(1), 4);
			sizes = Fill(sizes, Float(1), 8);
			sizes = Fill(sizes, Float(1), 12);
			sizes = Fill(sizes, Float(1), 16);
			sizes = Fill(sizes, Float(1), 20);
			sizes = Fill(sizes, Float(1), 24);
			sizes = Fill(sizes, Float(1), 28);
			return sizes;
		}
		private byte[] DataType() {
			var type = new byte[4];
			if (typeof(T) == typeof(int)) {
				type = Fill(type, Short(8));
				type = Fill(type, Short(32), 2);
				return type;
			}
			if (typeof(T) == typeof(float)) {
				type = Fill(type, Short(16));
				type = Fill(type, Short(32), 2);
				return type;
			}
			throw new NotSupportedException("No support for type "+typeof(T)+" is implemented, or it is not supported at all");
		}
		private byte[] Quaternion() {
			var quaternion = new byte[12];
			// quaternion = Fill(quaternion, Float(0));
			// quaternion = Fill(quaternion, Float(1), 4);
			// quaternion = Fill(quaternion, Float(0), 8);
			return quaternion;
		}
		private byte[] Affine() {
			var affine = new byte[48];
			// affine = Fill(affine, Float(Size.x));
			// affine = Fill(affine, Float(Size.z), 24);
			// affine = Fill(affine, Float(Size.y), 36);
			// // affine = Fill(affine, Float(Size.y), 20);
			// // affine = Fill(affine, Float(Size.z), 40);
			// // affine = Fill(affine, Float(Offset.x), 12);
			// // affine = Fill(affine, Float(Offset.y), 28);
			// // affine = Fill(affine, Float(Offset.z), 44);
			// affine = Fill(affine, Float(Offset.x), 12);
			// affine = Fill(affine, Float(Offset.z), 28);
			// affine = Fill(affine, Float(Offset.y), 44);
			var matrix = Transformation.Matrix();
			affine = Fill(affine, Float(matrix[0][0]));
			affine = Fill(affine, Float(matrix[0][1]), 4);
			affine = Fill(affine, Float(matrix[0][2]), 8);
			affine = Fill(affine, Float(matrix[0][3]), 12);
			affine = Fill(affine, Float(matrix[1][0]), 16);
			affine = Fill(affine, Float(matrix[1][1]), 20);
			affine = Fill(affine, Float(matrix[1][2]), 24);
			affine = Fill(affine, Float(matrix[1][3]), 28);
			affine = Fill(affine, Float(matrix[2][0]), 32);
			affine = Fill(affine, Float(matrix[2][1]), 36);
			affine = Fill(affine, Float(matrix[2][2]), 40);
			affine = Fill(affine, Float(matrix[2][3]), 44);
			return affine;
		}
		
		private static byte[] Padding(int multiples = 0) {
			return new byte[4 + 16 * multiples];
		}

		private byte[] Body() {
			if (typeof(T) == typeof(int)) {
				var body = new byte[Values.Length * 4];
				for (var i = 0; i < Values.Length; i++) {
					body = Fill(body, Int((int) Convert.ChangeType(Values[i], typeof(int))), i * 4); // This is ugly, constrain T and use overloads to resolve this somehow
				}
				return body;
			}
			if (typeof(T) == typeof(float)) {
				var body = new byte[Values.Length * 4];
				for (var i = 0; i < Values.Length; i++) {
					body = Fill(body, Float((float) Convert.ChangeType(Values[i], typeof(float))), i * 4); // This is ugly, constrain T and use overloads to resolve this somehow
				}
				return body;
			}
			throw new NotSupportedException("No support for type "+typeof(T)+" is implemented, or it is not supported at all");
		}

		private static byte[] Short(int value) {
			return new[] {(byte) value, (byte) (value >> 8)};
		}
		private static byte[] Int(int value) {
			return new[] {(byte) value, (byte) (value >> 8), (byte) (value >> 16), (byte) (value >> 24)};
		}
		private static byte[] Float(float value) {
			// TODO: Well, good to know this existed. Probably find some nice solution using this everywhere
			return BitConverter.GetBytes(value);
		}
		private static byte[] Fill(byte[] array, IReadOnlyList<byte> data, int offset=0) {
			for (var i = 0; i < data.Count; i++) {
				array[offset + i] = data[i];
			}
			return array;
		}
	}
}