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

    public GroundMaterial GetClone {
        get { return (GroundMaterial)this.MemberwiseClone(); }
    }


    public void SetToGrass()
    {
        tileTypeName = "Grass";
        color = new Color32(0x30, 0x59, 0x12, 0xFF);
        blendEdge = true;
        decays = false;
        lifetime = 0;
    }

    public void SetToMud() //temporary mud for construction/destruction
    {
        tileTypeName = "Mud";
        color = new Color32(0x44, 0x20, 0x0A, 0xFF);
        blendEdge = true;
        decays = true;
        lifetime = 5f + Random.Range(1f, 5f);
    }

    public bool CountDown()
    {
        if (decays)
        {
            lifetime -= Time.deltaTime;
            if (lifetime < 0)
            {
                SetToGrass();
                return true;
            }
        }
        return false;
    }
}
