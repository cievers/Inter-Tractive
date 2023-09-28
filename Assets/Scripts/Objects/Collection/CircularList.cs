using System.Collections.Generic;

namespace Objects.Collection {
	public class CircularList<T> : List<T> {
		public T Last {
			get => this[Count - 1];
			set => this[Count - 1] = value;
		}

		public T First {
			get => this[0];
			set => this[0] = value;
		}

		public void PushLast(T item) {
			Add(item);
		}

		public T PopLast() {
			T result = this[Count - 1];
			RemoveAt(Count - 1);
			return result;
		}

		public void PushFirst(T item) {
			Insert(0, item);
		}

		public T PopFirst() {
			T result = this[0];
			RemoveAt(0);
			return result;
		}
	}
}