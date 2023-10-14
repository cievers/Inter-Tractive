using System;

namespace Files {
	public class Exporter : Publisher {
		private readonly Func<Publication.Publication> file;

		public Exporter(string description, string type, Func<Publication.Publication> file) : base(description, type) {
			this.file = file;
		}

		public void Publish() {
			Publish(file.Invoke());
		}
	}
}