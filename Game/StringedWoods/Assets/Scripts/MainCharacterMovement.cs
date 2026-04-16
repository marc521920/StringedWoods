using UnityEngine;
using System.Collections;

public class MainCharacterMovement : MonoBehaviour
{
    public float speed = 6.0f;
    public float jumpSpeed = 18.0f;
    public float gravity = 20.0f;
    public float rotationSpeed = 15.0f;
    public float derrape = 10f;
    public bool canMove = true; // Variable para controlar si el personaje puede moverse o no
    public bool canJump = true; // Variable para controlar si el personaje puede saltar o no

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
    public GameObject attackAreaJump; // Un objeto vacío que representa el área de ataque para el salto
    public GameObject attackAreaCargado; // Un objeto vacío que representa el área de ataque cargado
    public GameObject attackAreaDash; // Un objeto vacío que representa el área de ataque para el dash
    public float maxHeightAttack = 5f;
    private bool isAttacking = false; // Para saber si estamos en medio de un ataque
    public float fuerzaGolpe = 5f; // Fuerza del golpe que se aplicará al enemigo
    public bool canAttack = true; // Para controlar el tiempo entre ataques
    public float cooldownAttackNumber = 0.5f; // Tiempo de espera entre ataques
    public float cooldownAttackCharged = 2f; // Tiempo que se debe mantener presionado el botón para hacer un ataque cargado
    // Usaremos esta variable solo para la caída y el salto
    public float velocidadY = 0.0f; 

    private CharacterController controller;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        attackArea.SetActive(false); // Aseguramos que el área de ataque esté desactivada al inicio
        speed = GameManager.Instance.velocidadDeMovimiento; // Velocidad de movimiento del personaje, cargada desde el GameManager
        tiempoDash = GameManager.Instance.tiempoDeDash; // Cuánto dura el impulso en segundos
        cooldownDash = GameManager.Instance.cooldownDeDash; // Tiempo de espera entre dashes
        attackRange = GameManager.Instance.rangoDeAtaque; // Rango del ataque
        attackDamage = GameManager.Instance.dañoAlAtacar; // Daño del ataque
        cooldownAttackNumber = GameManager.Instance.velocidadDeAtaque; // Tiempo de espera entre ataques
        fuerzaGolpe = GameManager.Instance.fuerzaDeEmpuje; // Fuerza del golpe que se aplicará al enemigo
        canAttack = true; // Permitimos atacar al inicio del juego
    }

    void Update() 
    {
       if (isDashing) return;
        //Debug.Log(velocidadY);
        // Calculamos el movimiento horizontal SIEMPRE (para poder movernos en el aire)
        Vector3 moveDirection = new Vector3(-Input.GetAxis("Horizontal"), 0, -Input.GetAxis("Vertical"));
        
        // Calculamos la rotación
        if (moveDirection != Vector3.zero && canMove)
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
            if (Input.GetKey(KeyCode.Space) && salto == false && canJump == true) {
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
        if (canMove)
        {
            controller.Move(moveDirection * Time.deltaTime);
        }
        

        // Ataque
            if (Input.GetMouseButtonDown(0)) // Si se presiona el botón izquierdo del mouse
            {
                Ray ray = new Ray(transform.position, Vector3.down); // Lanzamos un rayo hacia adelante para detectar enemigos
                RaycastHit hit;
                Debug.DrawRay(transform.position, Vector3.down * maxHeightAttack, Color.red, 1f); // Dibuja el rayo en la escena para depuración
                if (Physics.Raycast(ray, out hit, maxHeightAttack) && canAttack == true) // Si el rayo golpea algo dentro del rango de ataque
                {
                    velocidadY = velocidadY/2f;
                    StartCoroutine(Atack()); // Iniciamos la rutina de ataque
                    
                }
                else if (canAttack == true) // Si el rayo no golpea nada, pero el jugador puede atacar, hacemos un ataque de salto
                {
                    
                        velocidadY = -10f*2f;
                    

                    StartCoroutine(AttackJump()); // Iniciamos la rutina de ataque para el salto
                    

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
        float rotationSpeedBackup = rotationSpeed; // Guardamos la velocidad de rotación original para restaurarla después del ataque
        float tiempoPresionado = 0f; // Contador para medir cuánto tiempo se mantiene presionado el botón de ataque
        while (Input.GetMouseButton(0))
        {

            
            
            tiempoPresionado += Time.deltaTime;
            if (tiempoPresionado >= 0.3f) 
            {
                rotationSpeed = rotationSpeed/1.1f; // Reducimos la velocidad de rotación para hacer el ataque más lento y pesado
                speed = speed/1.1f;
                
                canJump = false;
                if (tiempoPresionado >= cooldownAttackCharged && isDashing == true) 
                {
                    rotationSpeed = rotationSpeedBackup; // Restauramos la velocidad de rotación original
                    speed = GameManager.Instance.velocidadDeMovimiento; // Aseguramos que la velocidad vuelva a su valor normal
                    canJump = true;
                    StartCoroutine(AttackDash()); // Si se ha mantenido presionado el tiempo suficiente para un ataque cargado, hacemos un ataque de salto
                    yield break; // Si se ha mantenido presionado el tiempo suficiente para un ataque cargado, salimos del bucle
                }
                else if (tiempoPresionado < cooldownAttackCharged && isDashing == true) 
                {    // Si se ha soltado el botón antes de tiempo, pero estamos dasheando, hacemos un ataque de salto normal
                    rotationSpeed = rotationSpeedBackup; // Restauramos la velocidad de rotación original
                    speed = GameManager.Instance.velocidadDeMovimiento; // Aseguramos que la velocidad vuelva a su valor normal
                    canJump = true;
                    yield break; // Si se ha soltado el botón antes de tiempo, pero estamos dasheando, salimos del bucle
                }
                // Si se ha mantenido presionado el tiempo suficiente para un ataque cargado, salimos del bucle
            }
            
            // yield return null le dice a Unity: "Pausa el bucle aquí y continúa en el siguiente frame"
            yield return null; 
        }
        if (tiempoPresionado >= cooldownAttackCharged) // Si el botón se ha mantenido presionado durante al menos medio segundo, hacemos un ataque cargado
        {
            fuerzaGolpe = fuerzaGolpe*2f;
            canAttack = false;
            
            attackAreaCargado.SetActive(true); // Activamos el área de ataque
            velocidadY = velocidadY/2f; // Detenemos el movimiento vertical durante el ataque
            // Aquí podrías reproducir una animación de ataque, por ejemplo:
            // animator.SetTrigger("Attack");

            // Esperamos un momento para simular el tiempo de ataque
            yield return new WaitForSeconds(0.2f); // Ajusta este valor según la duración de tu animación
            
            attackAreaCargado.SetActive(false);
            rotationSpeed = rotationSpeedBackup; // Restauramos la velocidad de rotación original
            speed = GameManager.Instance.velocidadDeMovimiento; // Desactivamos el área de ataque después de un momento
            isAttacking = false; // Terminamos el ataque
            canMove = true;
            canJump = true;
             // Desactivamos la posibilidad de atacar inmediatamente después
            StartCoroutine(cooldownAttack());
        }
        else if (tiempoPresionado < cooldownAttackCharged) // Si el botón se ha soltado antes de tiempo, hacemos un ataque normal
        {
            isAttacking = true;
            rotationSpeed = rotationSpeedBackup; // Restauramos la velocidad de rotación original
            speed = GameManager.Instance.velocidadDeMovimiento; // Aseguramos que la velocidad vuelva a su valor normal
            fuerzaGolpe = GameManager.Instance.fuerzaDeEmpuje; 
            canMove = true;
            canJump = true;
            // Aseguramos que la fuerza de golpe vuelva a su valor normal
            canAttack = false;
             // Marcamos que estamos atacando
            attackArea.SetActive(true); // Activamos el área de ataque
            velocidadY = velocidadY/2f; // Detenemos el movimiento vertical durante el ataque
            // Aquí podrías reproducir una animación de ataque, por ejemplo:
            // animator.SetTrigger("Attack");

            // Esperamos un momento para simular el tiempo de ataque
            yield return new WaitForSeconds(0.3f); // Ajusta este valor según la duración de tu animación
            
            attackArea.SetActive(false); // Desactivamos el área de ataque después de un momento
            isAttacking = false; // Terminamos el ataque
             // Desactivamos la posibilidad de atacar inmediatamente después
            StartCoroutine(cooldownAttack());
        }
        // Aquí iría la lógica de ataque, por ejemplo, detectar enemigos cercanos y aplicar daño
    }
    IEnumerator AttackJump()
    {
        while (!controller.isGrounded) 
        {
            //Animation de caida con ataque
            yield return null; // Esperamos al siguiente frame para continuar el bucle
        }
        canMove = false; // Desactivamos el movimiento durante el ataque
        canJump = false; // Desactivamos el salto durante el ataque
        velocidadY = velocidadY/2f; // Detenemos el movimiento vertical durante el ataque
        canAttack = false; // Desactivamos la posibilidad de atacar inmediatamente después
        // Aquí podrías reproducir una animación de ataque, por ejemplo:
        // animator.SetTrigger("Attack");
        attackAreaJump.SetActive(true); // Activamos el área de ataque para el salto

        // Esperamos un momento para simular el tiempo de ataque
        yield return new WaitForSeconds(0.2f); // Ajusta este valor según la duración de tu animación
        attackAreaJump.SetActive(false); // Desactivamos el área de ataque después de un momento
        canMove = true; // Reactivamos el movimiento después del ataque
        canJump = true; // Reactivamos el salto después del ataque
        cooldownAttackNumber = cooldownAttackNumber+0.2f; // Reducimos el tiempo de espera entre ataques para hacer el ataque de salto más fluido
        StartCoroutine(cooldownAttack()); // Iniciamos el cooldown para evitar ataques consecutivos

    }
    IEnumerator AttackDash()
    {
        while (isDashing) 
        {
            //Animation de ataque durante el dash
            yield return null; // Esperamos al siguiente frame para continuar el bucle
        }
        canMove = false; // Desactivamos el movimiento durante el ataque
        canJump = false; // Desactivamos el salto durante el ataque
        velocidadY = velocidadY/2f; // Detenemos el movimiento vertical durante el ataque
        canAttack = false; // Desactivamos la posibilidad de atacar inmediatamente después
        // Aquí podrías reproducir una animación de ataque, por ejemplo:
        // animator.SetTrigger("Attack");
        attackAreaDash.SetActive(true); // Activamos el área de ataque para el salto

        // Esperamos un momento para simular el tiempo de ataque
        yield return new WaitForSeconds(0.2f); // Ajusta este valor según la duración de tu animación
        attackAreaDash.SetActive(false); // Desactivamos el área de ataque después de un momento
        canMove = true; // Reactivamos el movimiento después del ataque
        canJump = true; // Reactivamos el salto después del ataque
        StartCoroutine(cooldownAttack()); // Iniciamos el cooldown para evitar ataques consecutivos

    }
    IEnumerator cooldownAttack()
    {
        fuerzaGolpe = GameManager.Instance.fuerzaDeEmpuje; // Aseguramos que la fuerza de golpe vuelva a su valor normal
        yield return new WaitForSeconds(cooldownAttackNumber); // Tiempo de espera entre ataques, ajusta según tu animación
        canAttack = true; // Permitimos atacar de nuevo
        cooldownAttackNumber = GameManager.Instance.velocidadDeAtaque; // Aseguramos que el tiempo de espera vuelva a su valor normal
    }
}