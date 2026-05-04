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
    public float vida;
    public Animator animator; // Referencia al Animator para controlar las animaciones
    protected bool recibiendoGolpe = false;


    public MainCharacterMovement PlayerScript; // Referencia al script del jugador para acceder a sus variables
    protected virtual void Start()
    {
        animator = GetComponent<Animator>();
        player = GameObject.FindWithTag("Player"); // Asegúrate de que el jugador tenga el tag "Player"
        if (player != null)
        {
            PlayerScript = player.GetComponent<MainCharacterMovement>(); // Obtenemos el script del jugador para acceder a sus variables
        }
        meshRenderer = GetComponent<Renderer>();
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = false; // El enemigo no se moverá por la física hasta que sea golpeado
         // Desactivamos la gravedad para que no caiga mientras patrulla o persigue
        
       
        colorOriginal = meshRenderer.material.color;
    }
    protected virtual void Update()
    {
        Moverse();
        // Aquí podrías agregar lógica para el comportamiento del enemigo, como perseguir al jugador, atacar, etc.
        if (vida <= 0)
        {
            Destroy(gameObject); // Destruye el enemigo si su vida llega a 0 o menos
        }
    }
    
    // Seguro anti-multigolpe
  

    protected virtual void OnTriggerEnter(Collider other)
    {
         if (other.CompareTag("Ataque") && !recibiendoGolpe) 
         {
                recibiendoGolpe = true; 
                
                // 1. Matamos cualquier corrutina de patrulla/giro que el enemigo estuviera haciendo
                StopAllCoroutines(); 
                
                animator.SetBool("isWalking", false); 
                
                
                // 2. Apagamos la física para evitar el teletransporte por superposición de colliders
                rb.isKinematic = true; 
                rb.linearVelocity = Vector3.zero; 

                Debug.Log("¡Enemigo golpeado! Calculando desde el arma...");
                RecibirDaño();

                // 3. Calculamos la dirección estrictamente desde el centro del arma
                Vector3 direccionEmpuje = transform.position - other.transform.position;
                direccionEmpuje.y = 0; // Para que no se hunda en el suelo
                
                // Si justo el centro del arma y el del enemigo son el mismo punto (raro pero posible)
                if (direccionEmpuje == Vector3.zero) 
                {
                    direccionEmpuje = -transform.forward;
                }
                else 
                {
                    direccionEmpuje = direccionEmpuje.normalized;
                }
                
                meshRenderer.material.color = colorDaño;

                // 4. Arrancamos el empujón manual
                StartCoroutine(RutinaKnockbackEnemigo(direccionEmpuje));
         }
    }

    // --- RUTINA MANUAL EXTREMA ---
    IEnumerator RutinaKnockbackEnemigo(Vector3 direccion)
    {
        float duracionEmpuje = 0.2f; 
        float tiempoPasado = 0f;
        float velocidadEmpuje = PlayerScript.fuerzaGolpe * 2f; 

        while (tiempoPasado < duracionEmpuje)
        {
            tiempoPasado += Time.deltaTime;
            
            // Usamos transform.position directamente. Esto anula CUALQUIER bug físico.
            // El enemigo resbalará exactamente en la dirección que le decimos sin pestañear.
            transform.position += direccion * velocidadEmpuje * Time.deltaTime;
            
            yield return null;
        }

        RestaurarColor();
    }

    void RestaurarColor()
    {
        
        meshRenderer.material.color = colorOriginal;
        
        // Devolvemos el control físico para la gravedad
        rb.isKinematic = false; 
        
        // Quitamos el candado de los golpes
        recibiendoGolpe = false; 
    }
    protected virtual void Moverse()
    {
        // Aquí puedes implementar la lógica para reducir la salud del enemigo, reproducir animaciones de daño, etc.
    }
    protected virtual void RecibirDaño()
    {
        // Aquí puedes implementar la lógica para reducir la salud del enemigo, reproducir animaciones de daño, etc.
    }

}
