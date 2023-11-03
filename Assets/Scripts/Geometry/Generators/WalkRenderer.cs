using UnityEngine;

namespace Geometry.Generators {
	public interface WalkRenderer {
		public Mesh Render(Walk walk);
	}
}