using System;
using System.Linq;
using Evaluation;
using Evaluation.Coloring;
using Geometry;
using Geometry.Generators;
using Geometry.Topology;
using Objects.Collection;
using Objects.Concurrent;
using UnityEngine;

namespace Objects.Sources.Progressive {
	public class DiameterEvaluation : Promise<Triple<Tuple<Vector3, Vector3, Topology>>> {
		private Tuple<Vector3, Vector3, Walk> minimum;
		private Pair<Tuple<Vector3, Vector3, Walk>> ends;
		private readonly Func<Color32, WalkRenderer> rendering;
		private readonly Coloring coloring;

		private bool receivedMinimum;
		private bool receivedMaximum;

		public DiameterEvaluation(Promise<Tuple<Vector3, Vector3, Walk>> promisedMinimum, Promise<Pair<Tuple<Vector3, Vector3, Walk>>> promisedEndpoints, Func<Color32, WalkRenderer> rendering, Coloring coloring) {
			this.rendering = rendering;
			this.coloring = coloring;
			promisedMinimum.Request(minimum => {
				this.minimum = minimum;
				receivedMinimum = true;
				Update();
			});
			promisedEndpoints.Request(ends => {
				this.ends = ends;
				receivedMaximum = true;
				Update();
			});
		}
		
		private void Update() {
			if (receivedMinimum && receivedMaximum) {
				Start();
			}
		}
		
		protected override void Compute() {
			var collection = new[] {minimum, ends.Item1, ends.Item2};
			var colors = coloring.Color(collection.ToDictionary(walk => walk, walk => new Vector(walk.Item3.Skewer.Size.magnitude)));
			Complete(new Triple<Tuple<Vector3, Vector3, Topology>>(collection.Select(
				walk => new Tuple<Vector3, Vector3, Topology>(
					walk.Item1, 
					walk.Item2, 
					rendering.Invoke(colors[walk]).Render(walk.Item3)
				)
			).ToList()));
		}
	}
}