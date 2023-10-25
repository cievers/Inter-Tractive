using Geometry.Tracts;
using UnityEngine;

namespace Geometry.Generators {
	public interface TractRenderer {
		public Mesh Render(Tract tract);
	}
}