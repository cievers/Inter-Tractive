namespace Files {
	public class Publisher {
		private readonly string description;
		private readonly string type;

		public Publisher(string description, string type) {
			this.description = description;
			this.type = type;
		}
		
		public delegate void PublishEvent(Publication.Publication file, string description, string type);
		public event PublishEvent Published;
		public void Publish(Publication.Publication file) {
			Published?.Invoke(file, description, type);
		}
	}
}