using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Salas : MonoBehaviour
{
    // ... tus otras variables (CartonIzquierda, etc) ...

    [Header("Listas de la Sala")]
    // ¡IMPORTANTE! La ponemos public para que el Spawner pueda verla y meter al enemigo
    public List<GameObject> EnemigosSala = new List<GameObject>(); 
    
    // Aquí arrastrarás desde el Inspector todos los spawners que hayas puesto en esta sala
    public List<SpawnerEnemigos> spawnersDeLaSala = new List<SpawnerEnemigos>(); 

    public bool salaBoss;
    public bool salaEspecial;
    public bool salaNormal;
    public bool salaInicial;
    public bool salaTienda;

    public bool salaLimpia; 
    public bool jugadorDentro; 

    public GameObject detectorSalaCircundante;

    [Header("paredes de carton")]
    public Animation paredCartonIzquierda;
    public Animation paredCartonDerecha;
    public Animation paredCartonDelante;
    
    // NUEVO: Un "cerrojo" para no spawnear 60 veces por segundo en el Update
    private bool enemigosGenerados = false; 
    
    void Update()
    {
        // Si el jugador entra, la sala no está limpia, y AÚN NO hemos generado enemigos...
        if (jugadorDentro && !salaLimpia && !enemigosGenerados)
        {
            if (!salaInicial && !salaTienda)
            {
                SpawnearEnemigos();
            }
            else
            {
                // Si es tienda o inicial, la marcamos como limpia directamente
                salaLimpia = true; 
            }
            
        }
        if (enemigosGenerados && !salaLimpia)
            {
            // 1. EL BARRENDERO: Esta línea mágica busca cualquier enemigo destruido y lo borra de la lista
            EnemigosSala.RemoveAll(enemigo => enemigo == null);

            // 2. Comprobamos si, tras limpiar a los muertos, la lista se ha quedado vacía
            if (EnemigosSala.Count == 0)
            {
                salaLimpia = true;
                terminarSala();

                // Aquí pondrías el código para abrir las puertas o soltar el cofre de recompensa
            }
        }
    }

    void SpawnearEnemigos()
    {
        // 1. Echamos el cerrojo inmediatamente para que el Update no vuelva a entrar aquí
        enemigosGenerados = true; 

        // 2. Recorremos todos los spawners que asignaste a esta sala
        foreach (SpawnerEnemigos spawner in spawnersDeLaSala)
        {
            // 3. Le decimos al spawner que actúe. 
            // La palabra "this" significa "yo mismo" (le pasamos este script Salas al spawner)
            spawner.SpawnearEnemigos(this);
        }
    }
    void terminarSala()
    {
        if (tag == "SalaIzquierda")
        {
            
            paredCartonDerecha.Play("AbrirDerecha");
        }
        else if (tag == "SalaDerecha")
        {
            paredCartonIzquierda.Play("AbrirIzquierda");
        }
        else if (salaNormal)
        {
            paredCartonDelante.Play("AbrirDelante");
            if (Physics.Raycast(detectorSalaCircundante.transform.position, detectorSalaCircundante.transform.right, out RaycastHit hit, 100f))
            {
                paredCartonDerecha.Play("AbrirDerecha");
            }
            if (Physics.Raycast(detectorSalaCircundante.transform.position, -detectorSalaCircundante.transform.right, out RaycastHit hit2, 100f))
            {
                paredCartonIzquierda.Play("AbrirIzquierda");
            }
        }

        // Aquí pondrías el código para abrir las puertas o soltar el cofre de recompensa
    }
}