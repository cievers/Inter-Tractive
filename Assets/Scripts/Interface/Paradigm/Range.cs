namespace Interface.Paradigm {
	public interface Range<T> : Value<T> {
		public T Minimum {get;}
		public T Maximum {get;}
	}
}