using Camera;
using Files.Types;
using Geometry.Generators;
using UnityEngine;

namespace Objects.Sources {
	public class Volume : Voxels {
		public MeshFilter mesh;
		
		private Nii<float> volume;

		protected override void New(string path) {
			volume = Nii<float>.Load(path);
			mesh.mesh = new GridRenderer(0).Render(volume).Mesh();
			Focus(new Focus(Vector3.zero, 0));
		}
		public override Map Map() {
			throw new System.NotImplementedException();
		}
	}
}