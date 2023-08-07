using UnityEngine;

namespace Evaluation.Coloring.Gradients {
	public class Temperature : Gradient {
		// https://www.kennethmoreland.com/color-advice/#cool-warm-smooth
		public Temperature() : base(new Color[] {
			new(59 / 255f, 76 / 255f, 192 / 255f),
			new(221 / 255f, 221 / 255f, 221 / 255f),
			new(180 / 255f, 4 / 255f, 38 / 255f),
		}) {}
	}
}