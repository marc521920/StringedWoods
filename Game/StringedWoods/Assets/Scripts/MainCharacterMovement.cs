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
    public GameObject attackArea; // Golpe 1
    public GameObject attackAreaCombo2; // Golpe 2
    public GameObject attackAreaCombo3; // Golpe 3 (Fin del combo)
    public GameObject attackAreaJump; // Un objeto vacío que representa el área de ataque para el salto
    public GameObject attackAreaCargado; // Un objeto vacío que representa el área de ataque cargado
    public GameObject attackAreaDash; // Un objeto vacío que representa el área de ataque para el dash
    public float maxHeightAttack = 5f;
    private bool isAttacking = false; // Para saber si estamos en medio de un ataque
    public float fuerzaGolpe = 5f; // Fuerza del golpe que se aplicará al enemigo
    public bool canAttack = true; // Para controlar el tiempo entre ataques
    public float cooldownAttackNumber = 0.5f; // Tiempo de espera entre ataques
    public float cooldownAttackCharged = 2f; // Tiempo que se debe mantener presionado el botón para hacer un ataque cargado
    private bool isInvulnerable = false; // Activamos el estado de invulnerabilidad
    // Usaremos esta variable solo para la caída y el salto
    public float velocidadY = 0.0f; 
  


    // ... el resto de tus variables ...

    [Header("Ajustes de Daño")]
    private bool estaEmpujado = false; // Para perder el control mientras salimos volando
    public float duracionEmpuje = 0.2f; // Cuánto dura el "vuelo" hacia atrás
    public float fuerzaDeEmpuje = 10f; // Fuerza con la que el personaje es lanzado hacia atrás al recibir daño
    // (La variable fuerzaDeEmpuje ya la tienes)
    private int layerPlayer;
    private int layerEnemy;
    // animacion
    [Header("Animator")]
    public Animator animator;

    private CharacterController controller;

    public GameObject gameManager;

    [Header("Effects")]
    public GameObject AttackDownEffect; // Efecto visual para el ataque hacia abajo
    public GameObject TransformAttackDownEffect; // Efecto visual para la transformación del ataque hacia abajo


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


        layerPlayer = LayerMask.NameToLayer("player");
        layerEnemy = LayerMask.NameToLayer("enemy");
    }

    void Update() 
    {
       if (isDashing|| estaEmpujado) return;
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
            animator.SetBool("inGround", true);
            animator.SetBool("Jump", false);
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
                    animator.SetBool("Jump", true);
                
                    velocidadY = jumpSpeed; // La velocidad de salto se multiplica por el valor cargado

                    soltadoBotonSalto = false; // Se marca que el botón de salto está siendo presionado

                }                
            } else if (!Input.GetKeyUp(KeyCode.Space)) {
                soltadoBotonSalto = true; // Se marca que el botón de salto ha sido soltado

                
            }
            
        }
        else 
        {
            animator.SetBool("inGround", false);
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
        if (controller.isGrounded && (moveDirection.x != 0 || moveDirection.z != 0)) 
        {
            animator.SetBool("isRunning", true);
            // Aquí podrías reproducir una animación de caminar, por ejemplo:
            // animator.SetBool("isWalking", true);
        } 
        else 
        {
            animator.SetBool("isRunning", false);
            // Aquí podrías reproducir una animación de estar quieto, por ejemplo:
            // animator.SetBool("isWalking", false);
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

   void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (isInvulnerable == true) return; // Si estamos invulnerables, no recibimos daño ni empujones
        // OJO: Asegúrate de que el tag aquí coincida exactamente con las mayúsculas/minúsculas de Unity
        if (hit.gameObject.CompareTag("head")) 
        {
            // Si estamos cayendo y tocamos su cabeza desde arriba...
            if (velocidadY < 0 && transform.position.y > hit.transform.position.y)
            {
                // 1. REBOTE AUTOMÁTICO: Te empuja hacia arriba para echarte de la cabeza.
                // Ponle un número menor que tu jumpSpeed (ej. 10f o 12f) para que sea un rebote molesto.
                velocidadY = 12f; 
                
                // 2. Le decimos a tu Update que el personaje está en el aire
                salto = true; 

                // 3. Anulamos el empujón/daño por si el cuerpo del enemigo intentó pegarte
                estaEmpujado = false; 
                
                Debug.Log("¡Rebote automático! Te expulsa de la cabeza sin hacer daño.");
            }
        }
    }

public void RecibirDaño(float fuerza, Vector3 direccionHaciaAtras)
{
    if (isInvulnerable == true) return; // Si estamos invulnerables, no recibimos daño ni empujones
    Debug.Log("¡Ay! ¡Me han dado!");
    GameManager.Instance.vidaActual -= 1;
    GameManager.Instance.CambiarCorazones();
    
    // Lanzamos la corrutina que nos empujará físicamente
    StartCoroutine(RutinaKnockback(fuerza, direccionHaciaAtras));
}

IEnumerator RutinaKnockback(float fuerza, Vector3 direccion)
{
    estaEmpujado = true; // Bloqueamos el control del jugador del Update
    
    // Le damos un pequeño "saltito" inicial para despegarlo del suelo y que la caída tenga sentido
    // Si ya estaba en el aire, esto interrumpe su caída normal y simula el impacto hacia arriba.
    velocidadY = 5f; 
    
    // Iniciamos la invulnerabilidad de inmediato para no recibir doble daño mientras volamos
    StartCoroutine(Invulnerabilidad()); 

    float tiempoPasado = 0f;
    float inerciaActual = fuerza; // Guardamos la fuerza inicial para ir reduciéndola

    // --- FASE 1: EL IMPACTO INICIAL (Dura 'duracionEmpuje') ---
    while (tiempoPasado < duracionEmpuje)
    {
        tiempoPasado += Time.deltaTime;
        
        // Aplicamos nuestra propia gravedad manual mientras está bloqueado
        velocidadY -= gravity * Time.deltaTime;
        
        // Juntamos el empujón horizontal con la caída vertical
        Vector3 movimiento = direccion * inerciaActual;
        movimiento.y = velocidadY;
        
        controller.Move(movimiento * Time.deltaTime);
        yield return null;
    }

    // --- FASE 2: CAÍDA LIBRE HASTA EL SUELO ---
    // Si después del golpe fuerte inicial seguimos en el aire, no le devolvemos el control.
    while (!controller.isGrounded)
    {
        // Seguimos aplicando gravedad para que caiga cada vez más rápido
        velocidadY -= gravity * Time.deltaTime;
        
        // Fricción en el aire: reducimos la inercia horizontal poco a poco usando Lerp
        // para que no salga disparado de forma infinita como una bala.
        inerciaActual = Mathf.Lerp(inerciaActual, 0f, Time.deltaTime * 3f); 
        
        Vector3 movimiento = direccion * inerciaActual;
        movimiento.y = velocidadY;
        
        controller.Move(movimiento * Time.deltaTime);
        yield return null;
    }

    // --- FASE 3: ATERRIZAJE ---
    // Al tocar el suelo, reseteamos la gravedad para que no se acumule
    velocidadY = -2f; 
    
    // Opcional: Pequeño "stun" o tiempo de recuperación al chocar contra el suelo
    yield return new WaitForSeconds(0.15f); 
    
    estaEmpujado = false; // ¡Le devolvemos el control al jugador!
}



    // Rutina de dash
    IEnumerator Dash()
    {
         
    //  Empezamos el dash
        isDashing = true;
        Physics.IgnoreLayerCollision(layerPlayer, layerEnemy, true);

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
        Physics.IgnoreLayerCollision(layerPlayer, layerEnemy, false);
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
    // Rutina de ataque
    IEnumerator Atack()
    {
        float rotationSpeedBackup = rotationSpeed; 
        float tiempoPresionado = 0f; 
        
        while (Input.GetMouseButton(0))
        {
            tiempoPresionado += Time.deltaTime;
            if (tiempoPresionado >= 0.3f) 
            {
                rotationSpeed = rotationSpeed/1.1f; 
                speed = speed/1.1f;
                canJump = false;
                
                if (tiempoPresionado >= cooldownAttackCharged && isDashing == true) 
                {
                    rotationSpeed = rotationSpeedBackup; 
                    speed = GameManager.Instance.velocidadDeMovimiento; 
                    canJump = true;
                    StartCoroutine(AttackDash()); 
                    yield break; 
                }
                else if (tiempoPresionado < cooldownAttackCharged && isDashing == true) 
                {    
                    rotationSpeed = rotationSpeedBackup; 
                    speed = GameManager.Instance.velocidadDeMovimiento; 
                    canJump = true;
                    yield break; 
                }
            }
            yield return null; 
        }

        // --- ATAQUE CARGADO ---
        if (tiempoPresionado >= cooldownAttackCharged) 
        {
            fuerzaGolpe = fuerzaGolpe*2f;
            canAttack = false;
            attackAreaCargado.SetActive(true); 
            GameManager.Instance.ataqueActual = 3; // Set the current attack to the charged attack
            velocidadY = velocidadY/2f; 

            yield return new WaitForSeconds(0.2f); 
            
            attackAreaCargado.SetActive(false);
            rotationSpeed = rotationSpeedBackup; 
            speed = GameManager.Instance.velocidadDeMovimiento; 
            isAttacking = false; 
            canMove = true;
            canJump = true;
            StartCoroutine(cooldownAttack());
        }
        // --- ATAQUE NORMAL (SISTEMA DE COMBO) ---
        else if (tiempoPresionado < cooldownAttackCharged) 
        {
            isAttacking = true;
            rotationSpeed = rotationSpeedBackup; 
            speed = GameManager.Instance.velocidadDeMovimiento; 
            fuerzaGolpe = GameManager.Instance.fuerzaDeEmpuje; 
            canMove = true;
            canJump = true;
            canAttack = false;

            // --- GOLPE 1 ---
            animator.SetBool("basicAttack", true);
            attackArea.SetActive(true); 
                GameManager.Instance.ataqueActual = 0; // Set the current attack to the first attack
            velocidadY = velocidadY/2f; 
            

            float timer = 0f;
            bool comboQueued = false; // Memoria de si el jugador pulsó click de nuevo

            // En lugar de WaitForSeconds, usamos un bucle para "escuchar" clics mientras dura el ataque
            while (timer < 0.3f) 
            {
                if (Input.GetMouseButtonDown(0)) comboQueued = true;
                timer += Time.deltaTime;
                yield return null;
            }
            animator.SetBool("basicAttack", false);
            attackArea.SetActive(false); 

            // Ventana extra de tiempo (0.2s) para pulsar el botón por si el jugador es un poco lento
            timer = 0f;
            while (timer < 0.2f && !comboQueued)
            {
                if (Input.GetMouseButtonDown(0)) { comboQueued = true; break; }
                timer += Time.deltaTime;
                yield return null;
            }

            // --- GOLPE 2 (Si el jugador pulsó click a tiempo) ---
            if (comboQueued)
            {
                comboQueued = false; // Reseteamos la memoria para el siguiente golpe
                
                // Aquí podrías añadir: animator.SetBool("combo2", true);
                attackAreaCombo2.SetActive(true); 
                GameManager.Instance.ataqueActual = 1; // Set the current attack to the second attack
                velocidadY = velocidadY/2f; // Volvemos a frenar la caída por el nuevo golpe

                timer = 0f;
                while (timer < 0.3f) // Duración del golpe 2
                {
                    if (Input.GetMouseButtonDown(0)) comboQueued = true;
                    timer += Time.deltaTime;
                    yield return null;
                }
                // animator.SetBool("combo2", false);
                attackAreaCombo2.SetActive(false); 

                timer = 0f;
                while (timer < 0.2f && !comboQueued)
                {
                    if (Input.GetMouseButtonDown(0)) { comboQueued = true; break; }
                    timer += Time.deltaTime;
                    yield return null;
                }

                // --- GOLPE 3 FINISH (Si volvió a pulsar click) ---
                if (comboQueued)
                {
                    // Aquí podrías añadir: animator.SetBool("combo3", true);
                    attackAreaCombo3.SetActive(true); 
                    GameManager.Instance.ataqueActual = 2; // Set the current attack to the third attack
                    fuerzaGolpe = fuerzaGolpe * 1.5f; // ¡El último golpe del combo empuja más fuerte!
                    velocidadY = velocidadY/2f;

                    // Como es el último golpe, ya no guardamos memoria de clics, solo esperamos a que termine
                    yield return new WaitForSeconds(0.4f); // Dura un poco más por ser el remate
                    
                    // animator.SetBool("combo3", false);
                    attackAreaCombo3.SetActive(false); 
                }
            }

            isAttacking = false; 
            StartCoroutine(cooldownAttack());
        }
    }
        // Aquí iría la lógica de ataque, por ejemplo, detectar enemigos cercanos y aplicar daño
    
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
        Instantiate(AttackDownEffect,TransformAttackDownEffect.transform.position, TransformAttackDownEffect.transform.rotation); // Reproducimos el efecto visual del ataque hacia abajo
        GameManager.Instance.ataqueActual = 4; // Set the current attack to the charged attack

        // Esperamos un momento para simular el tiempo de ataque
        yield return new WaitForSeconds(0.04f); // Ajusta este valor según la duración de tu animación
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
        GameManager.Instance.ataqueActual = 5; // Set the current attack to the dash attack

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
    IEnumerator Invulnerabilidad()
    {
        isInvulnerable = true; // Activamos el estado de invulnerabilidad
        Debug.Log("¡Ahora soy invulnerable por un momento!");
        Physics.IgnoreLayerCollision(layerPlayer, layerEnemy, true);
        // Aquí podrías añadir una animación de parpadeo o algo para indicar que estás invulnerable
        yield return new WaitForSeconds(0.8f); // Duración de la invulnerabilidad, ajusta según tu animación
        Physics.IgnoreLayerCollision(layerPlayer, layerEnemy, false);
        isInvulnerable = false; // Desactivamos el estado de invulnerabilidad

        // Aquí terminaría la animación de invulnerabilidad
    }
}