using UnityEngine;
using UnityEngine.UI; 
using System.Collections;
using TMPro; 

public class UiManager : MonoBehaviour
{
    public static UiManager Instance { get; private set; }

    [Header("Referencias del Canvas")]
    public GameObject panelSubirNivel; 
    public RectTransform contenedorCartas; 
    
    [Header("UI General")]
    public Slider barraExperiencia;

    [Header("Elementos de las 3 Cartas (Orden: 0, 1, 2)")]
    public Button[] botonesCartas = new Button[3];
    public TextMeshProUGUI[] textosDescripcion = new TextMeshProUGUI[3];
    public TextMeshProUGUI[] textosCalidad = new TextMeshProUGUI[3];
    public Image[] fondosCartas = new Image[3];

    // --- CAMBIO: Sustituimos Colores por Sprites ---
    [Header("Sprites de las Calidades")]
    // Asigna aquí tus imágenes de fondo en el Inspector
    public Sprite spriteFondoEspecial; 
    public Sprite spriteFondoEpica;    
    public Sprite spriteFondoLegendaria; 

    // Mantenemos estas por si acaso quieres añadir un borde o texto coloreado,
    // pero para el fondo completo usaremos los Sprites de arriba.
    [Header("Colores de las Calidades (Opcionales)")]
    public Color colorEspecial = new Color(0.2f, 0.8f, 0.2f); // Verde
    public Color colorEpica = new Color(0.8f, 0.2f, 0.8f);    // Morado
    public Color colorLegendaria = new Color(1f, 0.8f, 0f);   // Dorado/Amarillo

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        panelSubirNivel.SetActive(false);
    }

    void Update()
    {
        if (GameManager.Instance != null && barraExperiencia != null)
        {
            float experienciaMaxima = 100f + (GameManager.Instance.nivel * 15f);
            barraExperiencia.maxValue = experienciaMaxima;
            barraExperiencia.value = GameManager.Instance.experiencia;
        }
    }

    public void MostrarMenuSubirNivel()
    {
        panelSubirNivel.SetActive(true);

        for (int i = 0; i < 3; i++)
        {
            CartaMejora carta = GameManager.Instance.cartasOfertadas[i];
            
            textosDescripcion[i].text = carta.textoDescripcion;
            textosCalidad[i].text = carta.calidad;

            // --- LÓGICA ACTUALIZADA: Sustituir Sprite y Resetear Color ---
            
            // Primero, nos aseguramos de que el color de la Image sea blanco puro.
            // Si no hacemos esto, tu nueva textura se teñirá horriblemente.
            fondosCartas[i].color = Color.white; 

            // Segundo, asignamos el sprite que toca según la calidad.
            if (carta.calidad == "Legendaria") 
            {
                fondosCartas[i].sprite = spriteFondoLegendaria;
                // Si quieres colorear el texto de calidad también, usarías esto:
                // textosCalidad[i].color = colorLegendaria; 
            }
            else if (carta.calidad == "Epica") 
            {
                fondosCartas[i].sprite = spriteFondoEpica;
                // textosCalidad[i].color = colorEpica;
            }
            else // Por defecto, Especial (Verde)
            {
                fondosCartas[i].sprite = spriteFondoEspecial;
                // textosCalidad[i].color = colorEspecial;
            }

            int indiceCarta = i; 
            botonesCartas[i].onClick.RemoveAllListeners(); 
            botonesCartas[i].onClick.AddListener(() => AlPulsarCarta(indiceCarta));
        }

        StartCoroutine(AnimarEntradaCartas());
    }

    public void AlPulsarCarta(int indice)
    {
        GameManager.Instance.SeleccionarCarta(indice);
        panelSubirNivel.SetActive(false);
        GameManager.Instance.ReanudarJuego();
    }

    IEnumerator AnimarEntradaCartas()
    {
        float duracion = 0.5f; 
        float tiempo = 0f;

        Vector2 posicionInicial = new Vector2(0, -1000f);
        Vector2 posicionFinal = Vector2.zero; 

        contenedorCartas.anchoredPosition = posicionInicial;

        while (tiempo < duracion)
        {
            tiempo += Time.unscaledDeltaTime; 
            float porcentaje = tiempo / duracion;
            
            float t = Mathf.SmoothStep(0f, 1f, porcentaje);
            
            contenedorCartas.anchoredPosition = Vector2.Lerp(posicionInicial, posicionFinal, t);
            yield return null;
        }

        contenedorCartas.anchoredPosition = posicionFinal;
    }
}