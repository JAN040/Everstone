using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MenuTextAnimator : MonoBehaviour
{
    #region VARIABLES

    [SerializeField]
    private TMP_Text m_TextComponent;

    public float Hue;
    [Range(0,1)]
    public float Saturation = 0.6f;
    [Range(0,1)]
    public float Brightness = 0.7f;
    public float AnimationSpeed = 1f;

	#endregion

	#region UNITY METHODS

    // Start is called before the first frame update

    void Start()
    {
        if (m_TextComponent == null)
        {
            m_TextComponent = GetComponent<TMP_Text>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        float hue, sat, bright;
        Color.RGBToHSV(m_TextComponent.outlineColor, out hue, out sat, out bright);
        hue += AnimationSpeed * Time.deltaTime * 0.25f;
        if (hue > 1)
            hue = 0;

        Hue = hue;
        m_TextComponent.outlineColor = Color.HSVToRGB(hue, Saturation, Brightness);
        m_TextComponent.ForceMeshUpdate();
        //Debug.Log($"Title color: {m_TextComponent.outlineColor}");
    }

    #endregion

}
