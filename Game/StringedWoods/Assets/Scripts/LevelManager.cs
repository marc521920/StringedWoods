using UnityEngine;

public class LevelManager : MonoBehaviour
{
    private int numeroDeSalasNormales;
    public int numeroDeSalasNormalesmMin;
    public int numeroDeSalasNormalesmMax;

    public int porbabilidadSalasEspeciales;

    public float espacioEntreSalas;
    public float posicionSala;

    //GameObjects Mapas
    public GameObject[] listaDeMapas;
    public GameObject[] listaDeMapasEspeciales;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        numeroDeSalasNormales = Random.Range(numeroDeSalasNormalesmMin,numeroDeSalasNormalesmMax);
        posicionSala = transform.position.z;
        GenerarMapa();
        
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(listaDeMapas.Length);
    }
    void GenerarMapa()
    {
        for (int i = 0; i < numeroDeSalasNormales; i++)
        {
            int indiceAleatorio = Random.Range(0, listaDeMapas.Length);
            // 1. Instanciamos y guardamos la copia en la variable "nuevoEnemigo"
            GameObject nuevoMapa = Instantiate(listaDeMapas[indiceAleatorio], new Vector3 (transform.position.x,transform.position.y,posicionSala) , Quaternion.identity);

            nuevoMapa.name = "Level 1";
            posicionSala = posicionSala + espacioEntreSalas;
    
            
        }
        

    }
}
