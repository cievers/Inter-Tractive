namespace Files.Types {
	public class Json : Publication.Publication {
		private readonly string serialized;

		public Json(string serialized) {
			this.serialized = serialized;;
		}
		
		public void Write(string path) {
			System.IO.File.WriteAllText(path, serialized);
		}
	}
}