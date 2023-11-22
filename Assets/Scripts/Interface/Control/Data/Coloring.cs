using System;
using System.Collections.Generic;

namespace Interface.Control.Data {
	public record Coloring : Dropdown.Data<global::Evaluation.Coloring.Coloring> {
		public Coloring(Dictionary<string, global::Evaluation.Coloring.Coloring> options, Action<global::Evaluation.Coloring.Coloring> update) : base(options, update) {}
	}
}