using UnityEngine;
using System.Collections;

public class MainCharacterMovement : MonoBehaviour
{
    public float speed = 6.0f;
    public float jumpSpeed = 18.0f;
    public float gravity = 20.0f;
    public float rotationSpeed = 15.0f;
    public float derrape = 10f;
    private float anguloDeGiro = 0f;

    //salto cargado
    private bool salto = true;
    private bool soltadoBotonSalto = true;

    // Dash
    [Header("Dash Settings")]
    public float velocidadDash = 30f;
    public float tiempoDash = 0.2f; // Cuánto dura el impulso en segundos
    private bool isDashing = false; // Para saber si estamos en medio de un dash
    private bool dashInCooldown = false; // Para evitar que el jugador pueda dashar de nuevo inmediatamente
    
    // Usaremos esta variable solo para la caída y el salto
    public float velocidadY = 0.0f; 

    private CharacterController controller;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update() 
    {
       if (isDashing) return;
        Debug.Log(velocidadY);
        // Calculamos el movimiento horizontal SIEMPRE (para poder movernos en el aire)
        Vector3 moveDirection = new Vector3(-Input.GetAxis("Horizontal"), 0, -Input.GetAxis("Vertical"));
        
        // Calculamos la rotación
        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            //float anguloDeGiro = Quaternion.Angle(transform.rotation, targetRotation);
            

        
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            
        }
       

        // Aplicamos la velocidad al movimiento horizontal
        moveDirection *= speed;

        // (Gravedad y Salto)
        if (controller.isGrounded) 
        {
            salto = false; // Reseteamos el estado de salto al estar en el suelo
           
            // Le damos un valor ligeramente negativo para que el personaje se mantenga pegado al suelo
            if (velocidadY < 0) 
            {
                velocidadY = -2f; 
            }

            // Salto
            if (Input.GetKey(KeyCode.Space) && salto == false) {
                if (soltadoBotonSalto){
                    salto = true;
                
                    velocidadY = jumpSpeed; // La velocidad de salto se multiplica por el valor cargado

                    soltadoBotonSalto = false; // Se marca que el botón de salto está siendo presionado

                }                
            } else if (!Input.GetKeyUp(KeyCode.Space)) {
                soltadoBotonSalto = true; // Se marca que el botón de salto ha sido soltado

                
            }
            
        }
        else 
        {
            // Si NO estamos en el suelo (estamos en el aire), aplicamos la gravedad poco a poco
            velocidadY -= gravity * Time.deltaTime;

           if (!Input.GetKey(KeyCode.Space) && velocidadY > 0 && salto == true) 
            {
                salto = false;
                // Esto hace que pierda fuerza pero siga subiendo un poquito por inercia
                velocidadY *= 0.5f; 
            }

        }
        
        if (Input.GetKeyDown(KeyCode.Q) && !dashInCooldown) 
        {
            StartCoroutine(Dash()); // Iniciamos el dash
        }
        // Unimos el movimiento vertical (Y) con el horizontal (X, Z)
        moveDirection.y = velocidadY;
        
        // Movemos al personaje
        controller.Move(moveDirection * Time.deltaTime);
    }
    IEnumerator Dash()
    {
         
    //  Empezamos el dash
        isDashing = true;

    // la velocidad Y a 0 para que no caiga mientras dashea en el aire
        velocidadY = 0f; 

        float startTime = Time.time; // Guardamos el momento exacto en el que empieza

    // Mientras no haya pasado el tiempoDash, nos movemos
        while (Time.time < startTime + tiempoDash)
        {
        // Movemos al personaje hacia donde mira
            controller.Move(transform.forward * velocidadDash * Time.deltaTime);
        
        // Esperamos al siguiente frame para continuar el bucle
        yield return null; 
        }

        // 3. Terminamos el dash y devolvemos el control al jugador
        isDashing = false;
        dashInCooldown = true; // Activamos el cooldown para evitar dashes consecutivos
    StartCoroutine(CooldownDash()); // Iniciamos la rutina de cooldown
    }
    IEnumerator CooldownDash()
    {
      yield return new WaitForSeconds(1f); // Esperamos 2 segundos antes de permitir otro dash
      dashInCooldown = false; // Desactivamos el cooldown
    }
    void Atack()
    {

        // Aquí iría la lógica de ataque, por ejemplo, detectar enemigos cercanos y aplicar daño
    }
}