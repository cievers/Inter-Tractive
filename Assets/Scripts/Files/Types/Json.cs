namespace Files.Types {
	public class Json : Stored {
		private readonly string serialized;

		public Json(string serialized) {
			this.serialized = serialized;;
		}
		
		public void Write(string path) {
			System.IO.File.WriteAllText(path, serialized);
		}
	}
}