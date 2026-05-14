using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIHovering : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    //Change color
    public Image BGImage;
    public Color vertexColor = Color.white;
    public Color hoverColor = Color.violet;

    //Change text color 
    public TextMeshProUGUI btnText;
    public Color normalTextColor = Color.white;
    public Color hoverTextColor = Color.violet;
    public float textColorSpeed = 5f;
    private Color currentTextColor;

    //Text Glow
    public Shadow textShadow; // Shadow del texto
    public Color textGlowColor = Color.violet;
    public float textGlowSpeed = 5f;
    private Color currentTextGlowColor;

    //Glow
    public Outline outline;
    public Color glowColor = Color.violet;
    public float pulseSpeed = 2f;

    //Scale
    public float ScaleRate = 1.5f;
    public float scaleSpeed = 5f;
    private float currentScale = 1f;

    private Color originalGlowColor;
    private Color originalTextGlowColor;
    private bool isHovered = false;
    private Vector3 originalScale;


    void Start()
    {
        if (outline != null)
        {
            originalGlowColor = outline.effectColor;
        }
        
        if (textShadow != null)
        {
            originalTextGlowColor = textShadow.effectColor;
            currentTextGlowColor = originalTextGlowColor;
        }
        
        originalScale = transform.localScale;
        currentScale = 1f;

        if (BGImage != null)
        {
            BGImage.color = vertexColor;
        }

        if (btnText != null)
        {
            currentTextColor = normalTextColor;
            btnText.color = currentTextColor;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        
        if(BGImage != null) BGImage.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
         isHovered = false;
        
        if(BGImage != null) BGImage.color = vertexColor;
    }

    void Update()
    {
        // Escala gradual
        float targetScale = isHovered ? ScaleRate : 1f;
        currentScale = Mathf.Lerp(currentScale, targetScale, Time.deltaTime * scaleSpeed);
        transform.localScale = originalScale * currentScale;

        // Color del texto gradual
        if (btnText != null)
        {
            Color targetTextColor = isHovered ? hoverTextColor : normalTextColor;
            currentTextColor = Color.Lerp(currentTextColor, targetTextColor, Time.deltaTime * textColorSpeed);
            btnText.color = currentTextColor;
        }

        // Glow del texto (Shadow)
        /*if (textShadow != null)
        {
            Color targetGlowColor = isHovered ? textGlowColor : originalTextGlowColor;
            currentTextGlowColor = Color.Lerp(currentTextGlowColor, targetGlowColor, Time.deltaTime * textGlowSpeed);
            textShadow.effectColor = currentTextGlowColor;
        }*/

        /*if(outline != null)
        {
            if (isHovered)
            {
                float pulse = Mathf.PingPong(Time.time * pulseSpeed, 1f);
                outline.effectColor = Color.Lerp(originalGlowColor, glowColor, pulse);
            }
            else
            {
                outline.effectColor = Color.Lerp(outline.effectColor, originalGlowColor, Time.deltaTime * 5f);
            }
        }*/
    }
}
