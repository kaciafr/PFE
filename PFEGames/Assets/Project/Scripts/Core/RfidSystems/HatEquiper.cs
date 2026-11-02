using System;
using System.Collections.Generic;
using UnityEngine;

public class HatEquipper : MonoBehaviour
{
    [Serializable]
    public class TagHat
    {
        public string uid;        
        public GameObject hatPrefab;
    }

    public RfidReader reader;

    public Transform headSlot;
   

    public bool toggleOnSameTag = true;

    public List<TagHat> hats = new List<TagHat>();

    Dictionary<string, GameObject> map = new Dictionary<string, GameObject>();
    GameObject currentHat;
    string currentUid;

    void Awake()
    {
        foreach (var h in hats)
            map[h.uid.Trim().ToUpper()] = h.hatPrefab;


        if (headSlot == null)
            Debug.LogError("HatEquipper : aucun headSlot ni Animator Humanoid trouvé.");
    }

    void OnEnable()
    {
        if (reader != null) reader.OnTagRead += HandleTag;
        else Debug.LogError("HatEquipper : 'reader' n'est pas assigné.");
    }

    void OnDisable()
    {
        if (reader != null) reader.OnTagRead -= HandleTag;
    }

    void HandleTag(string uid)
    {
        string key = uid.Trim().ToUpper();

        if (!map.TryGetValue(key, out var prefab) || prefab == null)
        {
            Debug.Log("Tag inconnu ou sans chapeau : [" + key + "]");
            return;
        }

        if (toggleOnSameTag && currentHat != null && key == currentUid)
        {
            RemoveHat();
            return;
        }

        RemoveHat();

        currentHat = Instantiate(prefab, headSlot.position, headSlot.rotation);
        currentHat.transform.SetParent(headSlot, true);
        currentUid = key;
        Debug.Log("Chapeau équipé : " + prefab.name);
    }

    void RemoveHat()
    {
        if (currentHat != null) Destroy(currentHat);
        currentHat = null;
        currentUid = null;
    }
}