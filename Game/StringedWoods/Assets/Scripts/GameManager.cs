using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
 
    // 1. Aquí declaras tus variables (pueden ser public o privadas, según necesites)
    [Header("Progresión")]
    public float experiencia;
    public float multiplicadorDeExperiencia;
    public int nivel;

    [Header("Supervivencia")]
    public int vida;
    public int vidaMaxima;
    public float curacionDeVida;

    [Header("Ataque")]
    public float dañoAlAtacar;
    public float velocidadDeAtaque;
    public float rangoDeAtaque;
    public float aumentoDeDañoCritico;

    [Header("Movimiento y Utilidad")]
    public float cooldownDeDash;
    public float tiempoDeDash;
    public float velocidadDeMovimiento;
    public float fuerzaDeEmpuje;
    public float suerte;

    // 2. Llama a esta función cuando quieras guardar todos los datos de golpe
    


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        // 2. Al despertar, el script dice: "¡Yo soy la Instancia!"
        if (Instance == null)
        {
            Instance = this; // "this" significa "este script exacto"
            DontDestroyOnLoad(gameObject); // Opcional: Para que no se destruya al cambiar de nivel
        }
        else
        {
            // Si ya existía uno (por ejemplo, al recargar el nivel), destruimos la copia
            Destroy(gameObject);
        }
    }
    void Start()
    {
        GuardarEstadisticas();
        PlayerPrefs.SetString("Nombre", "");

    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void GuardarEstadisticas()
    {
        // Progresión
        PlayerPrefs.SetFloat("experiencia", experiencia);
        PlayerPrefs.SetFloat("multiplicadorDeExperiencia", multiplicadorDeExperiencia);
        PlayerPrefs.SetInt("nivel", nivel);

        // Supervivencia
        PlayerPrefs.SetInt("vida", vida);
        PlayerPrefs.SetInt("vidaMaxima", vidaMaxima);
        PlayerPrefs.SetFloat("curacionDeVida", curacionDeVida);

        // Ataque
        PlayerPrefs.SetFloat("dañoAlAtacar", dañoAlAtacar);
        PlayerPrefs.SetFloat("velocidadDeAtaque", velocidadDeAtaque);
        PlayerPrefs.SetFloat("rangoDeAtaque", rangoDeAtaque);
        PlayerPrefs.SetFloat("aumentoDeDañoCritico", aumentoDeDañoCritico);

        // Movimiento y Utilidad
        PlayerPrefs.SetFloat("cooldownDeDash", cooldownDeDash);
        PlayerPrefs.SetFloat("tiempoDeDash", tiempoDeDash);
        PlayerPrefs.SetFloat("velocidadDeMovimiento", velocidadDeMovimiento);
        PlayerPrefs.SetFloat("fuerzaDeEmpuje", fuerzaDeEmpuje);
        PlayerPrefs.SetFloat("suerte", suerte);

        // 3. ¡Obligatorio! Guardar en disco para que no se pierda al cerrar el juego
        PlayerPrefs.Save();
        
        Debug.Log("¡Todas las estadísticas han sido guardadas correctamente!");
    }
}
