using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI; // <-- ¡Obligatorio para poder editar imágenes de la interfaz!
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
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
 
    // 1. Aquí declaras tus variables (pueden ser public o privadas, según necesites)
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
    public GameObject jugador; // Referencia al jugador en la escena

    GameObject corazones; // Referencia a los corazones en la escena (si los usas para mostrar la vida)
    public GameObject monedaPrefab;
    public GameObject corazonPrefab;
    public GameObject experienciaPrefab;

    [Header("UI y Corazones")]
    public GameObject corazonUIPrefab; // El prefab del corazón (¡debe ser un objeto de UI!)
    public Transform contenedorCorazones; // Un panel o Layout Group en tu Canvas
    public List<GameObject> listaCorazonesUI = new List<GameObject>(); // Aquí los guardaremos

    [Header("Sprites de Corazones")]
    public Sprite corazonLleno;
    public Sprite corazonMitad;
    public Sprite corazonVacio;

    [Header("Weapons")]
    public int Armas;
    // espada 1
    // martillo 2
    // Guadaña 3
    // Lanza 4
    public GameObject attackAreaEspadalv1;
    public GameObject attackAreaEspadalv2;
    public GameObject attackAreaMartillolv1;
    public GameObject attackAreaMartillolv2;
    public GameObject attackAreaGuadañalv1;
    public GameObject attackAreaGuadañalv2;
    public GameObject attackAreaLanzalv1;
    public GameObject attackAreaLanzalv2;
    public GameObject areaActual;

    public int ataqueActual; // Variable para saber qué ataque se está usando, para el hit stop
    // ataque 0 = normal
    // ataque 1 = normal combo 2
    // ataque 2 = normal combo 3
    // ataque 3 = cargado
    // ataque 4 = en salto
    // ataque 5 = en dash
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
        jugador = GameObject.FindWithTag("Player");
        CrearCorazones(); 
        
        // ¡IMPORTANTE! Si vidaMaxima son 3 corazones, tu vidaActual empieza en 6 (mitades)
        vidaActual = vidaMaxima * 2; 
        
        GuardarEstadisticas();
        PlayerPrefs.SetString("Nombre", "");
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (juegoPausado)
            {
                Time.timeScale = 1f; // Reanuda el juego
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
        if (Input.GetKeyDown(KeyCode.K))
        {
            GanarExperiencia(50);
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            Curar(1);
        }
        if (Input.GetKeyDown(KeyCode.J))
        {            
            GanarMonedas(10);
        }
        if (Input.GetKeyDown(KeyCode.U))
        {
            CambiarArma(1);
        }
        if (Input.GetKeyDown(KeyCode.I))
        {
            CambiarArma(2);
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            CambiarArma(3);
        }
        if (Input.GetKeyDown(KeyCode.P))
        {            CambiarArma(4);
        }
    }
    public void GuardarEstadisticas()
    {
        // Progresión
        
        // 3. ¡Obligatorio! Guardar en disco para que no se pierda al cerrar el juego
        //PlayerPrefs.Save();
        
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
            // Aquí puedes asignar los datos cargados a tus variables
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

            // Y también puedes mover al jugador a la posición guardada
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
    public void crearVida()
    {
        vidaActual = vidaMaxima;
        for (int i = 0; i < vidaMaxima; i++)
        {
            // Aquí puedes instanciar un corazón o actualizar tu UI de vida
            Debug.Log("Corazón " + (i + 1) + " creado");
            
        }
    }

    public void ActivarHitStop()
    {
        float duracionHitStop = 0.1f; // Duración del hit stop en segundos
        switch (ataqueActual) // <-- ¡Solo he cambiado la 's' a minúscula!
        {
            case 0:
                duracionHitStop = 0.1f;
                break;
            case 1:
                duracionHitStop = 0.12f;
                break;
            case 2:
                duracionHitStop = 0.15f;
                break;
            case 3:
                duracionHitStop = 0.3f;
                break;
            case 4:
                duracionHitStop = 0.18f;
                break;
            case 5:
                duracionHitStop = 0.25f;
                break;
            default:
                duracionHitStop = 0.1f;
                break;
        }
        
        // Le pasamos el ingrediente a la corrutina
        StartCoroutine(HitStop(duracionHitStop));
    }
    IEnumerator HitStop(float duracion)
    {
        Time.timeScale = 0.5f;
    
    // ESTA ES LA LÍNEA QUE FALTA PARA QUE EL ERROR DESAPAREZCA
        yield return new WaitForSecondsRealtime(duracion); 
    
        Time.timeScale = 1f;
        ResetearAtaques();

        
    }
    public void ResetearAtaques()
    {
        ataqueActual = 0;
        // Aquí también podrías resetear cualquier bool o trigger de ataque en tu animator
    }
    public void PausarJuego()
    {
        Time.timeScale = 0f;
    }
    public void ReanudarJuego()
    {
        Time.timeScale = 1f;
    }
    public void GanarExperiencia(float cantidad)
    {
        experiencia += cantidad * multiplicadorDeExperiencia;
        Debug.Log("¡Has ganado " + (cantidad * multiplicadorDeExperiencia) + " de experiencia! Total: " + experiencia);
        if (experiencia >= 100 + (nivel * 10))
        {
            SubirNivel();
        }
        // Aquí podrías agregar lógica para subir de nivel si alcanzas cierta cantidad de experiencia
    }
    public void Curar(int cantidad)
    {
        vidaActual += cantidad;
        if (vidaActual > vidaMaxima)
        {
            vidaActual = vidaMaxima;
        }
        Debug.Log("¡Has sido curado por " + cantidad + "! Vida actual: " + vidaActual);
        // Aquí podrías actualizar tu UI de vida para reflejar el cambio
    }
    public void GanarMonedas(int cantidad)
    {
        monedas += cantidad;
        // Aquí podrías agregar lógica para sumar monedas al jugador
        Debug.Log("¡Has ganado " + cantidad + " monedas!"); 
    }
    public void SubirNivel()
    {
        
    }
    private void GameOver()
    {
        Destroy(jugador);

    }
    public void CrearCorazones()
    {
        // 1. Limpiamos por si acaso ya había corazones de antes
        foreach (GameObject corazon in listaCorazonesUI)
        {
            if (corazon != null) Destroy(corazon);
        }
        listaCorazonesUI.Clear();

        // 2. Comprobamos que no se nos olvidó poner las referencias
        if (corazonUIPrefab == null || contenedorCorazones == null)
        {
            Debug.LogWarning("Falta asignar el Prefab del Corazón UI o el Contenedor en el GameManager");
            return;
        }

        // 3. Instanciamos tantos corazones como vida máxima tengamos
        for (int i = 0; i < vidaMaxima; i++)
        {
            // Instanciamos el corazón dentro del contenedor del Canvas
            GameObject nuevoCorazon = Instantiate(corazonUIPrefab, contenedorCorazones);
            
            // Lo guardamos en la lista para poder cambiarlo de color o apagarlo luego
            listaCorazonesUI.Add(nuevoCorazon); 
        }

        Debug.Log("Se han creado " + vidaMaxima + " corazones en la UI.");
    }
public void CambiarCorazones()
    {
        for (int i = 0; i < listaCorazonesUI.Count; i++)
        {
            // En lugar de buscar la Image, buscamos el Animator de tu prefab
            Animator animCorazon = listaCorazonesUI[i].GetComponent<Animator>();
            
            if (animCorazon == null) continue; // Por seguridad, si el prefab no tiene animator, lo salta

            int valorParaEstarLleno = (i + 1) * 2; 

            // Le pasamos el número exacto al Animator
            if (vidaActual >= valorParaEstarLleno)
            {
                animCorazon.SetInteger("EstadoVida", 2); // 2 = Lleno
            }
            else if (vidaActual == valorParaEstarLleno - 1)
            {
                animCorazon.SetInteger("EstadoVida", 1); // 1 = Mitad
            }
            else
            {
                animCorazon.SetInteger("EstadoVida", 0); // 0 = Vacío
            }
        }
    }
    public void CambiarArma(int nuevoTipoArma)
    {
        // Aquí podrías cambiar el sprite del arma que tiene el jugador, o activar/desactivar modelos 3D, etc.
        Armas = nuevoTipoArma;
        Debug.Log("¡Has cambiado de arma! Arma actual: " + Armas);
        jugador.GetComponent<MainCharacterMovement>().CambiarArma();
        if (Armas == 1|| Armas == 5)
        {
            duracionAtaque1 = ataque1Espada;
            duracionAtaque2 = ataque2Espada;
            duracionAtaque3 = ataque3Espada;
            duracionAtaqueCargado = ataqueCargadoEspada;
            duracionAtaqueSalto = ataqueSaltoEspada;
            duracionAtaqueDash = ataqueDashEspada;
            duracionAtaqueEnElAire = ataqueEnElAireEspada;
            if (Armas == 1)
            {
                areaActual = attackAreaEspadalv1;
            }
            else
            {
                areaActual = attackAreaEspadalv2;
            }
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
            if (Armas == 2)
            {
                areaActual = attackAreaMartillolv1;
            }
            else
            {
                areaActual = attackAreaMartillolv2;
            }
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
            if (Armas == 3)
            {
                areaActual = attackAreaGuadañalv1;
            }
            else
            {
                areaActual = attackAreaGuadañalv2;
            }
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
            if (Armas == 4)
            {
                areaActual = attackAreaLanzalv1;
            }
            else
            {
                areaActual = attackAreaLanzalv2;
            }
        }

    }
}
