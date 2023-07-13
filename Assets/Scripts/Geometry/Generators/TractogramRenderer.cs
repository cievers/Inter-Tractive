using Geometry.Tracts;
using UnityEngine;

namespace Geometry.Generators {
	public interface TractogramRenderer {
		public Mesh Render(Tractogram tractogram);
	}
}