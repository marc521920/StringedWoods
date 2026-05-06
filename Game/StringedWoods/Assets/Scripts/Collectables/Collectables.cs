using UnityEngine;

public class Collectables : MonoBehaviour
{
    public bool esVida; // Si es vida, cura al jugador. Si no, le da experiencia.
    public bool esExperiencia; // Si es experiencia, da experiencia al jugador. Si no, cura al jugador.
    public bool esMoneda;
    public int cantidad; // Cantidad de vida a curar o experiencia a dar
    rigidbody rb; // Para aplicar una pequeña fuerza al caer
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // Aplicamos una pequeña fuerza hacia arriba para que el objeto salte un poco al aparecer
        
        if (esExperiencia && esVida)
        {
            rb.isTrigger = true; // Si es ambos, lo hacemos un trigger para que el jugador pueda recogerlo sin colisionar
            rb.AddForce(Vector3.up * 5f, ForceMode.Impulse); 
        }
        else if (esMoneda)
        {
            rb.isTrigger = false;
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (esVida)
            {
                GameManager.Instance.PlayerScript.Curar(cantidad);
            }
            if (esExperiencia)
            {
                GameManager.Instance.PlayerScript.GanarExperiencia(cantidad);
            }
            
            Destroy(gameObject); // Destruye el objeto después de recogerlo
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && esMoneda)
        {
            GameManager.Instance.PlayerScript.GanarMonedas(cantidad);
            Destroy(gameObject); // Destruye el objeto después de recogerlo
        }
    }
}
