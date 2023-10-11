namespace Files {
	public class Publisher {
		private readonly string description;
		private readonly string type;

		public Publisher(string description, string type) {
			this.description = description;
			this.type = type;
		}
		
		public delegate void PublishEvent(Stored file, string description, string type);
		public event PublishEvent Published;
		public void Publish(Stored file) {
			Published?.Invoke(file, description, type);
		}
	}
}