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
    public float velocidadDeGiro = 45f; // Grados por segundo
    
    private bool estaGirando = false;
    private bool jugadorDetectado = false;
    private bool Golpeado = false; // Nueva variable para rastrear si el enemigo ha sido golpeado

    

    [Header("Ajustes de Daño al Jugador")]
    public float fuerzaDeMiGolpe = 7f;

    // (He borrado 'float rotacionInicial' de aquí arriba porque creaba conflictos con la corrutina)
    protected override void Start()
    {
        
        base.Start();
        // Aquí podrías agregar cualquier inicialización adicional específica para el RagDollPuppet
        vida = 100;
        
    }
protected override void Moverse()
    {
        if (animator != null && !animator.applyRootMotion) return;
        rb.isKinematic = false; 
        
        // 1. Calculamos la distancia y dirección
        float distanciaAlJugador = Vector3.Distance(transform.position, player.transform.position);
        Vector3 direccionAlJugador = (player.transform.position - transform.position).normalized;

        // --- LÓGICA DE DETECCIÓN ---
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

        // --- CAMBIO DE ESTADO ---
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

        // --- LÓGICA DE MOVIMIENTO ---
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

            // B) Activamos la animación en lugar de moverlo por código
            if (animator != null) 
            {
                animator.SetBool("isWalking", true);
            }
        }
        else
        {
            // MODO PATRULLA
            if (!estaGirando)
            {
                // Activamos la animación de caminar
                if (animator != null) 
                {
                    animator.SetBool("isWalking", true);
                }
                
                Debug.DrawRay(transform.position, transform.forward * 1.5f, Color.green);

                if (Physics.Raycast(transform.position, transform.forward, 1.5f))
                {
                    StartCoroutine(GirarHastaDespejar());
                }
                else
                {
                    int valorAleatorio = Random.Range(0, 1000);
                    if (valorAleatorio < 2) 
                    {
                        StartCoroutine(GirarHastaDespejar());
                    }
                }
            }
            else
            {
                // Si el enemigo está quieto girando (en la corrutina), apagamos la animación de caminar
                if (animator != null) 
                {
                    animator.SetBool("isWalking", false);
                }
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
            transform.Rotate(0, direccionGiro * velocidadDeGiro * Time.deltaTime, 0);
            transform.position += transform.forward * (velocityCaminando * 0.5f) * Time.deltaTime;
            yield return null; 
        }

        // FASE 2: Margen de seguridad (Ahora con aleatoriedad)
        // Calculamos el ángulo extra aleatorio para que no sea predecible
        float gradosObjetivo = Random.Range(25f, 100f);
        float gradosGirados = 0f; // Llevamos la cuenta de cuánto hemos girado en esta fase

        // Mientras no hayamos alcanzado nuestro objetivo aleatorio...
        while (gradosGirados < gradosObjetivo)
        {
            float giroActual = velocidadDeGiro * Time.deltaTime;
            transform.Rotate(0, direccionGiro * giroActual, 0);
            
            // Sigue completando la curva suave
            transform.position += transform.forward * (velocityCaminando * 0.5f) * Time.deltaTime;
            
            gradosGirados += giroActual;
            yield return null;
        }

        estaGirando = false;
    }
    protected override void RecibirDaño()
    {
        vida = vida - PlayerScript.attackDamage;
        Golpeado = true;
    }
    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);
        
        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("¡Me chocaste! Intentando hacer daño al jugador...");
            MainCharacterMovement PlayerScript = other.gameObject.GetComponent<MainCharacterMovement>();
            // Si estás por encima de su cabeza rebotando, el enemigo ignora el choque y no te pega
            if (other.transform.position.y > transform.position.y + 1.5f)
            {
                return; 
            }
            // 1. Buscamos tu script en el jugador (cambia 'MainCharacterMovement' por el nombre de tu script si es otro)
            

            // Si lo hemos encontrado...
            if (PlayerScript != null)
            {
                
                // 2. Calculamos la dirección del golpe: Desde MÍ (Enemigo) hacia el JUGADOR
                Vector3 direccionGolpe = (other.transform.position - transform.position).normalized;
                
                // Anulamos la Y para que no lo mande a volar al espacio
                direccionGolpe.y = 0;

                // 3. ¡Le damos la orden al jugador de que reciba el daño y salga volando!
                PlayerScript.RecibirDaño(fuerzaDeMiGolpe, direccionGolpe);
            }

            // (Aquí abajo ya iría el código normal donde el enemigo te hace daño si te choca de frente)
        }
        // Si con lo que me acabo de chocar es el Jugador...

    }

}