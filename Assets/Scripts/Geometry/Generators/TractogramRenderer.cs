using Geometry.Tracts;
using UnityEngine;

namespace Geometry.Generators {
	public interface TractogramRenderer : TractRenderer {
		public Mesh Render(Tractogram tractogram);
	}
}