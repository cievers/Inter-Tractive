using Geometry.Tracts;
using Objects.Concurrent;

namespace Objects.Sources.Progressive {
	public class Resample : Promise<UniformTractogram> {
		private readonly Tractogram tractogram;
		private readonly int samples;

		public Resample(Tractogram tractogram, int samples) {
			this.tractogram = tractogram;
			this.samples = samples;

			Start();
		}

		protected override void Compute() {
			Complete(tractogram.Sample(samples));
		}
	}
}