using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManageHealth : MonoBehaviour
{
    // Start is called before the first frame update

    public float health;
    public float maxHealth = 5.0f;
    public Transform fill;
    public SpriteRenderer renderer;
    public Color startColor = Color.white;
    public Color endColor = new Color(1.0f, 0.5f, 0.0f);
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float healthPercent = health / maxHealth;
        fill.localScale = new Vector3(healthPercent, fill.localScale.y, 1);
        float newX = -0.5f + (healthPercent/2);
        fill.localPosition = new Vector3(newX, fill.localPosition.y, 0);

        // Interpolate color from white to orange
        Color lerpedColor = Color.Lerp(startColor, endColor, healthPercent);
        renderer.color = lerpedColor;

    }
}
