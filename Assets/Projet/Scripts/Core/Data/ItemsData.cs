using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "Items/ItemData")]
public class ItemsData : ScriptableObject
{
	[Header("Informations")]
	[field:SerializeField ] public string ItemName { get; private set; }
	[field:SerializeField ] public bool IsBreakable { get; private set; }
	[field:SerializeField ] public float Mass { get; private set; }     
	
	
	
	[Header("Settings")]
	[field:SerializeField ] public float StoppingDistance { get; private set; }
	[field:SerializeField ] public float MaxImpactForce { get; private set; }
	[field:SerializeField ] public GameObject ItemPrefab { get; private set; }
	[field:SerializeField ] public AudioClip FallSound { get; private set; }
	
}