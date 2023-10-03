using System.Collections.Concurrent;

namespace Objects.Concurrent {
	public class PromiseCollector<T> : ConcurrentBag<T> {
		public void Add(Promise<T> promise) {
			promise.Request(Add);
		}
	}
}