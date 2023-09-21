using System.Collections.Generic;
using UnityEngine;

namespace Geometry.Tracts {
	public interface Tractogram {
		IEnumerable<Tract> Tracts {get;}
		IEnumerable<Vector3> Points {get;}
		Boundaries Boundaries {get;}
		float Resolution {get;}
		float Slack {get;}

		public UniformTractogram Sample(int samples);
	}
}