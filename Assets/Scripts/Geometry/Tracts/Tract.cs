using System.Collections.Generic;
using UnityEngine;

namespace Geometry.Tracts {
	public interface Tract {
		public IEnumerable<Segment> Segments {get;}
		public Vector3[] Points {get;}
		public Boundaries Boundaries {get;}
		public float Resolution {get;}
		public float Slack {get;}
	}
}