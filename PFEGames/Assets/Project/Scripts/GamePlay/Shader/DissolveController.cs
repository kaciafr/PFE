using UnityEngine;

namespace Goblfin.Gameplay.Gameplay.Shader
{
	public class DissolveController : MonoBehaviour
	{
		[SerializeField] private Renderer targetRenderer; // glisse ici le mesh du gobelin
		[SerializeField] private float speed = 1f;
		[SerializeField] private float maxDissolve = 2.5f;
		[SerializeField] private Color dangerColor;
		[SerializeField] private Color baseColor;
		[SerializeField] private float rgbSpeed = 0.3f;
		private Material mat;
		private float current;
		private bool isChanging;
		

		void Awake()
		{
			mat = targetRenderer.sharedMaterial;
			mat.SetColor("_CurrentColor", baseColor);
			mat.SetFloat("_Dissolve", 0f);
		}

		void Update()
		{
		}
		
		public void InDanger()
		{
			mat.SetColor("_NewColor", dangerColor);
			ChangeColor();
		}

		public void InNormal()
		{
			mat.SetColor("_CurrentColor", baseColor);
			mat.SetFloat("_Dissolve", 0f);
		}

		public void RGB()
		{
			Color c = Color.HSVToRGB(Mathf.Repeat(Time.time * rgbSpeed, 1f), 1f, 1f);
				mat.SetColor("_NewColor", c);
			ChangeColor();
		}

		public void Die()
		{
			mat.SetColor("_NewColor", dangerColor);
			ChangeColor();
		}
		
		private void ChangeColor()
		{
			isChanging=!isChanging;
			
			if (!isChanging)
			{
				
				current = Mathf.Lerp(current, maxDissolve, speed * Time.deltaTime);
				this.mat.SetFloat("_Dissolve", current);
			}
			
		}
	}
}