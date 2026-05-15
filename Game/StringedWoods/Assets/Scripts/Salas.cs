using UnityEngine;
using System.Collections.Generic;

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

    public List<GameObject> ParedTrasera = new List<GameObject>();

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
        paredCartonDelante["AbrirDelante"].speed = 1f; // <--- Aseguramos velocidad
                paredCartonDelante["AbrirDelante"].time = 0f;  // <--- Aseguramos que empiece de cero
                paredCartonDelante["AbrirDelante"].wrapMode = WrapMode.ClampForever;
                paredCartonDelante.Play("AbrirDelante");

        // 1. Usamos 'if' separados para que TODAS las condiciones se puedan cumplir a la vez
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
        
        // 2. Si es una sala normal (o inicial/tienda), revisamos las puertas principales
        if (salaNormal || salaInicial || salaTienda) 
        {
            if (paredCartonDelante != null)
            {
                paredCartonDelante["AbrirDelante"].wrapMode = WrapMode.ClampForever;
                paredCartonDelante.Play("AbrirDelante");
            }

            // 3. Comprobamos los raycasts (¡Ojo! Asegurándonos de que el detector exista para evitar errores)
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
        if (salaEspecial || pasillo)
        {
            paredCartonEspecial["AbrirTelonEspecial"].speed = 1f;
            paredCartonEspecial["AbrirTelonEspecial"].wrapMode = WrapMode.ClampForever;
            paredCartonEspecial.Play("AbrirTelonEspecial");
            
        }
    }
    void CerrarParedes()
    {
        Debug.Log("¡Cerrando paredes de la sala " + gameObject.name + " para generar enemigos!");
        paredDelante.SetActive(true);
        
        foreach (GameObject obj in ParedTrasera)
        {
            if (obj != null)
            {
                obj.SetActive(true);
            }
        }

        if (gameObject.CompareTag("SalaIzquierda") && paredCartonDerecha != null)
        {
            paredCartonDerecha["AbrirDerecha"].speed = -1f;
            paredCartonDerecha["AbrirDerecha"].wrapMode = WrapMode.ClampForever;
            
            // --- ESTA ES LA LÍNEA QUE TE FALTA Y QUE HACE LA MAGIA ---
            paredCartonDerecha["AbrirDerecha"].time = paredCartonDerecha["AbrirDerecha"].length; 
            
            paredCartonDerecha.Play("AbrirDerecha");
        }
        
        if (gameObject.CompareTag("SalaDerecha") && paredCartonIzquierda != null)
        {
            paredCartonIzquierda["AbrirIzquierda"].speed = -1f;
            paredCartonIzquierda["AbrirIzquierda"].wrapMode = WrapMode.ClampForever;
            
            // --- ESTA ES LA LÍNEA QUE TE FALTA Y QUE HACE LA MAGIA ---
            paredCartonIzquierda["AbrirIzquierda"].time = paredCartonIzquierda["AbrirIzquierda"].length;
            
            paredCartonIzquierda.Play("AbrirIzquierda");
        }
        if (salaEspecial )
        {
            paredCartonEspecial["AbrirTelonEspecial"].speed = -1f;
            paredCartonEspecial["AbrirTelonEspecial"].wrapMode = WrapMode.ClampForever;
            paredCartonEspecial.Play("AbrirTelonEspecial");
            
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") )
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
            if (obj != null)
            {
                obj.SetActive(true);
            }
        }
        int sala = 0;
        float anguloAñadido = 0f;
        if (pasillo)
        {
            sala = 1;
        }else if (salaEspecial)
        {
            sala = 2;
            anguloAñadido = 90f;
        }

            ControladorCamara.CambioDeReferencia(paredIzquierda,paredDerecha,paredDelante,paredAtras,transform.position,sala,anguloAñadido);
            jugadorDentro = true;

        }
            
        }
        
    }
    private void OnTriggerExit(Collider other) 
    {
        if (other.CompareTag("Player") )
        {
            if (!gameObject.CompareTag("SalaIzquierda") && !gameObject.CompareTag("SalaDerecha"))
            {
                foreach (GameObject obj in ParedTrasera)
                {
                    // Esta comprobación evita errores si alguno de los objetos ya fue destruido antes
                    if (obj != null)
                    {

                obj.SetActive(false); 
                }
                }
                paredDelante.SetActive(false);
            }
            
            jugadorDentro = false;
            
        }
        
    }
}