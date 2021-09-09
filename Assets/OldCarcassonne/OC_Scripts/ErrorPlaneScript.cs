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

    public void flashError()
    {
        //StartCoroutine(FadeImage(false));
        //StartCoroutine(FadeImage(true));
        if (ready) StartCoroutine(FadeImage(1, 0));
    }

    public void flashConfirm()
    {
        if (ready) StartCoroutine(FadeImage(0, 1));
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

    private IEnumerator FadeImage(bool fadeAway)
    {
        if (fadeAway)
            for (float i = 1; i >= 0; i -= Time.deltaTime)
            {
                mat.color = new Color(1, 0, 0, i);
                yield return null;
            }
        else
            for (float i = 0; i <= 1; i += Time.deltaTime)
            {
                mat.color = new Color(1, 0, 0, i);
                yield return null;
            }
    }

    public void UpdatePosition(Vector3 basePosition, int x, int z)
    {
        if (ready)
            transform.position =
                new Vector3(basePosition.x + x * 0.2f, basePosition.y + 0.1f, basePosition.z + z * 0.2f);
    }
}