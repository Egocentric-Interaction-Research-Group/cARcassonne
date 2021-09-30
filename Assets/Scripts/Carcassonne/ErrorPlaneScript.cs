using System.Collections;
using UnityEngine;

public class ErrorPlaneScript : MonoBehaviour
{
    private Material mat;

    private bool ready = true;

    // Start is called before the first frame update
    private void Start()
    {
        mat = GetComponent<MeshRenderer>().material;
        mat.color = new Color(1, 0, 0, 0);
    }

    private IEnumerator FadeImage(int red, int green)
    {
        ready = false;
        for (float i = 1; i >= 0; i -= Time.deltaTime * 2)
        {
            mat.color = new Color(red, green, 0, i);
            yield return null;
        }

        ready = true;
    }
}