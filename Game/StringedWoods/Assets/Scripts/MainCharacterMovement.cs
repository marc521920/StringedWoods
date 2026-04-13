using UnityEngine;
using System.Collections;

public class MainCharacterMovement : MonoBehaviour
{
    public float speed = 6.0f;
    public float jumpSpeed = 18.0f;
    public float gravity = 20.0f;
    public float rotationSpeed = 15.0f;
    public float derrape = 10f;

    //salto cargado
    private bool salto = true;
    private bool soltadoBotonSalto = true;

    // Dash
    [Header("Dash Settings")]
    public float velocidadDash = 30f;
    public float tiempoDash = 0.2f; // Cuánto dura el impulso en segundos
    private bool isDashing = false; // Para saber si estamos en un dash
    private bool dashInCooldown = false; // Para evitar que el jugador pueda dashear inmediatamente
    public float cooldownDash = 1f; // Tiempo de espera entre dashes

    // Ataque
    [Header("Attack Settings")]
    public float attackRange = 2f; // Rango del ataque
    public float attackDamage = 10f; // Daño del ataque
    public GameObject attackArea; // Un objeto vacío que representa el área de ataque, con un collider para detectar enemigos
    public float maxHeightAttack = 5f;
    private bool isAttacking = false; // Para saber si estamos en medio de un ataque
    public float fuerzaGolpe = 5f; // Fuerza del golpe que se aplicará al enemigo
    private bool canAttack = true; // Para controlar el tiempo entre ataques
    public float cooldownAttack = 0.5f; // Tiempo de espera entre ataques
    // Usaremos esta variable solo para la caída y el salto
    public float velocidadY = 0.0f; 

    private CharacterController controller;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        attackArea.SetActive(false); // Aseguramos que el área de ataque esté desactivada al inicio
        float speed = PlayerPrefs.GetFloat("velocidadDeMovimiento", 6.0f);
    
    
    
        float tiempoDash = PlayerPrefs.GetFloat("tiempoDeDash", 0.2f); // Cuánto dura el impulso en segundos
    
        float cooldownDash = PlayerPrefs.GetFloat("cooldownDeDash", 1f); // Tiempo de espera entre dashes

    
        float attackRange = PlayerPrefs.GetFloat("rangoDeAtaque", 2f); // Rango del ataque
        float attackDamage = PlayerPrefs.GetFloat("dañoAlAtacar", 10f); // Daño del ataque
        float cooldownAttack = PlayerPrefs.GetFloat("velocidadDeAtaque", 0.5f); // Tiempo de espera entre ataques
    
        float fuerzaGolpe = PlayerPrefs.GetFloat("fuerzaDeEmpuje", 5f); // Fuerza del golpe que se aplicará al enemigo
    
    }

    void Update() 
    {
       if (isDashing) return;
        //Debug.Log(velocidadY);
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
        
        if (Input.GetKeyDown(KeyCode.Q) && !dashInCooldown && !isAttacking) 
        {
            StartCoroutine(Dash()); // Iniciamos el dash
        }
        // Unimos el movimiento vertical (Y) con el horizontal (X, Z)
        moveDirection.y = velocidadY;
        
        // Movemos al personaje
        controller.Move(moveDirection * Time.deltaTime);

        // Ataque
            if (Input.GetMouseButtonDown(0)) // Si se presiona el botón izquierdo del mouse
            {
                Ray ray = new Ray(transform.position, Vector3.down); // Lanzamos un rayo hacia adelante para detectar enemigos
                RaycastHit hit;
                Debug.DrawRay(transform.position, Vector3.down * maxHeightAttack, Color.red, 1f); // Dibuja el rayo en la escena para depuración
                if (Physics.Raycast(ray, out hit, maxHeightAttack)) // Si el rayo golpea algo dentro del rango de ataque
                {
                    velocidadY = velocidadY/2f;
                    StartCoroutine(Atack()); // Iniciamos la rutina de ataque
                    
                }
                else
                {
                    if (velocidadY >= -20)
                    {
                        velocidadY = -10f*2f;
                    }
                    
                    else if (velocidadY < -20)
                    {
                        velocidadY = velocidadY*2f;
                    }

                }
            }
    }



    // Rutina de dash
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



    // Rutina de cooldown para el dash
    IEnumerator CooldownDash()
    {
      yield return new WaitForSeconds(cooldownDash); // Esperamos el tiempo de cooldown antes de permitir otro dash
      dashInCooldown = false; // Desactivamos el cooldown
    }



    // Rutina de ataque
    IEnumerator Atack()
    {
            isAttacking = true; // Marcamos que estamos atacando
            attackArea.SetActive(true); // Activamos el área de ataque
            velocidadY = velocidadY/2f; // Detenemos el movimiento vertical durante el ataque
            // Aquí podrías reproducir una animación de ataque, por ejemplo:
            // animator.SetTrigger("Attack");

            // Esperamos un momento para simular el tiempo de ataque
            yield return new WaitForSeconds(0.2f); // Ajusta este valor según la duración de tu animación
            
            attackArea.SetActive(false); // Desactivamos el área de ataque después de un momento
            isAttacking = false; // Terminamos el ataque
            canAttack = false; // Desactivamos la posibilidad de atacar inmediatamente después
            
        // Aquí iría la lógica de ataque, por ejemplo, detectar enemigos cercanos y aplicar daño
    }
    IEnumerator cooldownAtack()
    {
        yield return new WaitForSeconds(0.5f); // Tiempo de espera entre ataques, ajusta según tu animación
        canAttack = true; // Permitimos atacar de nuevo
    }
}