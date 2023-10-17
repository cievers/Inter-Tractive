using System;

namespace Interface.Paradigm {
	public interface Transformation<T> : Value<T> {
		public Func<T, T> Transformation {get;}
	}
}