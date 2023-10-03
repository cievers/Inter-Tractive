using System;
using System.Threading;

namespace Objects.Concurrent {
	public abstract class Promise<T> {
		private bool completed;
		private T result;
		
		private delegate void PromiseCompleted(T promise);
		private event PromiseCompleted Completion;

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
			Completion?.Invoke(value);
		}
		public void Request(Action<T> action) {
			if (completed) {
				action.Invoke(result);
			} else {
				Completion += action.Invoke;
			}
		}
	}
}