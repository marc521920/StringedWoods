using UnityEngine;

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

    private bool jugadorDetectado = false;

    private bool Golpeado = false; 

    private bool estaGirando = false;

    public GameObject bullet;

    public GameObject salidaBala;

    [Header("Ajustes de Disparo")]
    public float cadenciaDeDisparo = 0.5f; // Cada cuántos segundos dispara
    private float temporizadorDisparo = 0f; // Nuestro cronómetro interno

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    protected virtual void Update()
    {
        base.Update();
        salidaBala.transform.LookAt(player.transform);
    }
    protected override void Moverse()
    {
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
                Debug.Log("¡TE VEO! Empezando disparar.");
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

        if (jugadorDetectado)
        {
            // MODO PERSECUCIÓN
            Vector3 posicionObjetivo = new Vector3(player.transform.position.x, transform.position.y, player.transform.position.z);
            Vector3 direccionAlObjetivo = (posicionObjetivo - transform.position).normalized;
    
            // --- SISTEMA DE DISPARO (Cronómetro) ---
            temporizadorDisparo += Time.deltaTime; // Le sumamos el tiempo que pasa en cada fotograma
            
            // Si el cronómetro supera nuestro límite (0.5s)...
            if (temporizadorDisparo >= cadenciaDeDisparo)
            {
                // Disparamos
                Instantiate(bullet, salidaBala.transform.position, salidaBala.transform.rotation);
                
                // ¡Vaciamos el cronómetro para volver a contar!
                temporizadorDisparo = 0f; 
            }
            // ---------------------------------------

            // Rotación suave hacia el jugador
            if (direccionAlObjetivo != Vector3.zero)
            {
                Quaternion rotacionDeseada = Quaternion.LookRotation(direccionAlObjetivo);
                transform.rotation = Quaternion.Slerp(transform.rotation, rotacionDeseada, 10f * Time.deltaTime);
            }
        }
        else 
        {
            // Opcional pero recomendado: si pierde de vista al jugador, reseteamos el cronómetro.
            // Así, la próxima vez que te vea, no disparará instantáneamente.
            temporizadorDisparo = 0f; 
        }
    }

}  
