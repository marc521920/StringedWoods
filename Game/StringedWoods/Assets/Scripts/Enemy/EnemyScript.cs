using UnityEngine;
using System.Collections;
public class EnemyScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Color colorDaño = Color.red; 
    
    protected Color colorOriginal;
    protected Renderer meshRenderer;
    protected Rigidbody rb;
    public GameObject player; // Referencia al jugador para aplicar la fuerza en la dirección correcta
    


    public MainCharacterMovement PlayerScript; // Referencia al script del jugador para acceder a sus variables
    void Start()
    {
        player = GameObject.FindWithTag("Player"); // Asegúrate de que el jugador tenga el tag "Player"
        if (player != null)
        {
            PlayerScript = player.GetComponent<MainCharacterMovement>(); // Obtenemos el script del jugador para acceder a sus variables
        }
        meshRenderer = GetComponent<Renderer>();
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true; // El enemigo no se moverá por la física hasta que sea golpeado
        
       
        colorOriginal = meshRenderer.material.color;
    }
    void Update()
    {
        Moverse();
        // Aquí podrías agregar lógica para el comportamiento del enemigo, como perseguir al jugador, atacar, etc.
    }
    
    void OnTriggerEnter(Collider other)
    {
        
         if (other.CompareTag("Ataque")) 
        
            {
                rb.isKinematic = false; // Permite que el enemigo sea afectado por la física
                Debug.Log("¡Enemigo golpeado!");
                //RecibirDaño();
                Vector3 direccionHaciaAtacante = other.transform.position - transform.position;
                direccionHaciaAtacante = direccionHaciaAtacante.normalized;
                rb.AddForce(-direccionHaciaAtacante * PlayerScript.fuerzaGolpe, ForceMode.Impulse); // Aplica una fuerza hacia atrás al enemigo
            }
        meshRenderer.material.color = colorDaño;
            
        // Extra: Llamamos a la función de restaurar color después de medio segundo (0.5f)
        Invoke("RestaurarColor", 0.5f);
    }

    void RestaurarColor()
    {
        // Volvemos a ponerle el color normal a la cápsula
        meshRenderer.material.color = colorOriginal;
        rb.isKinematic = true; // Volvemos a hacer que el enemigo no se mueva por la física después de ser golpeado
        
    }
    protected virtual void Moverse()
    {
        // Aquí puedes implementar la lógica para reducir la salud del enemigo, reproducir animaciones de daño, etc.
    }

}
