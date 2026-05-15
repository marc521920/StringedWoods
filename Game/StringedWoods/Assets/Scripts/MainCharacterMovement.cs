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

    private float tiempoSinTocarSuelo = 0f;

    [Header("Armas")]
    public GameObject espada;
    public GameObject martillo;
    public GameObject guadaña;
    public GameObject lanza;
    public GameObject espadaLv2;
    public GameObject martilloLv2;
    public GameObject guadañaLv2;
    public GameObject lanzaLv2;
    public GameObject armaActual; // Para guardar el arma que tenemos equipada y mostrarla en el UI, por ejemplo

    [Header("Effects")]
    public GameObject AttackDownEffect; // Efecto visual para el ataque hacia abajo
    public GameObject TransformAttackDownEffect; // Efecto visual para la transformación del ataque hacia abajo


    void Start()
    {
        controller = GetComponent<CharacterController>();
        //attackArea.SetActive(false); // Aseguramos que el área de ataque esté desactivada al inicio
        GameManager.Instance.areaActual.SetActive(false); // Aseguramos que el área de ataque esté desactivada al inicio
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
        // 1. Recogemos el input puro
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 moveDirection;

        // 2. ¿Estamos en la sala especial? (Girada 90 grados)
        if (GameManager.Instance.salaEspecial == true)
        {
            Debug.Log("¡Estamos en la sala especial! Cambiando controles para rotación de 90 grados.");
            // Cambiamos las reglas del mundo:
            // La 'W' y 'S' (v) ahora te mueven en el eje X (acercarte/alejarte de la cámara)
            // La 'A' y 'D' (h) ahora te mueven en el eje Z (ir a izquierda/derecha en la pantalla)
            // El -v y la h positiva alinean el teclado perfectamente con la cámara a 270 grados
            moveDirection = new Vector3(-v, 0, h);
        }
        else
        {
            // Tu movimiento original intacto para el resto del juego
            moveDirection = new Vector3(-h, 0, -v);
        }
        
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
            tiempoSinTocarSuelo = 0f; // Reseteamos el temporizador
            animator.SetBool("isOnFloor", true);
            salto = false; 
           
            if (velocidadY < 0) 
            {
                velocidadY = -2f; 
            }

            if (Input.GetKey(KeyCode.Space) && salto == false && canJump == true) 
            {
                if (soltadoBotonSalto)
                {
                    salto = true;
                    animator.SetTrigger("isJumping");
                    velocidadY = jumpSpeed; 
                    soltadoBotonSalto = false; 
                }                
            } 
            else if (!Input.GetKeyUp(KeyCode.Space)) 
            {
                soltadoBotonSalto = true; 
            }
        }
        
        else 
        {
            // --- APLICAMOS LA GRAVEDAD SIEMPRE QUE ESTEMOS EN EL AIRE ---
            velocidadY -= gravity * Time.deltaTime;

            // --- PERO LAS ANIMACIONES SE ESPERAN 0.1 SEGUNDOS ANTES DE REACCIONAR ---
            tiempoSinTocarSuelo += Time.deltaTime;
            
            if (tiempoSinTocarSuelo > 0.1f) 
            {
                animator.ResetTrigger("isJumping");
                animator.SetBool("isOnFloor", false);
            }

            if (!Input.GetKey(KeyCode.Space) && velocidadY > 0 && salto == true) 
            {
                salto = false;
                velocidadY *= 0.5f; 
            }
        }
        
        if (Input.GetKeyDown(KeyCode.Q) && !dashInCooldown && !isAttacking) 
        {
            StartCoroutine(Dash()); // Iniciamos el dash
        }
        // Unimos el movimiento vertical (Y) con el horizontal (X, Z)
        moveDirection.y = velocidadY;

        if (Time.timeScale > 0f && canMove) 
        {
            controller.Move(moveDirection * Time.deltaTime);
        }
        else if (canMove)
        {
            controller.Move(moveDirection * Time.deltaTime);
        }
        else
        {
            controller.Move(new Vector3(0, moveDirection.y, 0) * Time.deltaTime); // Si no puede moverse, solo aplicamos la gravedad
        }

        if ( (moveDirection.x != 0 || moveDirection.z != 0)) 
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
                if (Physics.Raycast(ray, out hit, maxHeightAttack) && canAttack == true) 
                {
                // ¡SOLO frenamos si está en el aire!
                    if (!controller.isGrounded) 
                    {
                        velocidadY = velocidadY / 2f;
                    }
                    StartCoroutine(Atack()); 
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
        animator.SetBool("isDashing", true); // Aquí podrías reproducir una animación de dash, por ejemplo:
        Physics.IgnoreLayerCollision(layerPlayer, layerEnemy, true);

    // la velocidad Y a 0 para que no caiga mientras dashea en el aire
        velocidadY = 0f; 

        float startTime = Time.time; // Guardamos el momento exacto en el que empieza

    // Mientras no haya pasado el tiempoDash, nos movemos
        while (Time.time < startTime + GameManager.Instance.tiempoDeDash)
        {
        // Movemos al personaje hacia donde mira
            controller.Move(transform.forward * velocidadDash * Time.deltaTime);
        
        // Esperamos al siguiente frame para continuar el bucle
        yield return null; 
        }
        Physics.IgnoreLayerCollision(layerPlayer, layerEnemy, false);
        // 3. Terminamos el dash y devolvemos el control al jugador
        isDashing = false;
        animator.SetBool("isDashing", false); // Aquí terminarías la animación de dash, por ejemplo:
        dashInCooldown = true; // Activamos el cooldown para evitar dashes consecutivos
    StartCoroutine(CooldownDash()); // Iniciamos la rutina de cooldown
    }



    // Rutina de cooldown para el dash
    IEnumerator CooldownDash()
    {
      yield return new WaitForSeconds(GameManager.Instance.cooldownDeDash); // Esperamos el tiempo de cooldown antes de permitir otro dash
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
                animator.SetBool("isCharging", true);
                 // Aquí podrías reproducir una animación de carga, por ejemplo:
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
            animator.SetInteger("Attack", 4);
            animator.SetBool("isCharging", false); // Aquí terminarías la animación de carga, por ejemplo:
            fuerzaGolpe = fuerzaGolpe*2f;
            canAttack = false;
            //attackAreaCargado.SetActive(true); 
            GameManager.Instance.areaActual.SetActive(true);
            GameManager.Instance.ataqueActual = 3; // Set the current attack to the charged attack
            velocidadY = velocidadY/2f; 

            yield return new WaitForSeconds(GameManager.Instance.duracionAtaqueCargado); // Duración del ataque cargado
            
            //attackAreaCargado.SetActive(false);
            GameManager.Instance.areaActual.SetActive(false);
            animator.SetInteger("Attack", 0);
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
            animator.SetBool("isCharging", false); // Aquí terminarías la animación de carga, por ejemplo:
            isAttacking = true;
            rotationSpeed = rotationSpeedBackup; 
            speed = GameManager.Instance.velocidadDeMovimiento; 
            fuerzaGolpe = GameManager.Instance.fuerzaDeEmpuje; 
            canMove = true;
            canJump = true;
            canAttack = false;

            // --- GOLPE 1 ---
            animator.SetInteger("Attack", 1);
            //attackArea.SetActive(true); 
            GameManager.Instance.areaActual.SetActive(true);
            GameManager.Instance.ataqueActual = 0; // Set the current attack to the first attack
            velocidadY = velocidadY/2f; 
            

            float timer = 0f;
            bool comboQueued = false; // Memoria de si el jugador pulsó click de nuevo
            bool ataqueEnElAire = false; // Para saber si el ataque se hizo en el aire o en el suelo
            float temporizadorAtaque = 0f;
            temporizadorAtaque = GameManager.Instance.duracionAtaque1;
                if (!controller.isGrounded) 
                {
                    temporizadorAtaque = GameManager.Instance.duracionAtaqueEnElAire;
                    ataqueEnElAire = true;
                     // Cambiamos a la animación de ataque en el aire
                }

            // En lugar de WaitForSeconds, usamos un bucle para "escuchar" clics mientras dura el ataque
            while (timer < temporizadorAtaque) 
            {

                 // Si el jugador aterriza, interrumpimos el ataque para que no se quede flotando
                if (Input.GetMouseButtonDown(0) && ataqueEnElAire == false) comboQueued = true;
                timer += Time.deltaTime;
                yield return null;
            }

            //attackArea.SetActive(false); 
            GameManager.Instance.areaActual.SetActive(false);
            if (ataqueEnElAire) {
                // Si el ataque se hizo en el aire, reseteamos la animación al golpear el suelo
                animator.SetInteger("Attack", 0);
            }

            // Ventana extra de tiempo (0.2s) para pulsar el botón por si el jugador es un poco lento
            timer = 0f;
            while (timer < 0.2f && !comboQueued)
            {
                if (Input.GetMouseButtonDown(0) && ataqueEnElAire == false) { comboQueued = true; break; }
                timer += Time.deltaTime;
                yield return null;
            }

            // --- GOLPE 2 (Si el jugador pulsó click a tiempo) ---
            if (comboQueued)
            {
                comboQueued = false; // Reseteamos la memoria para el siguiente golpe
                animator.SetInteger("Attack", 2);
                // Aquí podrías añadir: animator.SetBool("combo2", true);
                //attackAreaCombo2.SetActive(true); 
                GameManager.Instance.areaActual.SetActive(true);
                GameManager.Instance.ataqueActual = 1; // Set the current attack to the second attack
                velocidadY = velocidadY/2f; // Volvemos a frenar la caída por el nuevo golpe

                timer = 0f;
                while (timer < GameManager.Instance.duracionAtaque2) // Duración del golpe 2
                {
                    if (Input.GetMouseButtonDown(0)) comboQueued = true;
                    timer += Time.deltaTime;
                    yield return null;
                    
                }
                // animator.SetBool("combo2", false);
                //attackAreaCombo2.SetActive(false); 
                GameManager.Instance.areaActual.SetActive(false);

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
                    animator.SetInteger("Attack", 3);
                    //attackAreaCombo3.SetActive(true); 
                    GameManager.Instance.areaActual.SetActive(true);
                    GameManager.Instance.ataqueActual = 2; // Set the current attack to the third attack
                    fuerzaGolpe = fuerzaGolpe * 1.5f; // ¡El último golpe del combo empuja más fuerte!
                    velocidadY = velocidadY/2f;

                    // Como es el último golpe, ya no guardamos memoria de clics, solo esperamos a que termine
                    yield return new WaitForSeconds(GameManager.Instance.duracionAtaque3); // Dura un poco más por ser el remate
                    
                    // animator.SetBool("combo3", false);
                    
                    //attackAreaCombo3.SetActive(false); 
                    GameManager.Instance.areaActual.SetActive(false); 
                }
            }
            ataqueEnElAire = false;
            animator.SetInteger("Attack", 0);

            isAttacking = false; 
            StartCoroutine(cooldownAttack());
        }
    }
        // Aquí iría la lógica de ataque, por ejemplo, detectar enemigos cercanos y aplicar daño
    




    IEnumerator AttackJump()
    {
        float temporizador = 0f;
        // --- 1. PREPARACIÓN ---
        animator.SetInteger("Attack", 5);
        
        canJump = false; 
        canAttack = false; 
        canMove = false;

        // Guardamos la velocidad y detenemos al jugador en el aire por completo
        float velocidadNormal = velocidadY;
        velocidadY = 0f; // 0f lo deja totalmente "congelado" en el aire. Si quieres que caiga despacito, usa velocidadY / 2f;

        // --- 2. SUSPENSIÓN EN EL AIRE ---
       while (temporizador < 0.5f) // El personaje se queda suspendido en el aire durante medio segundo, ajusta este valor a tu gusto
        {
            velocidadY = 0f; // Seguimos asegurando que no caiga durante la suspensión
            temporizador += Time.deltaTime;
            yield return null; // Esperamos al siguiente frame para continuar el bucle
        }

        // --- 3. CAÍDA Y ATAQUE ---
        velocidadY = velocidadNormal*2; // Devolvemos la gravedad/velocidad a la normalidad
        attackAreaJump.SetActive(true); // Encendemos el área de daño mientras cae
        GameManager.Instance.ataqueActual = 4;

        // --- 4. ESPERAR HASTA ATERRIZAR ---
        // Este while es mágico: pausa la corrutina frame a frame HASTA que el jugador toque el suelo.
        while (!controller.isGrounded)
        {
            yield return null; // "Espera al siguiente frame y vuelve a comprobar"
        }

        // --- 5. IMPACTO EN EL SUELO ---
        // ¡Boom! Toca el suelo, instanciamos las partículas
        Instantiate(AttackDownEffect, TransformAttackDownEffect.transform.position, TransformAttackDownEffect.transform.rotation);
        
        // --- 6. TIEMPO DE RECUPERACIÓN (RECOVERY) ---
        // Esperamos medio segundo (0.5f) para que se vea el impacto antes de dejarle moverse.
        // Ajusta este valor si quieres que la pausa en el suelo sea mayor o menor.
        yield return new WaitForSeconds(0.2f); 

        // --- 7. RESETEO TOTAL ---
        animator.SetInteger("Attack", 0);
        attackAreaJump.SetActive(false); 
        
        canMove = true; 
        canJump = true; 
        
        cooldownAttackNumber += 0.2f; 
        StartCoroutine(cooldownAttack()); 
    }





    IEnumerator AttackDash()
    {
        animator.SetBool("isCharging", false); // Aquí terminarías la animación de carga, por ejemplo:
        animator.SetBool("isDashing", true); // Aquí terminarías la animación de dash, por ejemplo:
        animator.SetInteger("Attack", 6);
        
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
        //attackAreaDash.SetActive(true); // Activamos el área de ataque para el salto
        GameManager.Instance.areaActual.SetActive(true);
        GameManager.Instance.ataqueActual = 5; // Set the current attack to the dash attack

        // Esperamos un momento para simular el tiempo de ataque
        yield return new WaitForSeconds(0.2f); // Ajusta este valor según la duración de tu animación
        //attackAreaDash.SetActive(false); // Desactivamos el área de ataque después de un momento
        GameManager.Instance.areaActual.SetActive(false);
        canMove = true; // Reactivamos el movimiento después del ataque
        canJump = true; // Reactivamos el salto después del ataque
        animator.SetBool("isDashing", false); // Aquí terminarías la animación de dash, por ejemplo:
        animator.SetInteger("Attack", 0);
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

    public void CambiarArma()
    {
        // Aquí podrías cambiar el sprite del arma que tiene el jugador, o activar/desactivar modelos 3D, etc.
        if (GameManager.Instance.Armas == 1)
        {
            animator.SetInteger("Weapon", 1);
            animator.Play("IDDLE_sword");
            espada.SetActive(true);
            martillo.SetActive(false);
            guadaña.SetActive(false);
            lanza.SetActive(false);
            espadaLv2.SetActive(false);
            martilloLv2.SetActive(false);
            guadañaLv2.SetActive(false);
            lanzaLv2.SetActive(false);
             // Cambia a la segunda arma (ejemplo)
        }
        else if (GameManager.Instance.Armas == 2)
        {
            animator.SetInteger("Weapon", 2);
            animator.Play("IDDLE_hammer");
            espada.SetActive(false);
            martillo.SetActive(true);
            guadaña.SetActive(false);
            lanza.SetActive(false);
            espadaLv2.SetActive(false);
            martilloLv2.SetActive(false);
            guadañaLv2.SetActive(false);
            lanzaLv2.SetActive(false);
             // Cambia a la tercera arma (ejemplo)
        }
        else if (GameManager.Instance.Armas == 3)
        {
            animator.SetInteger("Weapon", 3);
            animator.Play("IDDLE_scythe");
            espada.SetActive(false);
            martillo.SetActive(false);
            guadaña.SetActive(true);
            lanza.SetActive(false);
            espadaLv2.SetActive(false);
            martilloLv2.SetActive(false);
            guadañaLv2.SetActive(false);
            lanzaLv2.SetActive(false);
             // Cambia a la cuarta arma (ejemplo)
        }
        else if (GameManager.Instance.Armas == 4)
        {
            animator.SetInteger("Weapon", 4);
            animator.Play("IDDLE_spear");
            espada.SetActive(false);
            martillo.SetActive(false);
            guadaña.SetActive(false);
            lanza.SetActive(true);
            espadaLv2.SetActive(false);
            martilloLv2.SetActive(false);
            guadañaLv2.SetActive(false);
            lanzaLv2.SetActive(false);
             // Cambia a la quinta arma (ejemplo)
        }
        else if (GameManager.Instance.Armas == 5)
        {
            animator.SetInteger("Weapon", 1);
            animator.Play("IDDLE_sword");
            espada.SetActive(false);
            martillo.SetActive(false);
            guadaña.SetActive(false);
            lanza.SetActive(false);
            espadaLv2.SetActive(true);
            martilloLv2.SetActive(false);
            guadañaLv2.SetActive(false);
            lanzaLv2.SetActive(false);
             // Cambia a la sexta arma (ejemplo)
        }
        else if (GameManager.Instance.Armas == 6)
        {
            animator.SetInteger("Weapon", 2);
            animator.Play("IDDLE_hammer");
            espada.SetActive(false);
            martillo.SetActive(false);
            guadaña.SetActive(false);
            lanza.SetActive(false);
            espadaLv2.SetActive(false);
            martilloLv2.SetActive(true);
            guadañaLv2.SetActive(false);
            lanzaLv2.SetActive(false);
             // Cambia a la séptima arma (ejemplo)
        }
        else if (GameManager.Instance.Armas == 7)
        {
            animator.SetInteger("Weapon", 3);
            animator.Play("IDDLE_scythe");
            espada.SetActive(false);
            martillo.SetActive(false);
            guadaña.SetActive(false);
            lanza.SetActive(false);
            espadaLv2.SetActive(false);
            martilloLv2.SetActive(false);
            guadañaLv2.SetActive(true);
            lanzaLv2.SetActive(false);
             // Cambia a la octava arma (ejemplo)
        }
        else if (GameManager.Instance.Armas == 8)
        {
            animator.SetInteger("Weapon", 4);
            animator.Play("IDDLE_spear");
            espada.SetActive(false);
            martillo.SetActive(false);
            guadaña.SetActive(false);
            lanza.SetActive(false);
            espadaLv2.SetActive(false);
            martilloLv2.SetActive(false);
            guadañaLv2.SetActive(false);
            lanzaLv2.SetActive(true);
             // Cambia a la novena arma (ejemplo)
        }
        Debug.Log("¡Has cambiado de arma! Arma actual: " + GameManager.Instance.Armas);
    }
}