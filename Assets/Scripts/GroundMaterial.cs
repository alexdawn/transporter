using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GroundMaterial
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

    public void SetToMud() //temporary mud for construction/destruction
    {
        previousMaterial = GetClone;
        tileTypeName = "Mud";
        color = new Color32(0x44, 0x20, 0x0A, 0xFF);
        blendEdge = true;
        decays = true;
        lifetime = 5f + Random.Range(1f, 5f);
    }

    public bool CountDown(float timer)
    {
        if (decays)
        {
            lifetime -= timer;
            if (lifetime < 0)
            {
                SetToPrevious();
                return true;
            }
        }
        return false;
    }
}
