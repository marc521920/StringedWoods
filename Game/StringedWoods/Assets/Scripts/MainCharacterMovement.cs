using UnityEngine;

public class MainCharacterMovement : MonoBehaviour
{
    public float speed = 6.0f;
    public float jumpSpeed = 18.0f;
    public float gravity = 20.0f;
    public float rotationSpeed = 15.0f;

    //salto cargado
    float CargaSalto = 1f;
    float velocidadCarga = 20f; 
    private bool salto = true;
    private bool soltadoBotonSalto = true;
    
    // Usaremos esta variable solo para la caída y el salto
    public float velocidadY = 0.0f; 

    private CharacterController controller;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update() 
    {
        Debug.Log(velocidadY);
        // Calculamos el movimiento horizontal SIEMPRE (para poder movernos en el aire)
        Vector3 moveDirection = new Vector3(-Input.GetAxis("Horizontal"), 0, -Input.GetAxis("Vertical"));
        
        // Calculamos la rotación
        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
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

        // Unimos el movimiento vertical (Y) con el horizontal (X, Z)
        moveDirection.y = velocidadY;
        
        // Movemos al personaje
        controller.Move(moveDirection * Time.deltaTime);
    }
    void MovementCharacter()
    {
        
    }
}