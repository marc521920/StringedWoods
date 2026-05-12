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
    
    private bool enemigosGenerados = false; 
    
    void Update()
    {
        if (jugadorDentro && !salaLimpia && !enemigosGenerados)
        {
            if (!salaInicial && !salaTienda)
            {
                SpawnearEnemigos();
            }
            else
            {
                // Si es tienda o inicial, la marcamos como limpia y ABRIMOS LAS PUERTAS
                salaLimpia = true; 
                terminarSala();
            }
        }

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

        // 1. Usamos 'if' separados para que TODAS las condiciones se puedan cumplir a la vez
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
                if (Physics.Raycast(detectorSalaCircundante.transform.position, detectorSalaCircundante.transform.right, out RaycastHit hit, 100f))
                {
                    if (paredCartonDerecha != null) 
                    {
                        paredCartonDerecha["AbrirDerecha"].wrapMode = WrapMode.ClampForever;
                        paredCartonDerecha.Play("AbrirDerecha");
                    }
                }
                
                if (Physics.Raycast(detectorSalaCircundante.transform.position, -detectorSalaCircundante.transform.right, out RaycastHit hit2, 100f))
                {
                    if (paredCartonIzquierda != null) 
                    {
                        paredCartonIzquierda["AbrirIzquierda"].wrapMode = WrapMode.ClampForever;
                        paredCartonIzquierda.Play("AbrirIzquierda");
                    }
                }
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorDentro = true;
        }
    }
}