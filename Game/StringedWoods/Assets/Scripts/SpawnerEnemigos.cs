using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class EnemigoProbabilidad
{
    public GameObject prefabEnemigo;
    
    [Tooltip("Cuanto más alto sea este número, más posibilidades tiene de aparecer.")]
    public float probabilidad; 
}

public class SpawnerEnemigos : MonoBehaviour
{
    [Header("Configuración de Spawn")]
    public List<EnemigoProbabilidad> listaEnemigos = new List<EnemigoProbabilidad>();

    public void SpawnearEnemigos(Salas sala)
    {
        if (listaEnemigos.Count == 0) return;

        GameObject enemigoElegido = ElegirEnemigoAleatorio();

        if (enemigoElegido != null)
        {
            // 1. Instanciamos el enemigo
            GameObject nuevoEnemigo = Instantiate(enemigoElegido, transform.position, Quaternion.identity);

            // 2. ¡LA CONEXIÓN! Metemos al recién nacido en la lista de la sala
            sala.EnemigosSala.Add(nuevoEnemigo);
        }
    }

    // --- LA RULETA DE PROBABILIDADES ---
    private GameObject ElegirEnemigoAleatorio()
    {
        float sumaTotalProbabilidades = 0f;

        // Sumamos todas las probabilidades (ej: 50 + 30 + 20 = 100)
        foreach (EnemigoProbabilidad enemigo in listaEnemigos)
        {
            sumaTotalProbabilidades += enemigo.probabilidad;
        }

        // Tiramos el dado virtual
        float dadoAleatorio = Random.Range(0f, sumaTotalProbabilidades);
        float probabilidadAcumulada = 0f;

        // Comprobamos quién se lleva el premio
        foreach (EnemigoProbabilidad enemigo in listaEnemigos)
        {
            probabilidadAcumulada += enemigo.probabilidad;
            
            if (dadoAleatorio <= probabilidadAcumulada)
            {
                return enemigo.prefabEnemigo; // Devolvemos al ganador
            }
        }

        return null; 
    }
}