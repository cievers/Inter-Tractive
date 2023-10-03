using System.Collections.Generic;
using UnityEngine;

namespace Geometry.Tracts {
	public interface Tract {
		public Vector3[] Points {get;}
		public IEnumerable<Vector3> Normals {get;}
		public IEnumerable<Segment> Segments {get;}
		public Boundaries Boundaries {get;}
		public float Resolution {get;}
		public float Slack {get;}

		public Tract Sample(int samples);
	}
}