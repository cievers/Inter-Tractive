using System;

namespace Interface.Paradigm {
	public interface Range<T> : Value<T> {
		public T Minimum {get;}
		public T Maximum {get;}
		public Func<T, T> Transformation => t => t;
	}
}