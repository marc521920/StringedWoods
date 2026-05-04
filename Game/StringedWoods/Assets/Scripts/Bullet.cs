using UnityEngine;

public class Bullet : MonoBehaviour
{
     private Rigidbody rb;
     public float velocidadBala;

     public GameObject player;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        rb = GetComponent<Rigidbody>();
        
    }

    // Update is called once per frame
    void Update()
    {
         rb.linearVelocity = Vector3.forward * velocidadBala;
         
         
            
         
        
    }
    private void OnCollisionEnter(Collision other) 
    {
        velocidadBala = 0;
        
        // ¡Aquí está la clave! Hay que añadir .gameObject
        if (other.gameObject.CompareTag("Player")) 
        {
            Destroy(gameObject);
        }
    }
}
