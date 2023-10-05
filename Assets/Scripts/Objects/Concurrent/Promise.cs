using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Objects.Concurrent {
	public abstract class Promise<T> {
		private bool completed;
		private T result;

		private readonly ConcurrentQueue<Action<T>> requests = new();

		protected void Start() {
			new Thread(Compute).Start();
		}
		protected abstract void Compute();
		protected void Complete(T value) {
			// While these three steps are not guaranteed to be atomic, I believe performing them in this order ensures
			// that any new request can either directly be served because the resulting value is set first, or will be
			// put in the event queue, which is about to complete
			result = value;
			completed = true;
			Invoke();
		}
		private void Invoke() {
			while (requests.TryDequeue(out var request)) {
				request.Invoke(result);
			}
		}
		public void Request(Action<T> action) {
			requests.Enqueue(action);
			if (completed) {
				Invoke();
			}
		}
	}
}