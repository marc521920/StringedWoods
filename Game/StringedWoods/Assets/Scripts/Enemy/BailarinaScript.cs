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
    public float velocityCaminando = 5f;
    
    [Header("Ajustes de Daño al Jugador")]
    public float fuerzaDeMiGolpe = 7f;

    [Header("Ajustes de Patrulla")]
    public float distanciaAntenas = 1.5f; // A qué distancia detecta la pared para rebotar
    private Vector3 direccionPatrulla = Vector3.zero; // Aquí guardamos la flecha de movimiento
    public GameObject prefabEfectoRebote; // Prefab del efecto visual al rebotar
    public GameObject posicionRebote; // Prefab del efecto visual al golpear al jugador

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
            // --- MODO PATRULLA REBOTANTE ---
            if (direccionPatrulla == Vector3.zero)
            {
                float anguloAleatorio = Random.Range(-90f, 90f);
                direccionPatrulla = Quaternion.Euler(0, anguloAleatorio, 0) * transform.forward;
                direccionPatrulla.Normalize();
            }

            // 1. Movimiento ciego en la dirección del rebote
            rb.linearVelocity = new Vector3(direccionPatrulla.x * velocityCaminando, rb.linearVelocity.y, direccionPatrulla.z * velocityCaminando);

            // 2. ¡ROTACIÓN ANTI-BUGEOS SOLO AQUÍ!
            // Obligamos físicamente a que la rotación se adapte a su dirección de patrulla
            if (direccionPatrulla != Vector3.zero)
            {
                Quaternion rotacionDeseada = Quaternion.LookRotation(direccionPatrulla);
                rb.MoveRotation(Quaternion.Slerp(transform.rotation, rotacionDeseada, 10f * Time.deltaTime));
            }

            // 3. Sistema de Antenas (El rayo nace desde más arriba)
            Vector3 origenDelRayo = transform.position + Vector3.up; 
            Debug.DrawRay(origenDelRayo, direccionPatrulla * distanciaAntenas, Color.blue); 

            if (Physics.Raycast(origenDelRayo, direccionPatrulla, out RaycastHit hit, distanciaAntenas, capaObstaculos))
            {
                // 1. Instanciar la partícula
                if (prefabEfectoRebote != null && posicionRebote != null)
                {
                    Instantiate(prefabEfectoRebote, posicionRebote.transform.position, transform.rotation);
                }

                // 2. Calcular la nueva dirección de escape
                direccionPatrulla = Vector3.Reflect(direccionPatrulla, hit.normal);
                direccionPatrulla.y = 0;
                direccionPatrulla.Normalize();
                
                // --- 3. EL REBOTE FÍSICO ---
                float fuerzaSalto = 2.5f;   // Cuánto se levanta del suelo (eje Y)
                float fuerzaEmpuje = 7f;  // Con cuánta fuerza sale escopetada en la nueva dirección (ejes X, Z)

                // Frenamos la caída un microsegundo para que el salto sea siempre limpio
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

                // Mezclamos un empuje hacia arriba + un empuje hacia la nueva dirección
                Vector3 impulso = (Vector3.up * fuerzaSalto) + (direccionPatrulla * fuerzaEmpuje);
                
                rb.AddForce(impulso, ForceMode.Impulse);
            }
        }
    }
}