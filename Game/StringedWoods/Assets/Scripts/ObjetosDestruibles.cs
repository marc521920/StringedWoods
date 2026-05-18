using UnityEngine;
using System.Collections;

public class ObjetoDestruible : MonoBehaviour
{
    [Header("Modelos (Hijos de este objeto)")]
    public GameObject modeloEntero;
    public GameObject modeloRoto;

    [Header("Ajustes de Limpieza")]
    public float tiempoAntesDeDesaparecer = 2f; // Tiempo que los pedazos se quedan en el suelo
    public float velocidadDeDesaparicion = 1.5f; // Cuánto tarda en encogerse y desaparecer

    private bool yaDestruido = false;

    void Start()
    {
        // Por seguridad, nos aseguramos de que empiece en el estado correcto
        if (modeloEntero != null) modeloEntero.SetActive(true);
        if (modeloRoto != null) modeloRoto.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if ((other.CompareTag("Ataque") || other.CompareTag("Enemy") ) && !yaDestruido) 
        {
            Debug.Log("HOLI");
            Romper();
        }
    }

    private void Romper()
    {
        yaDestruido = true;

        // 1. Apagamos el collider principal para que el jugador no choque con un jarrón invisible
        Collider miCollider = GetComponent<Collider>();
        if (miCollider != null) miCollider.enabled = false;

        // 2. Cambiamos los modelos (El cambiazazo)
        if (modeloEntero != null) modeloEntero.SetActive(false);
        if (modeloRoto != null) modeloRoto.SetActive(true);

        // 3. Iniciamos el proceso para limpiar la basura poco a poco
        StartCoroutine(DesaparecerPocoAPoco());
    }

    IEnumerator DesaparecerPocoAPoco()
    {
        // Damos tiempo a que las físicas hagan caer los pedazos al suelo
        yield return new WaitForSeconds(tiempoAntesDeDesaparecer);

        // Si tenemos el modelo roto asignado, lo encogemos suavemente a 0
        if (modeloRoto != null)
        {
            Vector3 escalaInicial = modeloRoto.transform.localScale;
            float tiempo = 0f;

            while (tiempo < velocidadDeDesaparicion)
            {
                tiempo += Time.deltaTime;
                float porcentaje = tiempo / velocidadDeDesaparicion;
                
                // Hacemos el modelo roto cada vez más pequeño
                modeloRoto.transform.localScale = Vector3.Lerp(escalaInicial, Vector3.zero, porcentaje);
                
                yield return null;
            }
        }

        // Una vez que es invisible (escala 0), destruimos el objeto padre por completo
        Destroy(gameObject);
    }
}