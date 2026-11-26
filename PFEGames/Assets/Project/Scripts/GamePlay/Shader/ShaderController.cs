using UnityEngine;

namespace Goblfin.Gameplay.Gameplay.Shader
{
	public class ShaderController : MonoBehaviour
	{
		[Header("References")]
		[SerializeField] private Renderer targetRenderer;
		[SerializeField] private ShaderManager shaderManager;
		
		[Header("Change Color Settings")]
		[SerializeField] private float speed = 1f;
		[SerializeField] private float maxDissolve = 2.5f;
		[SerializeField] private float rgbSpeed = 0.3f;
		
		[Header("ColorSettings")]
		[SerializeField] private Color dangerColor;
		[SerializeField] private Color dieColor;
		[SerializeField] private Color baseColor;
		[SerializeField] private Color littleDanger;
		[SerializeField] private Color hideColor;

		private static readonly int CurrentColorId = UnityEngine.Shader.PropertyToID("_CurrentColor");
		private static readonly int NewColorId     = UnityEngine.Shader.PropertyToID("_NewColor");
		private static readonly int DissolveId     = UnityEngine.Shader.PropertyToID("_Dissolve");
		
		[SerializeField]private Color currentColor;
		[SerializeField]private Color pendingColor;
		private float dissolve;
		private Material mat;
		private bool isChanging;
		private bool hasPending;
		
		private void Awake()
		{
			Material source = targetRenderer.sharedMaterial;
			mat = new Material(source);

			foreach (Renderer r in GetComponentsInChildren<Renderer>(true))
			{
				Material[] mats = r.sharedMaterials;
				bool changed = false;
				for (int i = 0; i < mats.Length; i++)
				{
					if (mats[i] == source) { mats[i] = mat; changed = true; }
				}
				if (changed) r.sharedMaterials = mats;
			}
			currentColor = baseColor;
			isChanging = false;
			hasPending = false;
			dissolve = maxDissolve;
			mat.SetColor(CurrentColorId, baseColor);
			mat.SetColor(NewColorId, baseColor);
			SetDissolve(dissolve);
		}

		private void OnEnable()
		{
			shaderManager.NormalStateChanged += InNormal;
			shaderManager.SearchStateChanged += InLittleDanger;
			shaderManager.DangerStateChanged+= InDanger;
			shaderManager.DieStateChanged += InDie;
			shaderManager.HideStateChanged+= Hide;
		}

		private void OnDisable()
		{
			shaderManager.NormalStateChanged -= InNormal;
			shaderManager.SearchStateChanged -= InLittleDanger;
			shaderManager.DangerStateChanged -= InDanger;
			shaderManager.DieStateChanged -= InDie;
			shaderManager.HideStateChanged -= Hide;
		}
		
		private void Update()
		{
			if (isChanging)
			{
				dissolve = Mathf.MoveTowards(dissolve, maxDissolve, speed * Time.deltaTime);
				SetDissolve(dissolve);
				if (dissolve >= maxDissolve)
					FinishTransition();
			}
		}
		public void Hide() => ChangeColor(hideColor);
		public void InNormal()=> ChangeColor(baseColor);
		public void InLittleDanger()=> ChangeColor(littleDanger);
		public void InDanger()=> ChangeColor(dangerColor);
		public void InDie()=> ChangeColor(dieColor);
		
		public void RGB()
		{
			Color c = Color.HSVToRGB(Mathf.Repeat(Time.time * rgbSpeed, 1f), 1f, 1f);
				mat.SetColor(NewColorId, c);
		}

		private void ChangeColor(Color color)
		{
			if (isChanging)
			{
				pendingColor =  color;
				hasPending = true;
				return;
			}
			if(color != currentColor)
				StartTransition(color);
		}

		private void StartTransition(Color color)
		{
			mat.SetColor(CurrentColorId, currentColor);
			mat.SetColor(NewColorId, color);
			currentColor = color;
			dissolve = 0;
			SetDissolve(dissolve);
			isChanging = true;
		}
		private void FinishTransition()
		{
			isChanging = false;
			mat.SetColor(CurrentColorId, currentColor);
			SetDissolve(maxDissolve);
			if (hasPending)
			{
				hasPending = false;
				if(pendingColor != currentColor)
					StartTransition(pendingColor);
			}
		}
		private void OnDestroy() { if (mat != null) Destroy(mat); }
		private void SetDissolve(float value) => mat.SetFloat(DissolveId, value);
	}
}