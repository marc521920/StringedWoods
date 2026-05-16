using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
    public float velocityCarrera = 20f;
    
    [Header("Ajustes de Ataque")]
    public float fuerzaDeMiGolpe = 7f;
    public bool attackStarted = false; // NUEVO: Para controlar cuándo empieza el ataque
    private float temporizador = 0f;
    public float tiempoEntreAtaques = 2f; // Tiempo que tarda en volver a atacar después de atrapar al jugador
    public float tiempoDeAtaqueRojo = 3f;
    private Vector3 posicionJugador; // NUEVO: Para guardar la posición del jugador al iniciar el ataque
    public GameObject prefabAvisoRuta; // El circulito o marca que aparecerá en el suelo
    public float distanciaEntreAvisos = 2f; // Cada cuántos metros aparece una marca
    private List<GameObject> marcasDeRuta = new List<GameObject>();

    private float velocityInicial;

    private bool unaves = true;


    [Header("Ajustes de Patrulla")]
    public float distanciaAntenas = 1.5f; // A qué distancia detecta la pared para rebotar
    private Vector3 direccionPatrulla = Vector3.zero; // Aquí guardamos la flecha de movimiento
    public GameObject prefabEfectoRebote; // Prefab del efecto visual al rebotar
    public GameObject posicionRebote; // Prefab del efecto visual al golpear al jugador

    private bool jugadorDetectado = false;
    private bool Golpeado = false; 
    public string color;
        // red or blue
    [Header ("Ataque")]

    public GameObject AtaqueArea;
    public GameObject EfectoAtaque;


    



    protected override void Start()
    {
        base.Start();
        vida = 100;
        
        if (rb != null)
        {
            rb.isKinematic = false;
        }
        AtaqueArea.SetActive(false);
        EfectoAtaque.SetActive(false);

    }

    protected override void Moverse()
    {
        if (player == null || rb == null || estaGolpeado) return;
        
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
            if (color == "red")
            {
                
                temporizador += Time.deltaTime;
                if (direccionAlObjetivo != Vector3.zero)
                {
                    Quaternion rotacionDeseada = Quaternion.LookRotation(direccionAlObjetivo);
                    transform.rotation = Quaternion.Slerp(transform.rotation, rotacionDeseada, 10f * Time.deltaTime);

                }
                if (temporizador >= tiempoEntreAtaques && attackStarted == false)
                {
                    if (unaves)
                    {
                        velocityInicial = velocity;
                        unaves = false;
                    }
                    
                    attackStarted = true;
                    velocity = velocityInicial * 2f;
                    AtaqueArea.SetActive(true);
                    EfectoAtaque.SetActive(true);
                    temporizador = 0;

                }
                if (temporizador >= tiempoDeAtaqueRojo && attackStarted == true)
                {
                    attackStarted = false;
                    velocity = velocity * 0;
                    AtaqueArea.SetActive(false);
                    EfectoAtaque.SetActive(false);
                    temporizador = 0;
                    
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
            else if (color == "blue")
            {
                posicionJugador = player.transform.position;
                temporizador += Time.deltaTime;
                if (direccionAlObjetivo != Vector3.zero && attackStarted == false)
                {
                    Quaternion rotacionDeseada = Quaternion.LookRotation(direccionAlObjetivo);
                    transform.rotation = Quaternion.Slerp(transform.rotation, rotacionDeseada, 10f * Time.deltaTime);
                }
                if (temporizador >= tiempoEntreAtaques && attackStarted == false)
                {
                    attackStarted = true;

                    
                    StartCoroutine(AtaqueAzul());
                    temporizador = 0f;
                }

            }

            
        }
        else
        {
            foreach (GameObject marca in marcasDeRuta)
            {
                if (marca != null) Destroy(marca);
            }
            if (color == "Red")
            {
                
                EfectoAtaque.SetActive(false);
            }
            else if (color == "Blue")
            {
                AtaqueArea.SetActive(false);
                EfectoAtaque.SetActive(false);
            }

            marcasDeRuta.Clear();
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
    
IEnumerator AtaqueAzul()
    {
        // ==========================================
        // FASE 1: TELEGRAFIAR EL ATAQUE (Poco a poco)
        // ==========================================
        animator.SetBool("isAttacking",true);
        Vector3 origenDelRayo = transform.position + Vector3.up; 
        float distanciaMaxima = 20f; // Por si no hay pared, que no ponga marcas infinitas

        // Miramos hasta dónde llega la pared
        if (Physics.Raycast(origenDelRayo, transform.forward, out RaycastHit hitPared, distanciaMaxima, capaObstaculos))
        {
            distanciaMaxima = hitPared.distance;
        }

        // Calculamos cuántas marcas caben en esa distancia
        int cantidadMarcas = Mathf.FloorToInt(distanciaMaxima / distanciaEntreAvisos);

        if (cantidadMarcas > 0)
        {
            // Dividimos los 2 segundos entre el número de marcas para que salgan progresivamente
            float tiempoPorMarca = 2f / cantidadMarcas; 

            for (int i = 1; i <= cantidadMarcas; i++)
            {
                // Calculamos la posición hacia adelante
                Vector3 posicionMarca = transform.position + (transform.forward * (i * distanciaEntreAvisos));
                
                // La bajamos un poco (eje Y) para que se quede pegada al suelo
                posicionMarca.y = transform.position.y - 0.3f; 

                if (prefabAvisoRuta != null)
                {
                    GameObject nuevaMarca = Instantiate(prefabAvisoRuta, posicionMarca, transform.rotation);
                    marcasDeRuta.Add(nuevaMarca); // La guardamos en la mochila para luego
                }

                // Esperamos un poquito antes de poner la siguiente
                yield return new WaitForSeconds(tiempoPorMarca); 
            }
        }
        else
        {
            // Si está pegado a la pared y caben 0 marcas, simplemente esperamos los 2 segundos
            yield return new WaitForSeconds(2f);
        }

        // ==========================================
        // FASE 2: LA EMBESTIDA (Y limpiar al pisar)
        // ==========================================
        AtaqueArea.SetActive(true);
        EfectoAtaque.SetActive(true);
        while (true)
        {
            origenDelRayo = transform.position + Vector3.up; 

            // Comprobar pared para frenar
            if (Physics.Raycast(origenDelRayo, transform.forward, out RaycastHit hit, distanciaAntenas, capaObstaculos))
            {
                break; // Chocamos, salimos del bucle
            }

            // --- DESTRUIR MARCAS AL PISARLAS ---
            // Leemos la lista al revés (desde la última a la primera) para poder borrar cosas sin que haya errores
            for (int i = marcasDeRuta.Count - 1; i >= 0; i--)
            {
                if (marcasDeRuta[i] != null)
                {
                    // Si estamos a menos de 1.5 metros de la marca, la "pisamos" y la borramos
                    float distanciaALaMarca = Vector3.Distance(transform.position, marcasDeRuta[i].transform.position);
                    if (distanciaALaMarca < 1.5f)
                    {
                        Destroy(marcasDeRuta[i]);
                        marcasDeRuta.RemoveAt(i);
                    }
                }
            }

            // Correr hacia adelante
            rb.linearVelocity = new Vector3(transform.forward.x * velocityCarrera, rb.linearVelocity.y, transform.forward.z * velocityCarrera);
            
            yield return null;
        }
        
        // ==========================================
        // FASE 3: FIN DEL ATAQUE Y LIMPIEZA FINAL
        // ==========================================
        rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        AtaqueArea.SetActive(false);
        EfectoAtaque.SetActive(false);
        
        // Por si ha sobrado alguna marca sin pisar, la limpiamos por seguridad
        foreach (GameObject marca in marcasDeRuta)
        {
            if (marca != null) Destroy(marca);
        }
        marcasDeRuta.Clear(); // Vaciamos la lista
         rb.linearVelocity = new Vector3(0,0,0);
        attackStarted = false; 
        temporizador = 0f; // Reiniciamos el temporizador para que vuelva a contar desde 0 en el próximo ataque
        animator.SetBool("isAttacking",false);
    }
        protected override void RecibirDaño()
    {
        
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
