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

    //GameObjects Mapas
    public GameObject[] listaDeMapas;
    public List<GameObject> listaDeMapasEspecialesOriginales;
    public List<GameObject> listaDeMapasEspecialesDisponibles;
    public GameObject[] listaDeTiendas;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        listaDeMapasEspecialesDisponibles = new List<GameObject>(listaDeMapasEspecialesOriginales);
        numeroDeSalasNormales = Random.Range(numeroDeSalasNormalesmMin,numeroDeSalasNormalesmMax);
        posicionSala = transform.position;
        GenerarMapa();
        
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(listaDeMapas.Length);
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
        for (int i = 0; i < numeroDeSalasNormales; i++)
        {
            int indiceAleatorio = Random.Range(0, listaDeMapas.Length);
            // 1. Instanciamos y guardamos la copia en la variable "nuevoEnemigo"
            GameObject nuevoMapa = Instantiate(listaDeMapas[indiceAleatorio], posicionSala , Quaternion.identity);

            nuevoMapa.name = "Level, " + i;
            posicionSala.z = posicionSala.z + espacioEntreSalasZ;
        }
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
                    // 1. Instanciamos y guardamos la copia en la variable "nuevoEnemigo"
                    GameObject nuevoMapa = Instantiate(listaDeMapas[indiceAleatorio], posicionSala , Quaternion.identity);
                    nuevoMapa.name = "Level," + (i + numeroDeSalasNormales);
                    nuevoMapa.tag = "SalaDerecha";
                    salaEspecialgenerada = false;
                }
            }

            
            posicionSala.z = posicionSala.z + espacioEntreSalasZ;
        }
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
                    // 1. Instanciamos y guardamos la copia en la variable "nuevoEnemigo"
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
