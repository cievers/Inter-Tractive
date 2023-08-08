using System;
using System.Collections.Generic;
using UnityEngine;

namespace Evaluation.Coloring {
	public interface Coloring {
		public Tuple<int, int> Dimensions {get;}

		public IEnumerable<Color32> Color(IEnumerable<Vector> measurements);
		public Dictionary<T,Color32> Color<T>(Dictionary<T, Vector> measurements);
	}
}