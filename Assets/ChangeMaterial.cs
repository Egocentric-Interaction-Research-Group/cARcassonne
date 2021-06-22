using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeMaterial : MonoBehaviour
{
    public Material pressMat, releaseMat;
    public void ChangeMaterialOnPress()
    {
        GetComponent<MeshRenderer>().material = pressMat;
    }
    public void ChangeMaterialOnRelease()
    {
        GetComponent<MeshRenderer>().material = releaseMat;
    }
}
