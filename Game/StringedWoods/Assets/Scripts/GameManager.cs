using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
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
    public int vidaMaxima;
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
    GameObject jugador; // Referencia al jugador en la escena

    GameObject corazones; // Referencia a los corazones en la escena (si los usas para mostrar la vida)

    public int ataqueActual; // Variable para saber qué ataque se está usando, para el hit stop
    // ataque 0 = normal
    // ataque 1 = normal combo 2
    // ataque 2 = normal combo 3
    // ataque 3 = cargado
    // ataque 4 = en salto
    // ataque 5 = en dash
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
        GameObject jugador = GameObject.FindWithTag("Player");
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
        Time.timeScale = 0f;
    
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

}
