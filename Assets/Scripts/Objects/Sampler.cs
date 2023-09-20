using Geometry.Generators;
using Geometry.Tracts;
using UnityEngine;

namespace Objects {
	public class Sampler : MonoBehaviour {
		public MeshFilter mesh;

		private void Start() {
			var tract = new ArrayTract(new Vector3[] {new(0, 0, 0), new(1, 0, 0), new(1, 1, 0), new(1, 1, 1)}, 0, Vector3.zero, 0);
			var sampled = tract.Sample(60);
			mesh.mesh = new WireframeRenderer().Render(sampled);
		}
	}
}