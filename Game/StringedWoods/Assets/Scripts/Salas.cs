using UnityEngine;
using System.Collections.Generic;
using System.Collections; // <--- IMPORTANTE: Asegúrate de tener esto para IEnumerator

public class Salas : MonoBehaviour
{
    [Header("Listas de la Sala")]
    public List<GameObject> EnemigosSala = new List<GameObject>(); 
    public List<SpawnerEnemigos> spawnersDeLaSala = new List<SpawnerEnemigos>(); 

    [Header("Tipos de Sala")]
    public bool salaBoss;
    public bool salaEspecial;
    public bool pasillo;
    public bool salaNormal;
    public bool salaInicial;
    public bool salaTienda;

    [Header("Estados")]
    public bool salaLimpia; 
    public bool jugadorDentro; 

    [Header("Detectores")]
    public GameObject detectorSalaCircundante;
    public GameObject detectorPlayerDentro;

    [Header("Paredes de carton")]
    public Animation paredCartonIzquierda;
    public Animation paredCartonDerecha;
    public Animation paredCartonDelante;

    public Animation paredCartonEspecial;
    [Header("Paredes de Habitacion")]
    public GameObject paredDerecha;
    public GameObject paredIzquierda;
    public GameObject paredAtras;
    public GameObject paredDelante;

    public GameObject collidersHabitacion;

    public List<GameObject> ParedTrasera = new List<GameObject>();

    // --- NUEVO: Listas de activación dinámica ---
    [Header("Objetos Dinámicos al Entrar/Salir")]
    public List<GameObject> objetosActivarAlEntrar = new List<GameObject>();
    public List<GameObject> objetosDesactivarAlEntrar = new List<GameObject>();
    
    private Coroutine rutinaDesactivacion; // Para poder cancelar el cronómetro si el jugador vuelve a entrar rápido

    public CameraController ControladorCamara;
    
    private bool enemigosGenerados = false; 

    void Start()
    {
        // Aseguramos que las paredes de cartón estén en su estado inicial (cerradas)
        if (paredCartonIzquierda != null) paredCartonIzquierda.Stop();
        if (paredCartonDerecha != null) paredCartonDerecha.Stop();
        if (paredCartonDelante != null) paredCartonDelante.Stop();
        if (gameObject.CompareTag("SalaIzquierda") && paredCartonDerecha != null)
        {
            paredCartonDerecha["AbrirDerecha"].wrapMode = WrapMode.ClampForever;
            paredCartonDerecha.Play("AbrirDerecha");
        }
        
        if (gameObject.CompareTag("SalaDerecha") && paredCartonIzquierda != null)
        {
            paredCartonIzquierda["AbrirIzquierda"].wrapMode = WrapMode.ClampForever;
            paredCartonIzquierda.Play("AbrirIzquierda");
        }
    }
    
    void Update()
    {
        Debug.DrawRay(detectorSalaCircundante.transform.position, detectorSalaCircundante.transform.right * 100f, Color.red);
        // Limpiamos las salas pacíficas (Tienda o Inicial) de forma automática
        if (jugadorDentro && !salaLimpia && !enemigosGenerados)
        {
            if (salaInicial || salaTienda || salaEspecial || pasillo)
            {
                salaLimpia = true; 
                terminarSala();
            }
        }

        // Comprobamos si hemos matado a todos los monstruos
        if (enemigosGenerados && !salaLimpia)
        {
            EnemigosSala.RemoveAll(enemigo => enemigo == null);

            if (EnemigosSala.Count == 0)
            {
                salaLimpia = true;
                terminarSala();
            }
        }
    }

    void SpawnearEnemigos()
    {
        enemigosGenerados = true; 
        foreach (SpawnerEnemigos spawner in spawnersDeLaSala)
        {
            spawner.SpawnearEnemigos(this);
        }
    }

    void terminarSala()
    {
        Debug.Log("¡La sala " + gameObject.name + " se ha limpiado! Abriendo puertas...");
        collidersHabitacion.SetActive(false);
        // ESCUDO: Comprobamos que no esté vacío antes de tocarlo
        if (paredCartonDelante != null)
        {
            paredCartonDelante["AbrirDelante"].speed = 1f; 
            paredCartonDelante["AbrirDelante"].time = 0f;  
            paredCartonDelante["AbrirDelante"].wrapMode = WrapMode.ClampForever;
            paredCartonDelante.Play("AbrirDelante");
        }

        if (gameObject.CompareTag("SalaIzquierda") && paredCartonDerecha != null)
        {
            paredCartonDerecha["AbrirDerecha"].time = 0f;
            paredCartonDerecha["AbrirDerecha"].speed = 1f;
            paredCartonDerecha["AbrirDerecha"].wrapMode = WrapMode.ClampForever;
            paredCartonDerecha.Play("AbrirDerecha");
        }
        
        if (gameObject.CompareTag("SalaDerecha") && paredCartonIzquierda != null)
        {
            paredCartonIzquierda["AbrirIzquierda"].time = 0f;
            paredCartonIzquierda["AbrirIzquierda"].speed = 1f;
            paredCartonIzquierda["AbrirIzquierda"].wrapMode = WrapMode.ClampForever;
            paredCartonIzquierda.Play("AbrirIzquierda");
        }
        
        if (salaNormal || salaInicial || salaTienda) 
        {
            if (detectorSalaCircundante != null)
            {
                if (Physics.Raycast(detectorSalaCircundante.transform.position, -detectorSalaCircundante.transform.right, out RaycastHit hit, 100f))
                {
                    if (paredCartonDerecha != null) 
                    {
                        paredCartonDerecha["AbrirDerecha"].speed = 1f;
                        paredCartonDerecha["AbrirDerecha"].wrapMode = WrapMode.ClampForever;
                        paredCartonDerecha.Play("AbrirDerecha");
                    }
                }
                
                if (Physics.Raycast(detectorSalaCircundante.transform.position, detectorSalaCircundante.transform.right, out RaycastHit hit2, 100f))
                {
                    if (paredCartonIzquierda != null) 
                    {
                        paredCartonIzquierda["AbrirIzquierda"].speed = 1f;
                        paredCartonIzquierda["AbrirIzquierda"].wrapMode = WrapMode.ClampForever;
                        paredCartonIzquierda.Play("AbrirIzquierda");
                    }
                }
            }
        }

        if (salaEspecial && paredCartonEspecial != null)
        {
            paredCartonEspecial["AbrirTelonEspecial"].speed = 1f;
            paredCartonEspecial["AbrirTelonEspecial"].wrapMode = WrapMode.ClampForever;
            paredCartonEspecial.Play("AbrirTelonEspecial");
        }
    }

    void CerrarParedes()
    {
        collidersHabitacion.SetActive(true);
        Debug.Log("¡Cerrando paredes de la sala " + gameObject.name + " para generar enemigos!");
        
        if (paredDelante != null) paredDelante.SetActive(true);
        
        foreach (GameObject obj in ParedTrasera)
        {
            if (obj != null) obj.SetActive(true);
        }
        
        if (gameObject.CompareTag("SalaIzquierda") && paredCartonDerecha != null)
        {
            paredCartonDerecha["AbrirDerecha"].speed = -1f;
            paredCartonDerecha["AbrirDerecha"].wrapMode = WrapMode.ClampForever;
            paredCartonDerecha["AbrirDerecha"].time = paredCartonDerecha["AbrirDerecha"].length; 
            paredCartonDerecha.Play("AbrirDerecha");
        }
        
        if (gameObject.CompareTag("SalaDerecha") && paredCartonIzquierda != null)
        {
            paredCartonIzquierda["AbrirIzquierda"].speed = -1f;
            paredCartonIzquierda["AbrirIzquierda"].wrapMode = WrapMode.ClampForever;
            paredCartonIzquierda["AbrirIzquierda"].time = paredCartonIzquierda["AbrirIzquierda"].length;
            paredCartonIzquierda.Play("AbrirIzquierda");
        }

        if (salaEspecial && paredCartonEspecial != null)
        {
            paredCartonEspecial["AbrirTelonEspecial"].speed = -1f;
            paredCartonEspecial["AbrirTelonEspecial"].wrapMode = WrapMode.ClampForever;
            paredCartonEspecial["AbrirTelonEspecial"].time = paredCartonEspecial["AbrirTelonEspecial"].length;
            paredCartonEspecial.Play("AbrirTelonEspecial");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (!salaLimpia && !enemigosGenerados)
            {
                CerrarParedes();
                SpawnearEnemigos();
            }
            
            if (!jugadorDentro)
            {
                paredDelante.SetActive(true);
                foreach (GameObject obj in ParedTrasera)
                {
                    if (obj != null) obj.SetActive(true);
                }

                // --- NUEVO: Gestión de Listas al Entrar ---
                
                // 1. Detenemos la corrutina de apagado si el jugador ha vuelto a entrar rápidamente
                if (rutinaDesactivacion != null)
                {
                    StopCoroutine(rutinaDesactivacion);
                    rutinaDesactivacion = null;
                }

                // 2. Activamos la lista de objetos que deben encenderse al entrar
                foreach (GameObject obj in objetosActivarAlEntrar)
                {
                    if (obj != null) obj.SetActive(true);
                }

                // 3. Desactivamos la lista de objetos que deben apagarse al entrar
                foreach (GameObject obj in objetosDesactivarAlEntrar)
                {
                    if (obj != null) obj.SetActive(false);
                }
                // -------------------------------------------

                int sala = 0;
                float anguloAñadido = 0f;
                if (pasillo) sala = 1;
                else if (salaEspecial)
                {
                    sala = 2;
                    anguloAñadido = 90f;
                }

                ControladorCamara.CambioDeReferencia(paredIzquierda, paredDerecha, paredDelante, paredAtras, transform.position, sala, anguloAñadido);
                jugadorDentro = true;
            }
        }
    }

    private void OnTriggerExit(Collider other) 
    {
        if (other.CompareTag("Player"))
        {
            if (!gameObject.CompareTag("SalaIzquierda") && !gameObject.CompareTag("SalaDerecha"))
            {
                foreach (GameObject obj in ParedTrasera)
                {
                    if (obj != null) obj.SetActive(false); 
                }
                paredDelante.SetActive(false);
            }
            
            // --- NUEVO: Gestión de Listas al Salir ---
            
            // 1. Reactivamos instantáneamente los objetos que desactivamos al entrar
            foreach (GameObject obj in objetosDesactivarAlEntrar)
            {
                if (obj != null) obj.SetActive(true);
            }

            // 2. Iniciamos el temporizador de 1.5 segundos para apagar los objetos de la sala
            rutinaDesactivacion = StartCoroutine(DesactivarObjetosConRetraso());
            // ------------------------------------------

            jugadorDentro = false;
        }
    }

    // --- NUEVA CORRUTINA: Retraso para apagar los objetos ---
    private IEnumerator DesactivarObjetosConRetraso()
    {
        yield return new WaitForSeconds(1.5f); // Esperamos 1.5 segundos

        // Apagamos los objetos de la lista
        foreach (GameObject obj in objetosActivarAlEntrar)
        {
            if (obj != null) obj.SetActive(false);
        }
        
        // Limpiamos la variable de la corrutina
        rutinaDesactivacion = null; 
    }
}