using System;
using System.Timers;

namespace Objects {
	public class ValueChangeBuffer<T> {
		private T buffer;
		private readonly float delay;
		private readonly Action<T> update;
		private Timer timer = new();

		public ValueChangeBuffer(float delay, Action<T> update) {
			this.delay = delay;
			this.update = update;
		}

		private void Propagate(object source, ElapsedEventArgs e) {
			update.Invoke(buffer);
		}
		public void Request(T value) {
			// Halt any previous timer if it was still running
			timer.Enabled = false;
			
			// Update the value to send
			buffer = value;
			
			// And create a new timer to eventually propagate it
			timer = new Timer(delay * 1000); // Change from seconds to the milliseconds Timer wants
			timer.Elapsed += Propagate;
			timer.AutoReset = false;
			timer.Enabled = true;
		}
	}
}