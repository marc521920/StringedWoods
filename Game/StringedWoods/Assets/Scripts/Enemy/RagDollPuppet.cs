using UnityEngine;
using System.Collections;
public class RagDollPuppet : EnemyScript
{
    [Header("Ajustes de Visión")]
    public float rangoDeVision = 10f;
    public float anguloDeVision = 65f;
    public LayerMask capaObstaculos;
    public float velocity = 3f;
    public float velocityCaminando = 1f;
    float rotacionInicial; 
    float rotacionFinal;
    private bool estaGirando = false;
    public float tiempoDeGiro = 1f; // Duración del giro en segundos
    
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
                // animacion de que se quede mirando donde estabas
                // animacion de que lo ha perdido
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
        else
        {
           

            int valorAleatorio = Random.Range(0, 1000);
            rb.isKinematic = false; // Aseguramos que el enemigo no sea afectado por la física mientras se mueve
            rb.linearVelocity = transform.forward  * velocityCaminando;
            Debug.DrawRay(transform.position, transform.forward * 3f, Color.green);
            if (valorAleatorio < 2) // 30% de probabilidad de cambiar de dirección
            {
                StartCoroutine(GirarProgresivamente());
            }
            if (!Physics.Raycast(transform.position, transform.forward, 3f))
            {

            }
            else
            {
                 StartCoroutine(GirarProgresivamente());
            }
            // Aquí podrías agregar lógica para patrullar o quedarse quieto, dependiendo de tu diseño de juego
        }
    }

    IEnumerator GirarProgresivamente()
    {

        estaGirando = true;

        // 1. Calculamos el ángulo aleatorio que querías (entre 90 y 180)
        float gradosAleatorios = Random.Range(-90f, 90f);

        // 2. Guardamos dónde estamos AHORA
        Quaternion rotacionInicial = transform.rotation;
        
        // 3. Calculamos dónde queremos TERMINAR
        // (Multiplicar Quaternions en Unity equivale a "sumar" rotaciones)
        Quaternion rotacionFinal = rotacionInicial * Quaternion.Euler(0, gradosAleatorios, 0);

        float tiempoPasado = 0f;

        // 4. El bucle mágico: MIENTRAS no hayamos consumido todo el tiempo...
        while (tiempoPasado < tiempoDeGiro)
        {
            tiempoPasado += Time.deltaTime; // Sumamos el tiempo de este frame
            
            // Calculamos el porcentaje del viaje (de 0.0 a 1.0)
            float porcentaje = tiempoPasado / tiempoDeGiro;

            // Slerp hace la transición suave entre la rotación inicial y la final
            transform.rotation = Quaternion.Slerp(rotacionInicial, rotacionFinal, porcentaje);

            yield return null; // Pausamos hasta el siguiente frame
        }

        // 5. Por si acaso los decimales fallan, aseguramos que quede EXACTAMENTE en el ángulo final
        transform.rotation = rotacionFinal;
        if (Physics.Raycast(transform.position, transform.forward, 3f))
        {
            Quaternion rotacionInicial = transform.rotation;
        
            // 3. Calculamos dónde queremos TERMINAR
            // (Multiplicar Quaternions en Unity equivale a "sumar" rotaciones)
            Quaternion rotacionFinal = rotacionInicial * Quaternion.Euler(0, gradosAleatorios, 0);

            float tiempoPasado = 0f;

            // 4. El bucle mágico: MIENTRAS no hayamos consumido todo el tiempo...
            while (tiempoPasado < tiempoDeGiro)
            {
            tiempoPasado += Time.deltaTime; // Sumamos el tiempo de este frame
            
            // Calculamos el porcentaje del viaje (de 0.0 a 1.0)
            float porcentaje = tiempoPasado / tiempoDeGiro;

            // Slerp hace la transición suave entre la rotación inicial y la final
            transform.rotation = Quaternion.Slerp(rotacionInicial, rotacionFinal, porcentaje);

            yield return null; // Pausamos hasta el siguiente frame
            }

            
        }
        
            estaGirando = false;
    }

    // Mantén tu OnDrawGizmos igual, es perfecto para debugear
    private void OnDrawGizmos() { /* ... tu código anterior ... */ }
}