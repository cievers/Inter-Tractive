using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Geometry.Tracts {
	public class UniformTractogram : Tractogram {
		public IEnumerable<Tract> Tracts {get;}
		public IEnumerable<Vector3> Points => Tracts.SelectMany(tract => tract.Points);
		public Boundaries Boundaries => Boundaries.Join(Tracts.Select(segment => segment.Boundaries));
		public float Resolution => Tracts.Select(segment => segment.Resolution).Min();
		public float Slack => Tracts.Select(segment => segment.Slack).Max();
		public int Samples => Tracts.Min(tract => tract.Points.Length);

		public UniformTractogram(IEnumerable<Tract> lines) {
			Tracts = lines;
		}
		public UniformTractogram Sample(int samples) {
			return new UniformTractogram(Tracts.Select(tract => tract.Sample(samples)));
		}
		public IEnumerable<Vector3> Slice(int index) {
			return Tracts.Select(tract => tract.Points[index]);
		}
		public IEnumerable<IEnumerable<Vector3>> Slices() {
			var result = new IEnumerable<Vector3>[Samples];
			for (var i = 0; i < Samples; i++) {
				result[i] = Slice(i);
			}
			return result;
		}
	}
}