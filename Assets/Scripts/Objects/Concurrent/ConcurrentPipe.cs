using System.Collections.Concurrent;

namespace Objects.Concurrent {
	public class ConcurrentPipe<T> : ConcurrentBag<T> {
		public bool IsCompleted {get; private set;}
		
		public void Restart() {
			IsCompleted = false;
		}
		public void Complete() {
			IsCompleted = true;
		}
	}
}