using System;
using System.Collections.Generic;

namespace Objects.Collection {
	public class Pair<T> : Tuple<T, T> {
		public Pair(T item1, T item2) : base(item1, item2) {}
		public Pair(IList<T> items) : base(items[0], items[1]) {
			if (items.Count > 2) {
				throw new OverflowException("Provided too many items to a pair "+items.Count);
			}
		}
	}
}