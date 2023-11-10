using Geometry.Tracts;

namespace Geometry.Generators {
	public interface TractogramRenderer : TractRenderer {
		public Topology.Topology Render(Tractogram tractogram);
	}
}