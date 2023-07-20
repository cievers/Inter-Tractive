using System;

namespace Interface.Data {
	public record Toggle(string Name, bool State, Action<bool> Action) : Configuration {
		public string Name {get; private set;} = Name;
		public bool State {get; private set;} = State;
		public Action<bool> Action {get; private set;} = Action;
	}
}