using UnityEngine;

public class Collectables : MonoBehaviour
{
    public bool esVida; // Si es vida, cura al jugador. Si no, le da experiencia.
    public bool esExperiencia; // Si es experiencia, da experiencia al jugador. Si no, cura al jugador.
    public bool esMoneda;
    public int cantidad; // Cantidad de vida a curar o experiencia a dar
    Rigidbody rb; // Para aplicar una pequeña fuerza al caer
    Collider collider; // Para hacer que el objeto sea un trigger o no, dependiendo de si es vida/experiencia o moneda
    private float distanciaAlJugador; // Para calcular la distancia al jugador y hacer que el objeto flote hacia él si está cerca

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        rb = GetComponent<Rigidbody>();
        collider = GetComponent<Collider>();
        // Aplicamos una pequeña fuerza hacia arriba para que el objeto salte un poco al aparecer
        
        if (esExperiencia && esVida)
        {
            collider.isTrigger = true; // Si es ambos, lo hacemos un trigger para que el jugador pueda recogerlo sin colisionar
            rb.AddForce(Vector3.up * 5f, ForceMode.Impulse); 
        }
        else if (esMoneda)
        {
            collider.isTrigger = false;
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position.y < -10f) // Si el objeto cae por debajo de cierta altura, lo destruimos para evitar que se pierda en el infinito
        {
            Destroy(gameObject);
        }
        if (esVida || esExperiencia)
        {
            GameObject jugador = GameObject.FindWithTag("Player");
            if (jugador != null)            {
                distanciaAlJugador = Vector3.Distance(transform.position, jugador.transform.position);
                
            }
            // Hacemos que el objeto flote suavemente hacia arriba y hacia abajo para darle un efecto visual agradable
            float flotacion = Mathf.Sin(Time.time * 2f) * 0.5f; // Oscilación suave
            transform.position += new Vector3(0, flotacion * Time.deltaTime, 0);
            if (distanciaAlJugador < 3f) // Si el jugador está cerca, hacemos que el objeto se mueva suavemente hacia él para facilitar la recogida
            {
                Vector3 direccionAlJugador = (jugador.transform.position - transform.position).normalized;
                rb.AddForce(direccionAlJugador * Time.deltaTime * 2f, ForceMode.VelocityChange); // Velocidad de movimiento hacia el jugador
            }
        }
        
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (esVida)
            {
                GameManager.Instance.Curar(cantidad);
            }
            if (esExperiencia)
            {
                GameManager.Instance.GanarExperiencia(cantidad);
            }
            
            Destroy(gameObject); // Destruye el objeto después de recogerlo
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && esMoneda)
        {
            GameManager.Instance.GanarMonedas(cantidad);
            Destroy(gameObject); // Destruye el objeto después de recogerlo
        }
    }
}
