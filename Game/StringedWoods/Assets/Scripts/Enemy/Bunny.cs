using UnityEngine;
using System.Collections;
public class Bunny : EnemyScript
{
    [Header("Ajustes de Visión")]
    public float rangoDeVision = 10f;
    public float anguloDeVision = 65f;
    public LayerMask capaObstaculos;
    private float moveX ;
    private float moveZ ;
    private Vector3 movimiento;
    public float velocity = 3f;
    public bool despertarse = true;

    private bool jugadorDetectado = false;
    private bool Golpeado = false; 
    private bool estaGirando = false; 

    // ¡CORREGIDO! Ahora es un float para que se pueda comparar con la distancia
    [Header("Ajustes de Kiting (Huir)")]
    public float distanciaDePrecaucion = 5f; 
    public bool conejoAreo;

    [Header("Ajustes de Disparo")]
    public GameObject bullet;
    public GameObject salidaBala;
    public float cadenciaDeDisparo = 0.5f; // Cada cuántos segundos dispara
    private float temporizadorDisparo = 0f; // Nuestro cronómetro interno

    protected override void Start()
    {
        base.Start();
        vida = 60;
        animator.SetTrigger("WakeUp"); 
        StartCoroutine(Despertandose());
    }
        IEnumerator Despertandose()
    {
        yield return new WaitForSeconds(1.5f); 
        despertarse = false;
    }
    protected virtual void Update()
    {
        base.Update();
        
        // Un pequeño seguro de vida por si el jugador desaparece
        if (player != null && salidaBala != null)
        {
            salidaBala.transform.LookAt(player.transform);
        }
        
        // Raycast hacia el suelo para saber si está pisando una Elevación / Caja
        if (Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, out RaycastHit hit, 0.5f) && hit.collider.CompareTag("Elevaciones"))
        {
            Debug.Log("¡Estoy encima de una caja!");
            conejoAreo = false;
        }
    }

    protected override void Moverse()
    {
        if (player == null || despertarse) return;
        
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
                animator.SetBool("isAttacking",false);
                animator.SetBool("isWalking",false);
                jugadorDetectado = true;
                Debug.Log("¡TE VEO! Empezando disparar.");
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

        if (jugadorDetectado)
        {
            animator.SetBool("isWalking",false);
            // MODO PERSECUCIÓN
            Vector3 posicionObjetivo = new Vector3(player.transform.position.x, transform.position.y, player.transform.position.z);
            Vector3 direccionAlObjetivo = (posicionObjetivo - transform.position).normalized;
    
            // --- SISTEMA DE DISPARO (Cronómetro) ---
            temporizadorDisparo += Time.deltaTime; 
            
            // Rotación suave hacia el jugador (SIEMPRE LO MIRA)
            if (direccionAlObjetivo != Vector3.zero)
            {
                Quaternion rotacionDeseada = Quaternion.LookRotation(direccionAlObjetivo);
                transform.rotation = Quaternion.Slerp(transform.rotation, rotacionDeseada, 10f * Time.deltaTime);
            }

            // --- LÓGICA DE DISTANCIAS Y EVASIÓN ---
            if (conejoAreo)
            {
                // 1. FRENAMOS AL CONEJO (Ya está a salvo, que deje de moverse)
                rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);

                // 2. Lógica de Disparo
                if (temporizadorDisparo >= cadenciaDeDisparo)
                {
                    animator.SetBool("isAttacking",true);
                    Instantiate(bullet, salidaBala.transform.position, salidaBala.transform.rotation);
                    temporizadorDisparo = 0f; 
                }
            }

            else if (distanciaAlJugador > distanciaDePrecaucion )
            {
                // 1. FRENAMOS AL CONEJO (Ya está a salvo, que deje de moverse)
                rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);

                // 2. Lógica de Disparo
                if (temporizadorDisparo >= cadenciaDeDisparo)
                {
                    animator.SetBool("isAttacking",true);
                    Instantiate(bullet, salidaBala.transform.position, salidaBala.transform.rotation);
                    temporizadorDisparo = 0f; 
                }
            }
            else
            {
                if(conejoAreo) {return;}
                animator.SetBool("isAttacking",false);
                animator.SetBool("isWalking",true);
                // ===========================================
                // --- MODO: HUIR HACIA ATRÁS CON EVASIÓN ---
                // ===========================================
                
                temporizadorDisparo = 0f; 
                
                // 1. Configuramos el Rayo de detección de pared trasera
                Vector3 origenRayoPared = transform.position + Vector3.up * 1f; 
                Vector3 direccionRayoPared = -transform.forward; 
                float distanciaDeteccionPared = 2f; // Detectamos la pared 2 metros antes

                Debug.DrawRay(origenRayoPared, direccionRayoPared * distanciaDeteccionPared, Color.green);

                // 2. Lanzamos el Raycast a la capa de obstáculos
                if (Physics.Raycast(origenRayoPared, direccionRayoPared, out RaycastHit hitPared, distanciaDeteccionPared, capaObstaculos))
                {
                    // ¡PARED DETECTADA DETRÁS! Mostramos rayo rojo
                    Debug.DrawRay(origenRayoPared, direccionRayoPared * hitPared.distance, Color.red);
                    
                    // --- CÁLCULO DE DESVÍO LIGERAMENTE LATERAL ---
                    Vector3 normalPared = hitPared.normal;
                    Vector3 direccionDesvio = Vector3.Cross(normalPared, Vector3.up);

                    // Seguro matemático por si se alinean los vectores a cero
                    if (direccionDesvio == Vector3.zero) direccionDesvio = transform.right;

                    // Mezclamos para resbalar por la pared
                    Vector3 direccionFinalHuida = (direccionDesvio + normalPared).normalized;

                    // Aplicamos velocidad lateral
                    rb.linearVelocity = new Vector3(direccionFinalHuida.x * velocity, rb.linearVelocity.y, direccionFinalHuida.z * velocity);
                }
                else
                {
                    // --- CAMINO DESPEJADO: HUIDA RECTA ---
                    // Le aplicamos velocidad pura hacia atrás (-transform.forward)
                    rb.linearVelocity = new Vector3(-transform.forward.x * velocity, rb.linearVelocity.y, -transform.forward.z * velocity);
                }
            }
        }
        else 
        {
            // Opcional pero recomendado: si pierde de vista al jugador, reseteamos el cronómetro.
            temporizadorDisparo = 0f; 
        }
    }

    protected override void RecibirDaño()
    {
        Debug.Log("¡Un enemigo ha sido tocado por la espada!");
        base.RecibirDaño();
        temporizadorDisparo = 0f; 
    }
}