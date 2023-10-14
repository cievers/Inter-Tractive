using Geometry;
using UnityEngine;

namespace Files.Types {
	public record Nii<T>(T[] Values, Index3 Composition, Vector3 Offset, Vector3 Size, int Measurements=1) : Publication.Nii<T> {
		public T[] Values {get;} = Values;
		public int Measurements {get;} = Measurements;
		public Index3 Composition {get;} = Composition;
		public Vector3 Offset {get;} = Offset;
		public Vector3 Size {get;} = Size;
	}
}