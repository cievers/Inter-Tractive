using Files.Publication;

namespace Geometry.Generators {
	public interface VolumeRenderer {
		public Topology.Topology Render(Nii<float> volume);
	}
}