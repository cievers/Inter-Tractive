using System;

namespace Interface.Paradigm {
	public interface Value<T> : Controller {
		public Action<T> Action {get;}
	}
}