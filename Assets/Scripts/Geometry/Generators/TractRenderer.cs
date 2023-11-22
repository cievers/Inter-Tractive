using Geometry.Tracts;

namespace Geometry.Generators {
	public interface TractRenderer {
		public Topology.Topology Render(Tract tract);
	}
}