using UnityEngine;

public class BailarinaScript : EnemyScript
{
    [Header("Ajustes de Visión")]
    public float rangoDeVision = 10f;
    public float anguloDeVision = 65f;
    public LayerMask capaObstaculos;
    
    [Header("Ajustes de Movimiento")]
    public float velocity = 3f;
    public float distanciaDeAtaque = 1.5f; // Distancia a la que se frena al atraparte
    
    [Header("Ajustes de Daño al Jugador")]
    public float fuerzaDeMiGolpe = 7f;

    private bool jugadorDetectado = false;
    private bool Golpeado = false; 

    protected override void Start()
    {
        base.Start();
        vida = 100;
        
        if (rb != null)
        {
            rb.isKinematic = false;
        }
    }

    protected override void Moverse()
    {
        if (player == null || rb == null) return;
        
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

        // --- 3. MOVIMIENTO FÍSICO (RIGIDBODY) ---
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

            // FRENOS: Si está lejos, corre. Si está cerca, frena en seco.
            if (distanciaAlJugador > distanciaDeAtaque)
            {
                rb.linearVelocity = new Vector3(transform.forward.x * velocity, rb.linearVelocity.y, transform.forward.z * velocity);
            }
            else
            {
                rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            }
        }
        else
        {
            // MODO IDLE / ESPERA
            // Cuando no te detecta, se queda quieta en el sitio conservando la gravedad
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            
            // ---> AQUÍ PODRÁS PONER TU NUEVA LÓGICA EN EL FUTURO <---
        }
    }
}