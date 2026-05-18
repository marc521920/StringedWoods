using UnityEngine;
using System.Collections;

public class RagDollPuppet : EnemyScript
{
    [Header("Ajustes de Visión")]
    public float rangoDeVision = 10f;
    public float anguloDeVision = 90f;
    public LayerMask capaObstaculos;
    public float alturaMaxima;
    
    [Header("Ajustes de Movimiento")]
    public float distanciaDeAtaque = 1.5f; 
    
    [Header("Ajustes de Daño al Jugador")]
    public float fuerzaDeMiGolpe = 7f;
    public float cooldownDeAtaque = 3f; // NUEVO: Tiempo entre ataques
    private float tiempoUltimoAtaque = -10f; // NUEVO: Reloj interno

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
        rb.isKinematic = false; 

        if (player == null || despertarse || estaAtacando) return;
        if (estaGolpeado || estaMuerto) return; 
        
        float distanciaAlJugador = Vector3.Distance(transform.position, player.transform.position);
        Vector3 direccionAlJugador = (player.transform.position - transform.position).normalized;

        bool enRango = distanciaAlJugador <= rangoDeVision;
        float anguloAlJugador = Vector3.Angle(transform.forward, direccionAlJugador);
        bool enAngulo = anguloAlJugador <= anguloDeVision;

        // --- NUEVO: Límite de altura ---
        // Calculamos la diferencia en el eje Y. 
        // Si el jugador está 2 metros (o más) por encima del enemigo, será true.
        float diferenciaAltura = player.transform.position.y - transform.position.y;
        bool demasiadoAlto = diferenciaAltura > alturaMaxima; // Puedes cambiar este 2f por el valor que quieras

        bool tieneLineaDeVision = false;
        
        // Añadimos "!demasiadoAlto" a las condiciones para que solo lo vea si NO está muy arriba
        if (enRango && enAngulo && !demasiadoAlto)
        {
            if (!Physics.Raycast(transform.position, direccionAlJugador, distanciaAlJugador, capaObstaculos))
            {
                tieneLineaDeVision = true;
            }
        }

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

        if (jugadorDetectado)
        {
            Vector3 posicionObjetivo = new Vector3(player.transform.position.x, transform.position.y, player.transform.position.z);
            Vector3 direccionAlObjetivo = (posicionObjetivo - transform.position).normalized;

            if (direccionAlObjetivo != Vector3.zero)
            {
                Quaternion rotacionDeseada = Quaternion.LookRotation(direccionAlObjetivo);
                transform.rotation = Quaternion.Slerp(transform.rotation, rotacionDeseada, 10f * Time.deltaTime);
            }

            // --- LÓGICA DE FRENOS Y COOLDOWN ACTUALIZADA ---
            if (distanciaAlJugador > distanciaDeAtaque)
            {
                animator.SetInteger("Walking", 2);
            }
            else 
            {
               
                
                if(!estaAtacando && Time.time >= tiempoUltimoAtaque + cooldownDeAtaque)
                {
                    animator.SetInteger("Walking", 0); // Siempre se frena al estar cerca
                    StartCoroutine(AttackRigPuppet());
                    estaAtacando = true;
                    tiempoUltimoAtaque = Time.time; // Reiniciamos el cronómetro
                }
            }
        }
        else
        {
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
        Debug.Log("rarete");
        bool estaEfecto = false;
        animator.SetBool("isAttacking",true);
        
        yield return new WaitForSeconds(tiempoAtaqueRagPuppet * 1.55f); 
        if (estaEfecto == false)
        {
            Instantiate(EffectoDeAtaque, tranformDeEfecto.transform.position, tranformDeEfecto.transform.rotation);
            estaEfecto = true;
        }    
        yield return new WaitForSeconds(tiempoAtaqueRagPuppet/1.2f); 
        animator.SetBool("isAttacking",false);
        estaAtacando = false;
        animator.SetInteger("Walking", 2); 
    }

    IEnumerator Despertandose()
    {
        yield return new WaitForSeconds(1.5f); 
        despertarse = false;
        areaAtaqueRagPuppet.SetActive(true);
    }

    IEnumerator GirarHastaDespejar()
    {
        estaGirando = true;
        float direccionGiro = Random.Range(0, 2) == 0 ? 1f : -1f;

        if (direccionGiro == 1f) animator.SetInteger("Walking", 3); 
        else if (direccionGiro == -1f) animator.SetInteger("Walking", 1); 

        while (Physics.Raycast(transform.position, transform.forward, 3f, capaObstaculos))
        {
            transform.Rotate(0, direccionGiro * velocidadDeGiro * Time.deltaTime, 0);
            yield return null; 
        }

        float tiempoExtra = 0.5f; 
        while (tiempoExtra > 0)
        {
            transform.Rotate(0, direccionGiro * velocidadDeGiro * Time.deltaTime, 0);
            tiempoExtra -= Time.deltaTime;
            yield return null;
        }

        animator.SetInteger("Walking", 2); 
        estaGirando = false;
    }

    protected override void RecibirDaño()
    {
        Debug.Log("¡Un enemigo ha sido tocado por la espada!");

        base.RecibirDaño();
        animator.SetInteger("Walking", 0); 

        animator.SetBool("isAttacking", false); 
        if (areaAtaqueRagPuppet != null) areaAtaqueRagPuppet.SetActive(false); 
        estaAtacando = false; 
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