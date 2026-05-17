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
    
    // --- NUEVO: Referencia a la barra de experiencia ---
    [Header("UI General")]
    public Slider barraExperiencia;

    [Header("Elementos de las 3 Cartas (Orden: 0, 1, 2)")]
    public Button[] botonesCartas = new Button[3];
    public TextMeshProUGUI[] textosDescripcion = new TextMeshProUGUI[3];
    public TextMeshProUGUI[] textosCalidad = new TextMeshProUGUI[3];
    public Image[] fondosCartas = new Image[3];

    [Header("Colores de las Calidades")]
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

    // --- NUEVO: Actualizamos la barra de experiencia en tiempo real ---
    void Update()
    {
        if (GameManager.Instance != null && barraExperiencia != null)
        {
            // 1. Calculamos cuál es el máximo de experiencia para este nivel
            float experienciaMaxima = 100f + (GameManager.Instance.nivel * 15f);
            
            // 2. Le decimos al Slider cuál es su límite máximo
            barraExperiencia.maxValue = experienciaMaxima;
            
            // 3. Rellenamos el Slider con la experiencia actual del jugador
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

            if (carta.calidad == "Legendaria") fondosCartas[i].color = colorLegendaria;
            else if (carta.calidad == "Epica") fondosCartas[i].color = colorEpica;
            else fondosCartas[i].color = colorEspecial;

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