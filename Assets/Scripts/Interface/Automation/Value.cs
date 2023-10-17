using System;

namespace Interface.Automation {
	public class Value<T> {
		private readonly Action<T> action;
		
		public Value(Paradigm.Value<T> paradigm) {
			action = paradigm.Action;
		}

		public void Automate(T value) {
			action.Invoke(value);
		}
	}
}