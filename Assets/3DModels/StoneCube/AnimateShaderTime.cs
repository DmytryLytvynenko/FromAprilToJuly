using UnityEngine;

public class AnimateShaderTime : MonoBehaviour
{
    private Material mat;
    private float customTime = 0f;

    public float speed = 1f;

    void Start()
    {
        // Get the material instance
        mat = GetComponent<Renderer>().material;
    }

    void Update()
    {
        customTime += Time.deltaTime * speed;

        customTime = Mathf.Repeat(customTime, 1000f);

        mat.SetFloat("_CustomTime", customTime);
    }
}
