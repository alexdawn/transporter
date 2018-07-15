using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class GroundMaterial: ScriptableObject
{
    public string tileTypeName;
    public Color color;
    public bool blendEdge;
    public bool decays;
    public float lifetime;
    public GroundMaterial previousMaterial;

    public GroundMaterial GetClone {
        get { return (GroundMaterial)this.MemberwiseClone(); }
    }


    public void SetToPrevious()
    {
        tileTypeName = previousMaterial.tileTypeName;
        color = previousMaterial.color;
        blendEdge = previousMaterial.blendEdge;
        decays = previousMaterial.decays;
        lifetime = previousMaterial.lifetime;
        previousMaterial = previousMaterial.previousMaterial;
    }

    public static void SetToMud(ref GroundMaterial material) //temporary mud for construction/destruction
    {
        Debug.Log("set mud");
        GroundMaterial newMaterial = Instantiate(material);
        newMaterial.tileTypeName = "Mud";
        newMaterial.color = new Color32(0x44, 0x20, 0x0A, 0xFF);
        newMaterial.blendEdge = true;
        newMaterial.decays = true;
        newMaterial.lifetime = 5f + Random.Range(1f, 1f);
        if(material.previousMaterial == null) // stops building long chains of mud decaying to mud
        {
            newMaterial.previousMaterial = material;
        }
        material = newMaterial;
    }

    public bool CountDown(float timer)
    {
        if (decays)
        {
            lifetime -= timer;
            if (lifetime < 0)
            {
                Debug.Log("set to previous");
                SetToPrevious();
                return true;
            }
        }
        return false;
    }
}
