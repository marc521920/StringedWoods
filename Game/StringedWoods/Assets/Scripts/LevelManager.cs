using UnityEngine;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    private int numeroDeSalasNormales;
    public int numeroDeSalasNormalesmMin;
    public int numeroDeSalasNormalesmMax;

    public int probabilidadSalasEspeciales;
    public int probabilidadSalasNormales; 

    private bool salaEspecialgenerada;

    public float espacioEntreSalasZ;
    public float espacioEntreSalasX;
    public Vector3 posicionSala;
    public Vector3 posicionTienda;

    [Header("GameObjects Mapas")]
    // --- NUEVO: Prefab de la Sala Inicial ---
    public GameObject salaInicialPrefab; 
    public GameObject[] listaDeMapas;
    public List<GameObject> listaDeMapasEspecialesOriginales;
    public List<GameObject> listaDeMapasEspecialesDisponibles;
    public GameObject[] listaDeTiendas;

    void Start()
    {
        listaDeMapasEspecialesDisponibles = new List<GameObject>(listaDeMapasEspecialesOriginales);
        numeroDeSalasNormales = Random.Range(numeroDeSalasNormalesmMin, numeroDeSalasNormalesmMax);
        posicionSala = transform.position;
        GenerarMapa();
    }

    void Update()
    {
        
    }

    void GenerarMapa()
    {
        int valorPosicionTienda = Random.Range(1, listaDeMapas.Length + 1);
        int valorLineaTienda = Random.Range(1, 3);
        posicionTienda = transform.position;
        posicionTienda.z = posicionTienda.z + (valorPosicionTienda * espacioEntreSalasZ);
        
        if (valorLineaTienda == 1)
        {
            posicionTienda.x = posicionTienda.x - espacioEntreSalasX;
        }
        else if (valorLineaTienda == 2)
        {
            posicionTienda.x = posicionTienda.x + espacioEntreSalasX;
        }

        // ========================================================
        // FILA CENTRAL: Aquí forzamos la primera sala (i == 0)
        // ========================================================
        for (int i = 0; i < numeroDeSalasNormales; i++)
        {
            GameObject nuevoMapa;

            // Si es la primera sala de todas, usamos el prefab de la sala inicial
            if (i == 0 && salaInicialPrefab != null)
            {
                nuevoMapa = Instantiate(salaInicialPrefab, posicionSala, Quaternion.identity);
                nuevoMapa.name = "Sala Inicial";
            }
            else // Para el resto de salas de la fila central, elige una aleatoria normal
            {
                int indiceAleatorio = Random.Range(0, listaDeMapas.Length);
                nuevoMapa = Instantiate(listaDeMapas[indiceAleatorio], posicionSala , Quaternion.identity);
                nuevoMapa.name = "Level, " + i;
            }

            posicionSala.z = posicionSala.z + espacioEntreSalasZ;
        }

        // ========================================================
        // FILA DERECHA
        // ========================================================
        posicionSala = transform.position;
        posicionSala.x = posicionSala.x + espacioEntreSalasX;

        for (int i = 0; i < numeroDeSalasNormales; i++)
        {
            if (posicionSala == posicionTienda)
            {
                int indiceAleatorioTiendas = Random.Range(0, listaDeTiendas.Length);
                GameObject nuevaTienda = Instantiate(listaDeTiendas[indiceAleatorioTiendas], posicionTienda, Quaternion.identity);
                nuevaTienda.name = "Tienda";
            }
            else
            {
                int valorAleatorio = Random.Range(0, 100);
                if (valorAleatorio < probabilidadSalasEspeciales  && listaDeMapasEspecialesDisponibles.Count > 0)
                {
                    if (salaEspecialgenerada == false)
                    {
                        int indiceAleatorioEspecial = Random.Range(0, listaDeMapasEspecialesDisponibles.Count);
                        GameObject nuevoMapaEspecial = Instantiate(listaDeMapasEspecialesDisponibles[indiceAleatorioEspecial], posicionSala, Quaternion.identity);
                        nuevoMapaEspecial.name = "Level, " + (i + numeroDeSalasNormales) + " Especial";
                        listaDeMapasEspecialesDisponibles.RemoveAt(indiceAleatorioEspecial);
                        salaEspecialgenerada = true;
                    }
                    else                    
                    {
                        salaEspecialgenerada = false;
                    }
                }
                else if ((probabilidadSalasNormales + probabilidadSalasEspeciales) > valorAleatorio )
                {
                    Debug.Log(valorAleatorio);
                    int indiceAleatorio = Random.Range(0, listaDeMapas.Length);
                    GameObject nuevoMapa = Instantiate(listaDeMapas[indiceAleatorio], posicionSala , Quaternion.identity);
                    nuevoMapa.name = "Level," + (i + numeroDeSalasNormales);
                    nuevoMapa.tag = "SalaDerecha";
                    salaEspecialgenerada = false;
                }
            }

            posicionSala.z = posicionSala.z + espacioEntreSalasZ;
        }

        // ========================================================
        // FILA IZQUIERDA
        // ========================================================
        posicionSala = transform.position;
        posicionSala.x = posicionSala.x - espacioEntreSalasX;

        for (int i = 0; i < numeroDeSalasNormales; i++)
        {
            if (posicionSala == posicionTienda)
            {
                int indiceAleatorioTiendas = Random.Range(0, listaDeTiendas.Length);
                GameObject nuevaTienda = Instantiate(listaDeTiendas[indiceAleatorioTiendas], posicionTienda, Quaternion.identity);
                nuevaTienda.name = "Tienda";
            }
            else
            {
                float valorAleatorio = Random.Range(0f, 100f);
                if (valorAleatorio < probabilidadSalasEspeciales && listaDeMapasEspecialesDisponibles.Count > 0 )
                {
                    if (salaEspecialgenerada == false)
                    {
                        int indiceAleatorioEspecial = Random.Range(0, listaDeMapasEspecialesDisponibles.Count);
                        GameObject nuevoMapaEspecial = Instantiate(listaDeMapasEspecialesDisponibles[indiceAleatorioEspecial], posicionSala, Quaternion.identity);
                        nuevoMapaEspecial.name = "Level, " + (i + (numeroDeSalasNormales * 2)) + " Especial";
                        listaDeMapasEspecialesDisponibles.RemoveAt(indiceAleatorioEspecial);
                        salaEspecialgenerada = true;
                    }
                    else
                    {
                        salaEspecialgenerada = false;
                    }
                }
                else if ((probabilidadSalasNormales + probabilidadSalasEspeciales) > valorAleatorio )
                {
                    int indiceAleatorio = Random.Range(0, listaDeMapas.Length);
                    GameObject nuevoMapa = Instantiate(listaDeMapas[indiceAleatorio], posicionSala , Quaternion.identity);
                    nuevoMapa.name = "Level," + (i + (numeroDeSalasNormales * 2));
                    nuevoMapa.tag = "SalaIzquierda";
                    salaEspecialgenerada = false;
                }
            }

            posicionSala.z = posicionSala.z + espacioEntreSalasZ;
        }
    }
}