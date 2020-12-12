using UnityEngine;

public class LED : MonoBehaviour {
    new public Light light;
    public Texture2D[] materials;

    private Material material;

    private void Start()
    {
        material = gameObject.GetComponent<MeshRenderer>().materials[0];
        light.transform.localScale = transform.lossyScale;
    }

    public enum Colors
    {
        Red = 0,
        Green = 1,
        Yellow = 2
    }

    public void ChangeMaterial(Colors color)
    {
        ChangeMaterial((int)color);
    }

    public void ChangeMaterial(int color)
    {
        material.SetTexture("_UnlitTex", materials[color]);
        switch (color)
        {
            case 0:
                light.color = new Color(1f, 0f, 0f);
                break;
            case 1:
                light.color = new Color(0f, 1f, 0f);
                break;
            case 2:
                light.color = new Color(1f, 1f, 0f);
                break;
        }
    }

    public bool ChangeBrightness(float Brightness)
    {
        material.SetFloat("_Blend", Mathf.Clamp01(Brightness));
        light.intensity = Brightness;
        return true;
    }
}
