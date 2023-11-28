using System;
using System.Linq;
using Geometry;
using Geometry.Generators;
using Geometry.Topology;
using Geometry.Tracts;
using Objects.Collection;
using UnityEngine;

namespace Objects {
	public class Logo : MonoBehaviour {
		public MeshFilter meshA;
		public MeshFilter meshB;
		public MeshFilter planeA;
		public MeshFilter planeB;

		public Vector3 normalA = Vector3.back;
		public Vector3 normalB = Vector3.back;

		private static readonly Vector2[] POINTS_A = {
			new(98.728f, 184.566f),
			new(103.111f, 189.005f),
			new(107.829f, 192.904f),
			new(112.839f, 196.262f),
			new(118.096f, 199.074f),
			new(123.558f, 201.34f),
			new(129.182f, 203.056f),
			new(134.924f, 204.221f),
			new(140.74f, 204.831f),
			new(146.589f, 204.884f),
			new(152.425f, 204.377f),
			new(158.207f, 203.309f),
			new(163.891f, 201.677f),
			new(169.433f, 199.478f),
			new(174.79f, 196.71f),
			new(179.92f, 193.37f),
			new(184.778f, 189.456f),
			new(187.111f, 187.263f),
			new(189.305f, 184.977f),
			new(191.357f, 182.605f),
			new(193.268f, 180.151f),
			new(195.038f, 177.623f),
			new(196.666f, 175.025f),
			new(198.153f, 172.363f),
			new(199.496f, 169.643f),
			new(200.698f, 166.871f),
			new(201.756f, 164.053f),
			new(202.671f, 161.193f),
			new(203.443f, 158.299f),
			new(204.071f, 155.375f),
			new(204.554f, 152.428f),
			new(204.893f, 149.463f),
			new(205.088f, 146.486f),
			new(205.306f, 144.603f),
			new(205.782f, 142.857f),
			new(206.491f, 141.258f),
			new(207.407f, 139.816f),
			new(208.506f, 138.541f),
			new(209.763f, 137.443f),
			new(211.152f, 136.531f),
			new(212.648f, 135.817f),
			new(214.226f, 135.309f),
			new(215.861f, 135.019f),
			new(217.528f, 134.955f),
			new(219.201f, 135.127f),
			new(220.856f, 135.547f),
			new(222.467f, 136.223f),
			new(224.009f, 137.166f),
			new(225.458f, 138.386f),
			new(226.775f, 139.673f),
			new(228.091f, 140.961f),
			new(229.408f, 142.248f),
			new(230.725f, 143.536f),
			new(232.042f, 144.823f),
			new(233.359f, 146.111f),
			new(234.676f, 147.398f),
			new(235.993f, 148.686f),
			new(237.31f, 149.973f),
			new(238.626f, 151.261f),
			new(239.943f, 152.548f),
			new(241.26f, 153.836f),
			new(242.577f, 155.123f),
			new(243.894f, 156.411f),
			new(245.211f, 157.698f),
			new(246.528f, 158.986f),
		};
		private static readonly Vector2[] POINTS_B = {
			new(189.668f, 103.398f),
			new(185.284f, 98.959f),
			new(180.566f, 95.059f),
			new(175.557f, 91.702f),
			new(170.299f, 88.889f),
			new(164.837f, 86.623f),
			new(159.214f, 84.907f),
			new(153.472f, 83.743f),
			new(147.655f, 83.133f),
			new(141.807f, 83.08f),
			new(135.97f, 83.586f),
			new(130.189f, 84.654f),
			new(124.505f, 86.286f),
			new(118.963f, 88.486f),
			new(113.605f, 91.254f),
			new(108.476f, 94.594f),
			new(103.618f, 98.508f),
			new(101.284f, 100.701f),
			new(99.091f, 102.986f),
			new(97.039f, 105.359f),
			new(95.127f, 107.812f),
			new(93.357f, 110.341f),
			new(91.729f, 112.939f),
			new(90.243f, 115.6f),
			new(88.899f, 118.32f),
			new(87.698f, 121.092f),
			new(86.639f, 123.911f),
			new(85.724f, 126.77f),
			new(84.953f, 129.665f),
			new(84.325f, 132.588f),
			new(83.841f, 135.536f),
			new(83.502f, 138.501f),
			new(83.308f, 141.478f),
			new(83.09f, 143.361f),
			new(82.614f, 145.107f),
			new(81.905f, 146.706f),
			new(80.988f, 148.148f),
			new(79.889f, 149.423f),
			new(78.633f, 150.521f),
			new(77.244f, 151.432f),
			new(75.748f, 152.146f),
			new(74.17f, 152.654f),
			new(72.535f, 152.945f),
			new(70.868f, 153.009f),
			new(69.195f, 152.836f),
			new(67.54f, 152.416f),
			new(65.929f, 151.74f),
			new(64.386f, 150.797f),
			new(62.938f, 149.578f),
			new(61.621f, 148.29f),
			new(60.304f, 147.003f),
			new(58.987f, 145.715f),
			new(57.67f, 144.428f),
			new(56.353f, 143.14f),
			new(55.036f, 141.853f),
			new(53.72f, 140.565f),
			new(52.403f, 139.278f),
			new(51.086f, 137.99f),
			new(49.769f, 136.703f),
			new(48.452f, 135.415f),
			new(47.135f, 134.128f),
			new(45.818f, 132.84f),
			new(44.501f, 131.553f),
			new(43.185f, 130.265f),
			new(41.868f, 128.978f),
		};
		private const float POINTS_OFFSET = 144.007875f;
		private readonly TubeRenderer tube = new(16, 10, (_, normal, _) => new Color(Math.Abs(normal.x), Math.Abs(normal.z), Math.Abs(normal.y)));
		
		private void Update() {
			var models = Manual();
			meshA.mesh = models.Item1.Mesh();
			meshB.mesh = models.Item2.Mesh();
			// planeA.mesh = Geometry.Plane.Mesh(Vector3.zero, normalA, POINTS_OFFSET * 2);
			// planeB.mesh = Geometry.Plane.Mesh(Vector3.zero, normalB, POINTS_OFFSET * 2);
		}
		private Pair<Topology> Manual() {
			return new Pair<Topology>(
				tube.Render(new ArrayTract(POINTS_A
					.Select(v => Geometry.Plane.Intersection(new Line(new Vector3(-(v.x - POINTS_OFFSET), v.y - POINTS_OFFSET, 0), Vector3.forward), Vector3.zero, normalA))
					.Where(intersection => intersection != null)
					.Select(intersection => (Vector3) intersection)
					.ToArray())
				),
				tube.Render(new ArrayTract(POINTS_B
					.Select(v => Geometry.Plane.Intersection(new Line(new Vector3(-(v.x - POINTS_OFFSET), v.y - POINTS_OFFSET, 0), Vector3.forward), Vector3.zero, normalB))
					.Where(intersection => intersection != null)
					.Select(intersection => (Vector3) intersection)
					.ToArray())
				)
			);
		}
	}
}