using UnityEngine;

public class RagDollPuppet : EnemyScript
{
    [Header("Ajustes de Visión")]
    public float rangoDeVision = 10f;
    public float anguloDeVision = 65f;
    public LayerMask capaObstaculos;
    public float velocity = 3f;
    
    private bool jugadorDetectado = false;

    protected override void Moverse()
    {
        // 1. Calculamos la distancia y dirección
        float distanciaAlJugador = Vector3.Distance(transform.position, player.transform.position);
        Vector3 direccionAlJugador = (player.transform.position - transform.position).normalized;

        // --- LÓGICA DE DETECCIÓN ---
        bool enRango = distanciaAlJugador <= rangoDeVision;
        float anguloAlJugador = Vector3.Angle(transform.forward, direccionAlJugador);
        bool enAngulo = anguloAlJugador <= anguloDeVision;

        // Raycast para ver si hay paredes
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
            }
        }
        else
        {
            if (jugadorDetectado)
            {
                rb.isKinematic = true; // Aseguramos que el enemigo no sea afectado por la física mientras se mueve
                jugadorDetectado = false;
                Debug.Log("Te perdí...");
            }
        }

        // --- LÓGICA DE PERSECUCIÓN ---
        if (jugadorDetectado)
        {
            rb.isKinematic = false; // Aseguramos que el enemigo no sea afectado por la física mientras se mueve
            // A) Rotar hacia el jugador (mirarlo)
            // Creamos una rotación que ignore la altura (Y) para que el enemigo no se incline hacia arriba/abajo
            Vector3 posicionObjetivo = new Vector3(player.transform.position.x, transform.position.y, player.transform.position.z);
            transform.LookAt(posicionObjetivo);

            // B) Moverse hacia el jugador
            // Usamos la velocidad que heredas de EnemyScript
            transform.position = Vector3.MoveTowards(transform.position, posicionObjetivo, velocity * Time.deltaTime);
        }
    }

    // Mantén tu OnDrawGizmos igual, es perfecto para debugear
    private void OnDrawGizmos() { /* ... tu código anterior ... */ }
}