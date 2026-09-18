using System;
using CharacterController.Script;
using UnityEngine;

namespace HideSysteme
{
	public class HideZone : MonoBehaviour
	{
		[SerializeField] private BoxCollider boxCollider;
		private CharacterSetup player;

		private void OnTriggerStay(Collider other)
		{
			if (other.tag == "Player")
			{
				player = other.GetComponent<CharacterSetup>();
			}
			GameManager.Instance.Hiden(player,this);
		}

		private void OnTriggerExit(Collider other)
		{
			GameManager.Instance.UnHiden(player);
			player = null;
		}
	}
}
