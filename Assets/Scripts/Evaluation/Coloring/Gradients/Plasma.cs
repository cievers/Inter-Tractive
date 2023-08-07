using UnityEngine;

namespace Evaluation.Coloring.Gradients {
	public class Plasma : Gradient {
		// https://www.kennethmoreland.com/color-advice/#plasma
		public Plasma() : base(new Color[] {
			new(13 / 255f, 8 / 255f, 135 / 255f),
			new(84 / 255f, 2 / 255f, 163 / 255f),
			new(139 / 255f, 10 / 255f, 165 / 255f),
			new(185 / 255f, 50 / 255f, 137 / 255f),
			new(219 / 255f, 92 / 255f, 104 / 255f),
			new(244 / 255f, 136 / 255f, 73 / 255f),
			new(254 / 255f, 188 / 255f, 43 / 255f),
			new(240 / 255f, 249 / 255f, 33 / 255f)
		}) {}
	}
}