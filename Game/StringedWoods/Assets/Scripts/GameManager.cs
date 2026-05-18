using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI; 

// --- CLASES DE GUARDADO ---
[System.Serializable]
public class DatosGuardado {
    public DatosJugador jugador;
    public Estadisticas stats;
}

[System.Serializable]
public class DatosJugador {
    public float[] posicion = new float[3]; // [x, y, z]
    public int salud;
    public float experiencia;
    public int nivel;
    public int monedas;
}

[System.Serializable]
public class Estadisticas {
    public float dañoAlAtacar;
    public int saludMaxima;
    public float curacionDeVida;
    public float velocidadDeAtaque;
    public float rangoDeAtaque;
    public float aumentoDeDañoCritico;
    public float cooldownDeDash;
    public float tiempoDeDash;
    public float velocidadDeMovimiento;
    public float fuerzaDeEmpuje;
    public float suerte;
}

// --- CLASES PARA LAS CARTAS DE MEJORA ---
public enum TipoEstadistica 
{ 
    VidaMaxima, DanoAlAtacar, VelocidadAtaque, RangoAtaque, CooldownDash, TiempoDash, VelocidadMovimiento, Suerte, FuerzaEmpuje 
}

[System.Serializable]
public class CartaMejora
{
    public string calidad;
    public TipoEstadistica estadistica;
    public float valorMejora; 
    public string textoDescripcion;
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
 
    [Header("Progresión")]
    public float experiencia;
    public float multiplicadorDeExperiencia;
    public int nivel;
    public int monedas;

    [Header("Supervivencia")]
    public int vidaActual;
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

    [Header("Otros")]
    public GameObject jugador; 
    public bool salaEspecial;

    public MainCharacterMovement PlayerScript;

    GameObject corazones; 
    public GameObject monedaPrefab;
    public GameObject corazonPrefab;

    [Header("Prefabs de Experiencia")]
    public GameObject exp1Prefab;
    public GameObject exp5Prefab;
    public GameObject exp10Prefab;
    public GameObject exp20Prefab;
    public GameObject exp50Prefab;
    public GameObject exp100Prefab;

    [Header("UI y Corazones")]
    public GameObject corazonUIPrefab; 
    public Transform contenedorCorazones; 
    public List<GameObject> listaCorazonesUI = new List<GameObject>(); 

    [Header("Sprites de Corazones")]
    public Sprite corazonLleno;
    public Sprite corazonMitad;
    public Sprite corazonVacio;

    [Header("Weapons")]
    public int Armas;
    // espada 1, martillo 2, Guadaña 3, Lanza 4
    public GameObject attackAreaEspadalv1;
    public GameObject attackAreaEspadalv2;
    public GameObject attackAreaMartillolv1;
    public GameObject attackAreaMartillolv2;
    public GameObject attackAreaGuadañalv1;
    public GameObject attackAreaGuadañalv2;
    public GameObject attackAreaLanzalv1;
    public GameObject attackAreaLanzalv2;
    public GameObject areaActual;

    public int ataqueActual; 

    [Header ("Mejoras de Subir de Nivel")]
    public int probabilidadDeEspecial;
    public int probabilidadDeEpica;
    public int probabilidadDeLegendario;
    public List<CartaMejora> cartasOfertadas = new List<CartaMejora>(); // Lista de las 3 cartas actuales

    [Header("Timmings armas")]
    public float ataque1Espada;
    public float ataque2Espada;
    public float ataque3Espada;
    public float ataqueEnElAireEspada;
    public float ataqueCargadoEspada;
    public float ataqueSaltoEspada;
    public float ataqueDashEspada;
    public float ataque1Martillo;
    public float ataque2Martillo;
    public float ataque3Martillo;
    public float ataqueEnElAireMartillo;
    public float ataqueCargadoMartillo;
    public float ataqueSaltoMartillo;
    public float ataqueDashMartillo;
    public float ataque1Guadaña;
    public float ataque2Guadaña;
    public float ataque3Guadaña;
    public float ataqueEnElAireGuadaña;
    public float ataqueCargadoGuadaña;
    public float ataqueSaltoGuadaña;
    public float ataqueDashGuadaña;
    public float ataque1Lanza;
    public float ataque2Lanza;
    public float ataque3Lanza;
    public float ataqueEnElAireLanza;
    public float ataqueCargadoLanza;
    public float ataqueSaltoLanza;
    public float ataqueDashLanza;
    public float duracionAtaque1;
    public float duracionAtaque2;
    public float duracionAtaque3;
    public float duracionAtaqueEnElAire;
    public float duracionAtaqueCargado;
    public float duracionAtaqueSalto;
    public float duracionAtaqueDash;

    [Header("Estado del Juego")]
    public bool juegoPausado = false;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this; 
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
         
        salaEspecial = false;
        
        // 1. Encuentra al jugador
        jugador = GameObject.FindWithTag("Player");
        PlayerScript = jugador.GetComponent<MainCharacterMovement>();
        
        // 2. Coge las áreas de ataque desde el script del jugador
        AsignarAreasDeAtaque();

        CrearCorazones(); 
        vidaActual = vidaMaxima * 2; 
        
        GuardarEstadisticas();
        PlayerPrefs.SetString("Nombre", "");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (juegoPausado)
            {
                Time.timeScale = 1f; 
                juegoPausado = false;
            }
            else
            {
                PausarJuego();
                juegoPausado = true;
            }
        }

        if (vidaActual <= 0) 
        {
            GameOver();
        }

        // Controles de prueba
        if (Input.GetKeyDown(KeyCode.K)) GanarExperiencia(50);
        if (Input.GetKeyDown(KeyCode.L)) Curar(1);
        if (Input.GetKeyDown(KeyCode.J)) GanarMonedas(10);
        
        if (Input.GetKeyDown(KeyCode.U)) CambiarArma(1);
        if (Input.GetKeyDown(KeyCode.I)) CambiarArma(2);
        if (Input.GetKeyDown(KeyCode.O)) CambiarArma(3);
        if (Input.GetKeyDown(KeyCode.P)) CambiarArma(4);
    }

    // --- SISTEMA DE GUARDADO ---
    public void GuardarEstadisticas()
    {
        Debug.Log("¡Todas las estadísticas han sido guardadas correctamente!");
    }

    public void GuardarPartida()
    {
        DatosGuardado misDatos = new DatosGuardado();
        misDatos.stats =  new Estadisticas();
        misDatos.jugador =  new DatosJugador();
        misDatos.jugador.posicion[0] = jugador.transform.position.x;
        misDatos.jugador.posicion[1] = jugador.transform.position.y;
        misDatos.jugador.posicion[2] = jugador.transform.position.z;
        misDatos.jugador.salud = vidaActual;
        misDatos.stats.saludMaxima = vidaMaxima;
        misDatos.jugador.experiencia = experiencia;
        misDatos.jugador.nivel = nivel;
        misDatos.stats.dañoAlAtacar = dañoAlAtacar;
        misDatos.stats.velocidadDeAtaque = velocidadDeAtaque;
        misDatos.stats.rangoDeAtaque = rangoDeAtaque;
        misDatos.stats.aumentoDeDañoCritico = aumentoDeDañoCritico;
        misDatos.stats.cooldownDeDash = cooldownDeDash;
        misDatos.stats.tiempoDeDash = tiempoDeDash;
        misDatos.stats.velocidadDeMovimiento = velocidadDeMovimiento;
        misDatos.stats.fuerzaDeEmpuje = fuerzaDeEmpuje;
        misDatos.stats.suerte = suerte;

        string json = JsonUtility.ToJson(misDatos);
        File.WriteAllText(Application.persistentDataPath + "/guardado.json", json);
        Debug.Log("¡Partida guardada en: " + Application.persistentDataPath + "/guardado.json");
    }

    public void CargarPartida()
    {
        string ruta = Application.persistentDataPath + "/guardado.json";
        if (File.Exists(ruta))
        {
            string json = File.ReadAllText(ruta);
            DatosGuardado misDatos = JsonUtility.FromJson<DatosGuardado>(json);
            
            vidaActual = misDatos.jugador.salud;
            vidaMaxima = misDatos.stats.saludMaxima;
            experiencia = misDatos.jugador.experiencia;
            nivel = misDatos.jugador.nivel;
            dañoAlAtacar = misDatos.stats.dañoAlAtacar;
            velocidadDeAtaque = misDatos.stats.velocidadDeAtaque;
            rangoDeAtaque = misDatos.stats.rangoDeAtaque;
            aumentoDeDañoCritico = misDatos.stats.aumentoDeDañoCritico;
            cooldownDeDash = misDatos.stats.cooldownDeDash;
            tiempoDeDash = misDatos.stats.tiempoDeDash;
            velocidadDeMovimiento = misDatos.stats.velocidadDeMovimiento;
            fuerzaDeEmpuje = misDatos.stats.fuerzaDeEmpuje;
            suerte = misDatos.stats.suerte;

            jugador.transform.position = new Vector3(misDatos.jugador.posicion[0], misDatos.jugador.posicion[1], misDatos.jugador.posicion[2]);

            Debug.Log("¡Partida cargada correctamente!");
        }
        else
        {
            Debug.LogWarning("No se encontró ningún archivo de guardado en: " + ruta);
        }
    }

    public void BorrarPartida()
    {
        string ruta = Application.persistentDataPath + "/guardado.json";
        if (File.Exists(ruta))
        {
            File.Delete(ruta);
            Debug.Log("¡Archivo de guardado borrado correctamente!");
        }
        else
        {
            Debug.LogWarning("No se encontró ningún archivo de guardado para borrar en: " + ruta);
        }
    }

    // --- SISTEMA DE VIDA ---
    public void crearVida()
    {
        vidaActual = vidaMaxima;
        for (int i = 0; i < vidaMaxima; i++)
        {
            Debug.Log("Corazón " + (i + 1) + " creado");
        }
    }

    public void CrearCorazones()
    {
        foreach (GameObject corazon in listaCorazonesUI)
        {
            if (corazon != null) Destroy(corazon);
        }
        listaCorazonesUI.Clear();

        if (corazonUIPrefab == null || contenedorCorazones == null)
        {
            Debug.LogWarning("Falta asignar el Prefab del Corazón UI o el Contenedor en el GameManager");
            return;
        }

        for (int i = 0; i < vidaMaxima; i++)
        {
            GameObject nuevoCorazon = Instantiate(corazonUIPrefab, contenedorCorazones);
            listaCorazonesUI.Add(nuevoCorazon); 
        }

        Debug.Log("Se han creado " + vidaMaxima + " corazones en la UI.");
    }

    public void CambiarCorazones()
    {
        for (int i = 0; i < listaCorazonesUI.Count; i++)
        {
            Animator animCorazon = listaCorazonesUI[i].GetComponent<Animator>();
            if (animCorazon == null) continue; 

            int valorParaEstarLleno = (i + 1) * 2; 

            if (vidaActual >= valorParaEstarLleno)
            {
                animCorazon.SetInteger("EstadoVida", 2); 
            }
            else if (vidaActual == valorParaEstarLleno - 1)
            {
                animCorazon.SetInteger("EstadoVida", 1); 
            }
            else
            {
                animCorazon.SetInteger("EstadoVida", 0); 
            }
        }
    }

    public void Curar(int cantidad)
    {
        vidaActual += cantidad;
        if (vidaActual > vidaMaxima * 2) 
        {
            vidaActual = vidaMaxima * 2;
        }
        Debug.Log("¡Has sido curado por " + cantidad + "! Vida actual: " + vidaActual);
        CambiarCorazones(); 
    }

    // --- HIT STOP ---
    public void ActivarHitStop()
    {
        float duracionHitStop = 0.1f; 
        switch (ataqueActual) 
        {
            case 0: duracionHitStop = 0.1f; break;
            case 1: duracionHitStop = 0.12f; break;
            case 2: duracionHitStop = 0.15f; break;
            case 3: duracionHitStop = 0.3f; break;
            case 4: duracionHitStop = 0.18f; break;
            case 5: duracionHitStop = 0.25f; break;
            default: duracionHitStop = 0.1f; break;
        }
        
        StartCoroutine(HitStop(duracionHitStop));
    }

    IEnumerator HitStop(float duracion)
    {
        Time.timeScale = 0.5f;
        yield return new WaitForSecondsRealtime(duracion); 
        Time.timeScale = 1f;
        ResetearAtaques();
    }

    public void ResetearAtaques()
    {
        ataqueActual = 0;
    }

    public void PausarJuego()
    {
        Time.timeScale = 0f;
    }

    public void ReanudarJuego()
    {
        Time.timeScale = 1f;
    }

    // --- PROGRESIÓN Y RECOMPENSAS ---
    public void GanarExperiencia(float cantidad)
    {
        experiencia += cantidad * multiplicadorDeExperiencia;
        Debug.Log("¡Has ganado " + (cantidad * multiplicadorDeExperiencia) + " de experiencia! Total: " + experiencia);
        if (experiencia >= 100 + (nivel * 15))
        
        {
            experiencia = 0;
            SubirNivel();
        }
    }

    public void GanarMonedas(int cantidad)
    {
        monedas += cantidad;
        Debug.Log("¡Has ganado " + cantidad + " monedas!"); 
    }

    public void SubirNivel()
    {
        nivel++; 
        cartasOfertadas.Clear(); 
        
        int numeroDeMejoras = 3;
        int sumaCalidades = probabilidadDeEspecial + probabilidadDeEpica + probabilidadDeLegendario;
        
        for (int i = 0; i < numeroDeMejoras; i++)
        {
            CartaMejora nuevaCarta = new CartaMejora();
            
            int numeroCalidadTargeta = Random.Range(0, sumaCalidades);
            float multiplicador = 1f;

            if (numeroCalidadTargeta < probabilidadDeLegendario)
            {
                nuevaCarta.calidad = "Legendaria";
                multiplicador = 2.3f;
            }
            else if (numeroCalidadTargeta < probabilidadDeLegendario + probabilidadDeEpica)
            {
                nuevaCarta.calidad = "Epica";
                multiplicador = 1.5f;
            }
            else
            {
                nuevaCarta.calidad = "Especial";
                multiplicador = 1f;
            }

            int statRandom = Random.Range(0, 9);
            nuevaCarta.estadistica = (TipoEstadistica)statRandom;

            switch (nuevaCarta.estadistica)
            {
                case TipoEstadistica.VidaMaxima:
                    nuevaCarta.valorMejora = Mathf.RoundToInt(2f * multiplicador); 
                    nuevaCarta.textoDescripcion = "+" + nuevaCarta.valorMejora + " Mitades de Vida";
                    break;
                case TipoEstadistica.DanoAlAtacar:
                    nuevaCarta.valorMejora = 2f * multiplicador; 
                    nuevaCarta.textoDescripcion = "+" + nuevaCarta.valorMejora.ToString("F1") + " Daño";
                    break;
                case TipoEstadistica.VelocidadAtaque:
                    nuevaCarta.valorMejora = 0.1f * multiplicador; 
                    nuevaCarta.textoDescripcion = "+" + nuevaCarta.valorMejora.ToString("F2") + " Vel. Ataque";
                    break;
                case TipoEstadistica.RangoAtaque:
                    nuevaCarta.valorMejora = 0.1f * multiplicador;
                    nuevaCarta.textoDescripcion = "+" + nuevaCarta.valorMejora.ToString("F1") + " Rango";
                    break;
                case TipoEstadistica.CooldownDash:
                    nuevaCarta.valorMejora = -0.2f * multiplicador; 
                    nuevaCarta.textoDescripcion = nuevaCarta.valorMejora.ToString("F2") + "s Cooldown Dash";
                    break;
                case TipoEstadistica.TiempoDash:
                    nuevaCarta.valorMejora = 0.05f * multiplicador;
                    nuevaCarta.textoDescripcion = "+" + nuevaCarta.valorMejora.ToString("F2") + "s Duración Dash";
                    break;
                case TipoEstadistica.VelocidadMovimiento:
                    nuevaCarta.valorMejora = 1.5f * multiplicador;
                    nuevaCarta.textoDescripcion = "+" + nuevaCarta.valorMejora.ToString("F1") + " Vel. Movimiento";
                    break;
                case TipoEstadistica.Suerte:
                    nuevaCarta.valorMejora = 2f * multiplicador;
                    nuevaCarta.textoDescripcion = "+" + nuevaCarta.valorMejora.ToString("F0") + " Suerte";
                    break;
                case TipoEstadistica.FuerzaEmpuje:
                    nuevaCarta.valorMejora = 2f * multiplicador;
                    nuevaCarta.textoDescripcion = "+" + nuevaCarta.valorMejora.ToString("F1") + " Fuerza de Empuje";
                    break;
            }

            cartasOfertadas.Add(nuevaCarta);
            Debug.Log($"Carta {i+1} generada: {nuevaCarta.calidad} - {nuevaCarta.textoDescripcion}");
        }
        
        // Avisamos a la UI para que muestre las cartas (Asegúrate de tener el script UiManager listo en Unity)
        if (UiManager.Instance != null)
        {
            UiManager.Instance.MostrarMenuSubirNivel();
        }
        PausarJuego();
    }

    public void SeleccionarCarta(int indiceCartaSeleccionada)
    {
        CartaMejora cartaElegida = cartasOfertadas[indiceCartaSeleccionada];

        switch (cartaElegida.estadistica)
        {
            case TipoEstadistica.VidaMaxima:
                int subidaDeCorazonesEnteros = (int)cartaElegida.valorMejora / 2; 
                vidaMaxima += subidaDeCorazonesEnteros;
                CrearCorazones(); 
                Curar((int)cartaElegida.valorMejora); 
                break;
            case TipoEstadistica.DanoAlAtacar:
                dañoAlAtacar += cartaElegida.valorMejora;
                break;
            case TipoEstadistica.VelocidadAtaque:
                velocidadDeAtaque += cartaElegida.valorMejora;
                break;
            case TipoEstadistica.RangoAtaque:
                rangoDeAtaque += cartaElegida.valorMejora;
                break;
            case TipoEstadistica.CooldownDash:
                cooldownDeDash += cartaElegida.valorMejora; 
                if (cooldownDeDash < 0.1f) cooldownDeDash = 0.1f; 
                break;
            case TipoEstadistica.TiempoDash:
                tiempoDeDash += cartaElegida.valorMejora;
                break;
            case TipoEstadistica.VelocidadMovimiento:
                velocidadDeMovimiento += cartaElegida.valorMejora;
                break;
            case TipoEstadistica.Suerte:
                suerte += cartaElegida.valorMejora;
                break;
            case TipoEstadistica.FuerzaEmpuje:
                fuerzaDeEmpuje += cartaElegida.valorMejora;
                break;
        }

        Debug.Log("¡Has aplicado la mejora: " + cartaElegida.textoDescripcion + "!");
        
        // --- AQUÍ CONECTAMOS Y ACTUALIZAMOS LAS ESTADÍSTICAS DEL JUGADOR ---
        if (jugador != null)
        {
            jugador.GetComponent<MainCharacterMovement>().ActualizarEstadisticas();
        }

        cartasOfertadas.Clear();
    }

    private void GameOver()
    {
        Destroy(jugador);
    }

    public void CambioDeSalaEspecial()
    {
        if (salaEspecial == false)
        {
            salaEspecial = true;
            Debug.Log("¡Has entrado en la sala especial!");
        }
        else
        {
            salaEspecial = false;
            Debug.Log("¡Has salido de la sala especial!");
        }
    }
    public void AsignarAreasDeAtaque()
    {
        if (jugador != null)
        {
            // Accedemos al script del jugador
            MainCharacterMovement scriptJugador = jugador.GetComponent<MainCharacterMovement>();
            
            if (scriptJugador != null)
            {
                // Cogemos las áreas directamente desde las variables de tu script del jugador
                attackAreaEspadalv1 = scriptJugador.attackAreaEspadalv1;
                attackAreaEspadalv2 = scriptJugador.attackAreaEspadalv2;
                
                attackAreaMartillolv1 = scriptJugador.attackAreaMartillolv1;
                attackAreaMartillolv2 = scriptJugador.attackAreaMartillolv2;
                
                attackAreaGuadañalv1 = scriptJugador.attackAreaGuadañalv1;
                attackAreaGuadañalv2 = scriptJugador.attackAreaGuadañalv2;
                
                attackAreaLanzalv1 = scriptJugador.attackAreaLanzalv1;
                attackAreaLanzalv2 = scriptJugador.attackAreaLanzalv2;

                Debug.Log("¡Áreas de ataque copiadas desde el script MainCharacterMovement!");
            }
        }
    }

    public void CambiarArma(int nuevoTipoArma)
    {
        Armas = nuevoTipoArma;
        Debug.Log("¡Has cambiado de arma! Arma actual: " + Armas);
        
        if (jugador != null)
        {
            jugador.GetComponent<MainCharacterMovement>().CambiarArma();
        }

        if (Armas == 1 || Armas == 5)
        {
            duracionAtaque1 = ataque1Espada;
            duracionAtaque2 = ataque2Espada;
            duracionAtaque3 = ataque3Espada;
            duracionAtaqueCargado = ataqueCargadoEspada;
            duracionAtaqueSalto = ataqueSaltoEspada;
            duracionAtaqueDash = ataqueDashEspada;
            duracionAtaqueEnElAire = ataqueEnElAireEspada;
            if (Armas == 1) areaActual = attackAreaEspadalv1;
            else areaActual = attackAreaEspadalv2;
        }
        else if (Armas == 2 || Armas == 6)
        {
            duracionAtaque1 = ataque1Martillo;
            duracionAtaque2 = ataque2Martillo;
            duracionAtaque3 = ataque3Martillo;
            duracionAtaqueCargado = ataqueCargadoMartillo;
            duracionAtaqueSalto = ataqueSaltoMartillo;
            duracionAtaqueDash = ataqueDashMartillo;
            duracionAtaqueEnElAire = ataqueEnElAireMartillo;
            if (Armas == 2) areaActual = attackAreaMartillolv1;
            else areaActual = attackAreaMartillolv2;
        }
        else if (Armas == 3 || Armas == 7)
        {
            duracionAtaque1 = ataque1Guadaña;
            duracionAtaque2 = ataque2Guadaña;
            duracionAtaque3 = ataque3Guadaña;
            duracionAtaqueCargado = ataqueCargadoGuadaña;
            duracionAtaqueSalto = ataqueSaltoGuadaña;
            duracionAtaqueDash = ataqueDashGuadaña;
            duracionAtaqueEnElAire = ataqueEnElAireGuadaña;
            if (Armas == 3) areaActual = attackAreaGuadañalv1;
            else areaActual = attackAreaGuadañalv2;
        }
        else if (Armas == 4 || Armas == 8)
        {
            duracionAtaque1 = ataque1Lanza;
            duracionAtaque2 = ataque2Lanza;
            duracionAtaque3 = ataque3Lanza;
            duracionAtaqueCargado = ataqueCargadoLanza;
            duracionAtaqueSalto = ataqueSaltoLanza;
            duracionAtaqueDash = ataqueDashLanza;
            duracionAtaqueEnElAire = ataqueEnElAireLanza;
            if (Armas == 4) areaActual = attackAreaLanzalv1;
            else areaActual = attackAreaLanzalv2;
        }
    }
}