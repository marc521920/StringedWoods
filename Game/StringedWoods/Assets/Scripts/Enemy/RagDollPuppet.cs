using UnityEngine;
using System.Collections;

public class RagDollPuppet : EnemyScript
{
    [Header("Ajustes de Visión")]
    public float rangoDeVision = 10f;
    public float anguloDeVision = 65f;
    public LayerMask capaObstaculos;
    
    [Header("Ajustes de Movimiento")]
    public float distanciaDeAtaque = 1.5f; 
    
    [Header("Ajustes de Daño al Jugador")]
    public float fuerzaDeMiGolpe = 7f;

    private bool estaGirando = false;
    public bool estaAtacando = false;
    private bool jugadorDetectado = false;

    public bool despertarse = true;

    public float velocidadDeGiro;

    public float tiempoAtaqueRagPuppet;

    public GameObject areaAtaqueRagPuppet;

 [Header("Effects")]
    public GameObject EffectoDeAtaque;

    public GameObject tranformDeEfecto;

    protected override void Start()
    {
        base.Start();
        vida = 100;
        animator.SetTrigger("WakeUp"); 
        StartCoroutine(Despertandose());

    }

    protected override void Moverse()
    {
        // Físicas encendidas siempre para la gravedad y recibir los knockbacks
        rb.isKinematic = false; 

        if (player == null || despertarse || estaAtacando) return;
        if (estaGolpeado) return; // Si está volando por un golpe, bloqueamos la lógica
        
        float distanciaAlJugador = Vector3.Distance(transform.position, player.transform.position);
        Vector3 direccionAlJugador = (player.transform.position - transform.position).normalized;

        // --- 1. DETECCIÓN ---
        bool enRango = distanciaAlJugador <= rangoDeVision;
        float anguloAlJugador = Vector3.Angle(transform.forward, direccionAlJugador);
        bool enAngulo = anguloAlJugador <= anguloDeVision;

        bool tieneLineaDeVision = false;
        if (enRango && enAngulo)
        {
            if (!Physics.Raycast(transform.position, direccionAlJugador, distanciaAlJugador, capaObstaculos))
            {
                tieneLineaDeVision = true;
            }
        }

        // --- 2. CAMBIOS DE ESTADO ---
        if (tieneLineaDeVision)
        {
            if (!jugadorDetectado)
            {
                jugadorDetectado = true;
                StopAllCoroutines(); 
                estaGirando = false; 
            }
        }
        else
        {
            if (jugadorDetectado)
            {
                jugadorDetectado = false;
            }
        }

        // --- 3. MOVIMIENTO (SÓLO ANIMACIONES) ---
        if (jugadorDetectado)
        {
            // MODO PERSECUCIÓN
            Vector3 posicionObjetivo = new Vector3(player.transform.position.x, transform.position.y, player.transform.position.z);
            Vector3 direccionAlObjetivo = (posicionObjetivo - transform.position).normalized;

            // NOTA: Dejo el 'Slerp' activo SOLO aquí. Las animaciones de caminar recto no giran 
            // solas hacia objetivos móviles, así que necesitamos ayudarle a apuntar hacia ti.
            if (direccionAlObjetivo != Vector3.zero)
            {
                Quaternion rotacionDeseada = Quaternion.LookRotation(direccionAlObjetivo);
                transform.rotation = Quaternion.Slerp(transform.rotation, rotacionDeseada, 10f * Time.deltaTime);
            }

            // FRENOS: Si está lejos, camina. Si está cerca, se detiene.
            if (distanciaAlJugador > distanciaDeAtaque)
            {
                animator.SetInteger("Walking", 2);
            }
            else if(!estaAtacando)
            {
                
                animator.SetInteger("Walking", 0);
                StartCoroutine(AttackRigPuppet());
                estaAtacando = true;
            }
        }
        else
        {
            // MODO PATRULLA
            if (!estaGirando)
            {
                animator.SetInteger("Walking", 2); 
                
                Debug.DrawRay(new Vector3 (transform.position.x,transform.position.y + 1f,transform.position.z) , transform.forward * 1.5f, Color.green);

                if (Physics.Raycast(new Vector3 (transform.position.x,transform.position.y + 1f,transform.position.z), transform.forward, 1.5f))
                {
                    StartCoroutine(GirarHastaDespejar());
                }
                else
                {
                    if (Random.Range(0f, 100f) < (10f * Time.deltaTime)) 
                    {
                        StartCoroutine(GirarHastaDespejar());
                    }
                }
            }
        }
    }
    IEnumerator AttackRigPuppet()
    {
        bool estaEfecto = false;
        animator.SetBool("isAttacking",true);
        areaAtaqueRagPuppet.SetActive(true);
        yield return new WaitForSeconds(tiempoAtaqueRagPuppet * 1.55f); 
        if (estaEfecto == false)
        {
            Instantiate(EffectoDeAtaque, tranformDeEfecto.transform.position, tranformDeEfecto.transform.rotation);
            estaEfecto = true;
        }    
        yield return new WaitForSeconds(tiempoAtaqueRagPuppet/1.2f); 
        areaAtaqueRagPuppet.SetActive(false);
        animator.SetBool("isAttacking",false);
        estaAtacando = false;
        animator.SetInteger("Walking", 2); 
        

    }
    IEnumerator Despertandose()
    {
        yield return new WaitForSeconds(1.5f); 
        despertarse = false;
    }

    IEnumerator GirarHastaDespejar()
    {
        estaGirando = true;
        
        // Elegimos dirección de giro aleatoria
        float direccionGiro = Random.Range(0, 2) == 0 ? 1f : -1f;

        // Le ponemos la animación de mover las piernas hacia los lados
        if (direccionGiro == 1f)
        {
             animator.SetInteger("Walking", 3); 
        }
        else if (direccionGiro == -1f)
        {
            animator.SetInteger("Walking", 1); 
        }

        // FASE 1: Giramos POR CÓDIGO hasta dejar de mirar a la pared
        // El Root Motion moverá los pies, pero nosotros controlamos el giro exacto
        while (Physics.Raycast(transform.position, transform.forward, 3f, capaObstaculos))
        {
            transform.Rotate(0, direccionGiro * velocidadDeGiro * Time.deltaTime, 0);
            yield return null; 
        }

        // FASE 2: Margen de seguridad extra (giramos un poquito más para no rozar)
        float tiempoExtra = 0.5f; 
        while (tiempoExtra > 0)
        {
            transform.Rotate(0, direccionGiro * velocidadDeGiro * Time.deltaTime, 0);
            tiempoExtra -= Time.deltaTime;
            yield return null;
        }

        // Volvemos a la animación de caminar recto
        animator.SetInteger("Walking", 2); 
        estaGirando = false;
    }

    protected override void RecibirDaño()
    {
        Debug.Log("¡Un enemigo ha sido tocado por la espada!");

        base.RecibirDaño();
        animator.SetInteger("Walking", 0); 

        animator.SetBool("isAttacking", false); // Apagamos la animación de ataque
        if (areaAtaqueRagPuppet != null) areaAtaqueRagPuppet.SetActive(false); // Apagamos el área de daño
        estaAtacando = false; // ¡ABRIMOS EL CANDADO A LA FUERZA!
        despertarse = false;

        estaGolpeado = true;
        estaGirando = false; 
    }

    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);
        
        if (other.CompareTag("Player"))
        {
            if (other.transform.position.y > transform.position.y + 3f) return; 

            if (other.TryGetComponent(out MainCharacterMovement playerScriptComponent))
            {
                Vector3 direccionGolpe = (other.transform.position - transform.position).normalized;
                direccionGolpe.y = 0;
                playerScriptComponent.RecibirDaño(fuerzaDeMiGolpe, direccionGolpe);
            }
        }
    }
}