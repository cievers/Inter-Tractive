using System.Collections.Generic;
using Files.Publication;
using UnityEngine;

namespace Geometry.Tracts {
	public interface Tractogram : Tck {
		IEnumerable<Vector3> Points {get;}
		Boundaries Boundaries {get;}
		float Resolution {get;}
		float Slack {get;}

		public UniformTractogram Sample(int samples);
	}
}