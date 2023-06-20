using System;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public struct ClothingObject
{
    public string name;
    public List<string> materials;
}

[Serializable]
public struct ClothingModel
{
    public GameObject model;
    public List<ClothingObject> clothingObjects;
} 

public class BSHClothing : MonoBehaviour
{

    public List<ClothingModel> clothingModels = new List<ClothingModel>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
