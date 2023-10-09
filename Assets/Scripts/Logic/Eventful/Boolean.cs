namespace Logic.Eventful {
	public interface Boolean {
		public delegate void ValueEvent();
		public event ValueEvent True;
		public event ValueEvent False;
		
		public delegate void ChangeEvent(bool state);
		public event ChangeEvent Change;

		public bool State {get;}
	}
}