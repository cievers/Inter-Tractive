using System;

namespace Files {
	public class Exporter : Publisher {
		private readonly Func<Stored> file;

		public Exporter(string description, string type, Func<Stored> file) : base(description, type) {
			this.file = file;
		}

		public void Publish() {
			Publish(file.Invoke());
		}
	}
}