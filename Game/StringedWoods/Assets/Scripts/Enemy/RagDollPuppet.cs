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

    protected override void Moverse()
    {
        rb.isKinematic = false; // El enemigo no se ve afectado por la física mientras patrulla o persigue
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
                
                // TRUCO PRO: Si estaba girando en su patrulla, cancelamos el giro de golpe para que te persiga
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
                // Si no ve al jugador y no está girando, tiene una pequeña probabilidad de empezar a girar para patrullar
                jugadorDetectado = true; // Aseguramos que no esté en modo persecución
                Golpeado = false; // Reseteamos el estado de golpeado para que pueda patrullar normalmente
            }
        }

        // --- LÓGICA DE MOVIMIENTO ---
        if (jugadorDetectado)
        {
            // MODO PERSECUCIÓN
            Vector3 posicionObjetivo = new Vector3(player.transform.position.x, transform.position.y, player.transform.position.z);
            transform.LookAt(posicionObjetivo);
            transform.position = Vector3.MoveTowards(transform.position, posicionObjetivo, velocity * Time.deltaTime);
        }
        else
        {
            // MODO PATRULLA
            
            // Regla de oro: SOLO caminamos y pensamos si NO estamos en medio de un giro
            if (!estaGirando)
            {
                // Usamos transform.position en lugar de linearVelocity para evitar conflictos físicos raros
                transform.position += transform.forward * velocityCaminando * Time.deltaTime;
                
                Debug.DrawRay(transform.position, transform.forward * 1.5f, Color.green);

                // 1. Si detecta pared -> Gira
                if (Physics.Raycast(transform.position, transform.forward, 1.5f))
                {
                    StartCoroutine(GirarHastaDespejar());
                }
                // 2. Si no hay pared, tiene un 0.2% de probabilidad de girar porque sí (patrulla aleatoria)
                else
                {
                    int valorAleatorio = Random.Range(0, 1000);
                    if (valorAleatorio < 2) 
                    {
                        StartCoroutine(GirarHastaDespejar());
                    }
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
        Golpeado = true;
    }
    void OnCollisionEnter(Collision collision)
    {
        // Si con lo que me acabo de chocar es el Jugador...
        if (collision.gameObject.CompareTag("Player"))
        {
            // 1. Buscamos tu script en el jugador (cambia 'MainCharacterMovement' por el nombre de tu script si es otro)
            MainCharacterMovement PlayerScript = collision.gameObject.GetComponent<MainCharacterMovement>();

            // Si lo hemos encontrado...
            if (PlayerScript != null)
            {
                // 2. Calculamos la dirección del golpe: Desde MÍ (Enemigo) hacia el JUGADOR
                Vector3 direccionGolpe = (collision.transform.position - transform.position).normalized;
                
                // Anulamos la Y para que no lo mande a volar al espacio
                direccionGolpe.y = 0;

                // 3. ¡Le damos la orden al jugador de que reciba el daño y salga volando!
                PlayerScript.RecibirDaño(fuerzaDeMiGolpe, direccionGolpe);
            }
        }
    }
}