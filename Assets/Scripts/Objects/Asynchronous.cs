using System.Collections.Concurrent;
using System.Threading;
using UnityEngine;

namespace Objects {
	public class Asynchronous : MonoBehaviour {
		private ConcurrentBag<int> bag;

		private void Start() {
			bag = new ConcurrentBag<int>();

			var producer = new ThreadedProducer(100, 0, bag);
			new Thread(producer.Run).Start();
			new Thread(producer.Run).Start();
			new Thread(producer.Run).Start();
			new Thread(producer.Run).Start();
			new Thread(producer.Run).Start();
		}
		private void Update() {
			for (var i = 0; i < 10 && !bag.IsEmpty; i++) {
				if (bag.TryTake(out var result)) {
					Debug.Log(result);
				}
			}
		}

		private class ThreadedProducer {
			private int amount;
			private int interval;
			private ConcurrentBag<int> bag;

			public ThreadedProducer(int amount, int interval, ConcurrentBag<int> bag) {
				this.amount = amount;
				this.interval = interval;
				this.bag = bag;
			}

			public void Run() {
				for (var i = 0; i < amount; i++) {
					bag.Add(i);
					Thread.Sleep(interval);
				}
			}
		}
	}
}