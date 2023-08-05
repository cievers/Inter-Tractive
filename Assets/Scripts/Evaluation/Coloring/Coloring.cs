using System;
using System.Collections.Generic;
using UnityEngine;

namespace Evaluation.Coloring {
	public interface Coloring {
		public abstract Tuple<int, int> Dimensions {get;}

		public abstract IEnumerable<Color32> Color<T>(IEnumerable<Vector> measurements);
		public abstract Dictionary<T,Color32> Color<T>(Dictionary<T, Vector> measurements);
	}
}