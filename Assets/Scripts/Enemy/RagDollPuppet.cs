using UnityEngine;
using System.Collections;

public class RagDollPuppet : EnemyScript
{
    [Header("Ajustes de Visión")]
    public float rangoDeVision = 10f;
    public float anguloDeVision = 65f;
    public LayerMask capaObstaculos;
    
    [Header("Ajustes de Movimiento")]
    public float velocity = 3f;
    public float velocityCaminando = 1f;
    public float tiempoDeGiro = 1f; 
    public float velocidadDeGiro = 45f; 
    public float distanciaDeAtaque = 1.5f; // NUEVO: Distancia a la que se frena para no fusionarse
    
    [Header("Ajustes de Daño al Jugador")]
    public float fuerzaDeMiGolpe = 7f;

    private bool estaGirando = false;
    private bool jugadorDetectado = false;
    private bool Golpeado = false; 

    protected override void Start()
    {
        base.Start();
        vida = 100;
    }

    protected override void Moverse()
    {
        if (animator != null && !animator.applyRootMotion) return;
        rb.isKinematic = false; 

        if (player == null) return;
        
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
                Debug.Log("¡TE VEO! Empezando persecución.");
                StopAllCoroutines(); 
                estaGirando = false; 
            }
        }
        else
        {
            if (jugadorDetectado)
            {
                jugadorDetectado = false;
                Debug.Log("Te perdí...");
            }
            else if (Golpeado)
            {
                jugadorDetectado = true; 
                Golpeado = false; 
            }
        }

        // --- 3. MOVIMIENTO MEJORADO ---
        if (jugadorDetectado)
        {
            // MODO PERSECUCIÓN
            Vector3 posicionObjetivo = new Vector3(player.transform.position.x, transform.position.y, player.transform.position.z);
            Vector3 direccionAlObjetivo = (posicionObjetivo - transform.position).normalized;

            if (direccionAlObjetivo != Vector3.zero)
            {
                Quaternion rotacionDeseada = Quaternion.LookRotation(direccionAlObjetivo);
                transform.rotation = Quaternion.Slerp(transform.rotation, rotacionDeseada, 10f * Time.deltaTime);
            }

            // FRENOS: Si está lejos, camina. Si está cerca, se detiene para pegarte.
            if (distanciaAlJugador > distanciaDeAtaque)
            {
                if (animator != null) animator.SetBool("isWalking", true);
            }
            else
            {
                if (animator != null) animator.SetBool("isWalking", false);
                // Aquí podrías activar un animator.SetTrigger("Attack") en el futuro
            }
        }
        else
        {
            // MODO PATRULLA
            if (!estaGirando)
            {
                if (animator != null) animator.SetBool("isWalking", true);
                
                Debug.DrawRay(transform.position, transform.forward * 1.5f, Color.green);

                if (Physics.Raycast(transform.position, transform.forward, 1.5f))
                {
                    StartCoroutine(GirarHastaDespejar());
                }
                else
                {
                    // Probabilidad fija basada en tiempo real (ej. 10% de girar cada segundo)
                    if (Random.Range(0f, 100f) < (10f * Time.deltaTime)) 
                    {
                        StartCoroutine(GirarHastaDespejar());
                    }
                }
            }
            else
            {
                if (animator != null) animator.SetBool("isWalking", false);
            }
        }
    }

    IEnumerator GirarHastaDespejar()
    {
        estaGirando = true;
        float direccionGiro = Random.Range(0, 2) == 0 ? 1f : -1f;

        // FASE 1: Esquivar el obstáculo
        while (Physics.Raycast(transform.position, transform.forward, 3f, capaObstaculos))
        {
            // Giramos
            transform.Rotate(0, direccionGiro * velocidadDeGiro * Time.deltaTime, 0);
            
            // Nos movemos hacia adelante respetando las paredes usando MovePosition
            Vector3 avance = transform.forward * (velocityCaminando * 0.5f) * Time.deltaTime;
            rb.MovePosition(rb.position + avance);
            
            yield return null; 
        }

        // FASE 2: Margen de seguridad (curva suave aleatoria)
        float gradosObjetivo = Random.Range(25f, 100f);
        float gradosGirados = 0f; 

        while (gradosGirados < gradosObjetivo)
        {
            float giroActual = velocidadDeGiro * Time.deltaTime;
            transform.Rotate(0, direccionGiro * giroActual, 0);
            
            Vector3 avance = transform.forward * (velocityCaminando * 0.5f) * Time.deltaTime;
            rb.MovePosition(rb.position + avance);
            
            gradosGirados += giroActual;
            yield return null;
        }

        estaGirando = false;
    }

    protected override void RecibirDaño()
    {
        animator.SetBool("isWalking", false); 
        vida -= PlayerScript.attackDamage;
        Golpeado = true;
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