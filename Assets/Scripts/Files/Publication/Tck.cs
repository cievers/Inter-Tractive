using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Geometry.Tracts;
using UnityEngine;

namespace Files.Publication {
	public interface Tck : Publication {
		public IEnumerable<Tract> Tracts {get;}

		void Publication.Write(string path) {
			Write(path, Header().Concat(Body()).ToArray());
		}
		private IEnumerable<byte> Header() {
			// If needed, add more entries and/or do something more complex than a plain string
			const string template = "mrtrix tracks\ndatatype: Float32LE\nfile: . 49\nEND\n";
			return Encoding.UTF8.GetBytes(template);
		}
		private IEnumerable<byte> Body() {
			var result = new List<byte>();
			var array = Tracts.ToArray();
			for (var i = 0; i < array.Length; i++) {
				result.AddRange(Track(array[i]));
				result.AddRange(i < array.Length - 1 ? Triplet(float.NaN) : Triplet(float.PositiveInfinity));
			}
			return result;
		}
		private static IEnumerable<byte> Track(Tract tract) {
			return tract.Points.SelectMany(Vector);
		}
		private static IEnumerable<byte> Vector(Vector3 vector) {
			return Triplet(vector.x, vector.z, vector.y);
		}
		private static IEnumerable<byte> Triplet(float a, float b, float c) {
			var result = new byte[12];
			result = Fill(result, BitConverter.GetBytes(a));
			result = Fill(result, BitConverter.GetBytes(b), 4);
			result = Fill(result, BitConverter.GetBytes(c), 8);
			return result;
		}
		private static IEnumerable<byte> Triplet(float a) {
			return Triplet(a, a, a);
		}
		private static byte[] Fill(byte[] array, IReadOnlyList<byte> data, int offset=0) {
			for (var i = 0; i < data.Count; i++) {
				array[offset + i] = data[i];
			}
			return array;
		}
	}
}