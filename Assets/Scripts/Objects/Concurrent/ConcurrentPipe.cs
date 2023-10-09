using System.Collections.Concurrent;
using Logic.Eventful;

namespace Objects.Concurrent {
	public class ConcurrentPipe<T> : ConcurrentBag<T>, Boolean {
		public bool IsPublished {get; private set;}
		public bool IsCompleted => IsPublished && IsEmpty;
		public bool State => !IsCompleted;
		
		public new bool TryTake(out T result) {
			var success = base.TryTake(out result);
			if (success && IsCompleted) {
				Completed();
			}
			return success;
		}
		
		public void Restart() {
			// This should probably clear out any remaining queue
			IsPublished = false;
			True?.Invoke();
			Change?.Invoke(State);
		}
		public void Sent() {
			IsPublished = true;
		}
		private void Completed() {
			False?.Invoke();
			Change?.Invoke(State);
		}
		
		public event Boolean.ValueEvent True;
		public event Boolean.ValueEvent False;
		public event Boolean.ChangeEvent Change;
	}
}