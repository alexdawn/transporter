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

    public GroundMaterial(SquareCell caller)
    {
        if (decays)
        {
            CallCountDown(caller);
        }
    }


    public void SetToPrevious(SquareCell caller)
    {
        tileTypeName = previousMaterial.tileTypeName;
        color = previousMaterial.color;
        blendEdge = previousMaterial.blendEdge;
        decays = previousMaterial.decays;
        lifetime = previousMaterial.lifetime;
        previousMaterial = previousMaterial.previousMaterial;
        if(decays)
        {
            CallCountDown(caller);
        }
    }

    public static void SetToMud(SquareCell caller, ref GroundMaterial material) //temporary mud for construction/destruction
    {
        GroundMaterial newMaterial = Instantiate(material);
        newMaterial.tileTypeName = "Mud";
        newMaterial.color = new Color32(0x44, 0x20, 0x0A, 0xFF);
        newMaterial.blendEdge = true;
        newMaterial.decays = true;
        newMaterial.lifetime = 5f + Random.Range(1f, 1f);
        if (material.previousMaterial == null) // stops building long chains of mud decaying to mud
        {
            newMaterial.previousMaterial = material;
        }
        material = newMaterial;
        caller.Tile = material;
        caller.Refresh();
        caller.StartCoroutine(material.CallCountDown(caller));
    }

    IEnumerator CallCountDown(SquareCell caller)
    {
        //Debug.Log("callCountDown Called");
        yield return new WaitForSeconds(lifetime);
        SetToPrevious(caller);
        caller.Refresh();
        //Debug.Log("mud revereted to previous");
    }
}
