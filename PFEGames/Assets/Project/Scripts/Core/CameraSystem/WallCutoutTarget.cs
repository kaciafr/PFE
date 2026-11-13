using UnityEngine;

namespace Goblfin
{
	public class WallCutoutTarget : MonoBehaviour
	{
		static readonly int PlayerId = Shader.PropertyToID("_CutoutPlayer");
		static readonly int RadiusId = Shader.PropertyToID("_CutoutRadius");
		static readonly int SoftnessId = Shader.PropertyToID("_CutoutSoftness");
		static readonly int MinAlphaId = Shader.PropertyToID("_CutoutMinAlpha");

		[SerializeField] float radius = 0.3f;
		[SerializeField] float softness = 0.1f;
		[SerializeField, Range(0f, 1f)] float minAlpha = 0.25f;
		[SerializeField] Vector3 offset = new(0f, 0.5f, 0f);

		void LateUpdate()
		{
			Vector3 p = transform.position + offset;
			Shader.SetGlobalVector(PlayerId, new Vector4(p.x, p.y, p.z, 1f));
			Shader.SetGlobalFloat(RadiusId, radius);
			Shader.SetGlobalFloat(SoftnessId, softness);
			Shader.SetGlobalFloat(MinAlphaId, minAlpha);
		}

		void OnDisable() => Shader.SetGlobalVector(PlayerId, Vector4.zero);
	}
}
