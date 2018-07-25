using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Industry : Building
{
    public Cargo[] cargoProduced;
    public Cargo[] cargoCosumed;
    public IndustryType industryType;
}

[CreateAssetMenu]
public class Cargo: ScriptableObject
{
    public string cargoName;
    public string units;
    public CargoType cargoType;
    public int value;

    void Foobar()
    {

    }
}

public enum IndustryType
{
    Primary,
    Secondary,
    Tertiary
}

public enum CargoType
{
    bulk,
    liquid,
    breakbulk,
    covered,
    live,
    refrigerated,
    passenger,
    armoured
}