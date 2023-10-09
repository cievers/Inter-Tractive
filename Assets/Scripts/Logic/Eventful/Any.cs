using System.Collections.Generic;
using System.Linq;

namespace Logic.Eventful {
	public class Any : Boolean {
		private readonly IEnumerable<Boolean> inputs;
		public bool State {get; private set;}

		public Any(IEnumerable<Boolean> inputs) {
			var array = inputs as Boolean[] ?? inputs.ToArray();
			this.inputs = array;
			foreach (var state in array) {
				state.Change += _ => Update();
			}
		}

		private void Update() {
			var current = inputs.Any(input => input.State);
			if (State != current) {
				Update(current);
			}
		}
		private void Update(bool state) {
			State = state;
			Change?.Invoke(State);
			if (State) {
				True?.Invoke();
			} else {
				False?.Invoke();
			}
		}
		
		public event Boolean.ValueEvent True;
		public event Boolean.ValueEvent False;
		public event Boolean.ChangeEvent Change;
	}
}