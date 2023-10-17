namespace Interface.Automation {
	public class Action {
		private readonly System.Action action;
		
		public Action(Paradigm.Action paradigm) {
			action = paradigm.Action;
		}

		public void Automate() {
			action.Invoke();
		}
	}
}