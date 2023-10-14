using System.IO;
using System.Text;

namespace Files.Publication {
	public interface Publication {
		public void Write(string path);
		public static void Write(string path, byte[] data) {
			using var stream = File.Open(path, FileMode.Create);
			using var writer = new BinaryWriter(stream, Encoding.UTF8, false);
			writer.Write(data);
		}
	}
}