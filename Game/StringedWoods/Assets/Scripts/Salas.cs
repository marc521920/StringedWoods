using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Salas : MonoBehaviour
{
    GameObject CartonIzquierda;
    GameObject CartonDerecha;
    GameObject CartonDelante;
    private List<GameObject> EnemigosSala = new List<GameObject>();
    public bool salaBoss;
    public bool salaEspecial;
    public bool salaNormal;
    public bool salaInicial;
    public bool salaTienda;

    public bool salaLimpia; // Para saber si la sala ya fue limpiada de enemigos, y así no volver a spawnear enemigos al entrar
    public bool jugadorDentro; // Para saber si el jugador está dentro de la sala, y
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (jugadorDentro && !salaLimpia)
        {
            // Aquí podrías agregar lógica para spawnear enemigos si la sala no es de tipo "salaInicial" o "salaTienda"
            if (!salaInicial && !salaTienda)
            {
                SpawnearEnemigos();
            }
        }
        
    }
    void SpawnearEnemigos()
    {
        // Aquí podrías agregar la lógica para spawnear enemigos dependiendo del tipo de sala (salaBoss, salaEspecial, salaNormal)
        // Por ejemplo, podrías usar Instantiate para crear enemigos en posiciones específicas dentro de la sala
        // Y luego agregar esos enemigos a la lista EnemigosSala para poder controlarlos posteriormente (como verificar si están vivos o no)
    }
}
