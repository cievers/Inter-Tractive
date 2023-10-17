using System.Collections.Generic;

namespace Interface.Paradigm {
	public interface Container {
		public string Prefix {get;}
		public IEnumerable<Component> Components {get;}
	}
}