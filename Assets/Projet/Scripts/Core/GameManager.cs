using CharacterController.Script;
using DefaultNamespace;
using HideSysteme;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
	[field : SerializeField] public bool IsHiden { get; private set; }
	[field : SerializeField] public bool IsSee { get; private set; }
	[field : SerializeField] public Transform Where { get; private set; }
	protected override void Awake()
	{
		base.Awake();
	}

	public void Hiden(CharacterSetup player, HideZone zone)
	{
		IsHiden = true;
		Where =  zone.transform;
	}

	public void UnHiden(CharacterSetup player)
	{
		IsHiden = false;
		Where = null;
	}
	
	public void See(CharacterSetup player)
	{
		IsSee = true;
	}

	public void UnSee(CharacterSetup player)
	{
		IsSee = false;
	}
}