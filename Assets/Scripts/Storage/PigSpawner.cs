using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PigSpawner : MonoBehaviour
{


    public ScrollRect scrollRect;
    
    // Start is called before the first frame update
    void Start()
    {
        spawnUnipigs();
    }

    public GameObject[] whichUnipigSpawnPrefab;
    public GameObject[] whichUnipigSpawnPrefabClone;
    private GameObject unipigRespawnlayer;

    void spawnUnipigs()
    {
        unipigRespawnlayer = GameObject.FindWithTag("PigContent");
        Image im = unipigRespawnlayer.AddComponent(typeof(Image)) as Image;
        whichUnipigSpawnPrefab[0] = Instantiate(whichUnipigSpawnPrefab[0],unipigRespawnlayer.transform) as GameObject;
        whichUnipigSpawnPrefab[0] = Instantiate(whichUnipigSpawnPrefab[0],unipigRespawnlayer.transform) as GameObject;
    }
}
