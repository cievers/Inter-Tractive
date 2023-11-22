using System;
using System.Collections.Generic;

namespace Objects.Collection {
	public class Triple<T> : Tuple<T, T, T> {
		public Triple(T item1, T item2, T item3) : base(item1, item2, item3) {}
		public Triple(IList<T> items) : base(items[0], items[1], items[2]) {
			if (items.Count > 3) {
				throw new OverflowException("Provided too many items to a pair "+items.Count);
			}
		}
	}
}