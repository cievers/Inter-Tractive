using System.Collections.Concurrent;
using Logic.Eventful;

namespace Objects.Concurrent {
	public class PromiseCollector<T> : ConcurrentBag<T>, Boolean {
		public bool State {get; private set;}
		public void Add(Promise<T> promise) {
			State = true;
			True?.Invoke();
			Change?.Invoke(State);
			promise.Request(Complete);
		}
		private void Complete(T result) {
			Add(result);
			State = false;
			False?.Invoke();
			Change?.Invoke(State);
		}
		
		public event Boolean.ValueEvent True;
		public event Boolean.ValueEvent False;
		public event Boolean.ChangeEvent Change;
	}
}