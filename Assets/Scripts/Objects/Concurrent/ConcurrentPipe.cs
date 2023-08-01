using System.Collections.Concurrent;

namespace Objects.Concurrent {
	public class ConcurrentPipe<T> : ConcurrentBag<T> {
		public bool Completed {get; private set;}
		
		public void Restart() {
			Completed = false;
		}
		public void Complete() {
			Completed = true;
		}
	}
}