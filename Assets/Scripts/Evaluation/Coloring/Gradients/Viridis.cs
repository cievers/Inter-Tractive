using UnityEngine;

namespace Evaluation.Coloring.Gradients {
	public class Viridis : Gradient {
		// https://www.kennethmoreland.com/color-advice/#viridis
		public Viridis() : base(new Color[] {
			new(68 / 255f, 1 / 255f, 84 / 255f),
			new(70 / 255f, 50 / 255f, 127 / 255f),
			new(54 / 255f, 92 / 255f, 141 / 255f),
			new(39 / 255f, 127 / 255f, 142 / 255f),
			new(31 / 255f, 161 / 255f, 135 / 255f),
			new(74 / 255f, 194 / 255f, 109 / 255f),
			new(159 / 255f, 218 / 255f, 58 / 255f),
			new(253 / 255f, 231 / 255f, 37 / 255f)
		}) {}
	}
}