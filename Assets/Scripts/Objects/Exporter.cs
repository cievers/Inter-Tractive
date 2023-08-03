using Files.Types;
using Geometry;
using UnityEngine;

namespace Objects {
	public class Exporter : MonoBehaviour {
		private void Start() {
			var nii = new Nii<float>(new[] {0, 0.125f, 0.25f, 0.375f, 0.5f, 0.625f, 0.75f, 0.875f}, new Index3(2, 2, 2), new Vector3(0, 0, 0), new Vector3(2, 4, 8));
			nii.Write("test.nii");
		}
	}
}