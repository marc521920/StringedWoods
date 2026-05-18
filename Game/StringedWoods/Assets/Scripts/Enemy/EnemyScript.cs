using UnityEngine;
using System.Collections;
public class EnemyScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Color colorDaño = Color.red; 
    
    protected Color colorOriginal;

    protected Rigidbody rb;
    public GameObject player; // Referencia al jugador para aplicar la fuerza en la dirección correcta
    public float vida;
    public Animator animator; // Referencia al Animator para controlar las animaciones
    protected bool recibiendoGolpe = false;
    public bool estaGolpeado = false; // Nueva variable para controlar el estado de golpeado

    public bool estaMuerto = false;

    public bool tieneHyperArmor = false; // Si es true, los golpes no le frenan

    public MainCharacterMovement PlayerScript; // Referencia al script del jugador para acceder a sus variables

    public GameObject EffectoDeGolpeo;
    protected virtual void Start()
    {
        
        player = GameObject.FindWithTag("Player"); // Asegúrate de que el jugador tenga el tag "Player"
        if (player != null)
        {
            PlayerScript = player.GetComponent<MainCharacterMovement>(); // Obtenemos el script del jugador para acceder a sus variables
        }

        rb = GetComponent<Rigidbody>();
        rb.isKinematic = false; // El enemigo no se moverá por la física hasta que sea golpeado
         // Desactivamos la gravedad para que no caiga mientras patrulla o persigue
        
       

    }
    protected virtual void Update()
    {
        Moverse();
        // Aquí podrías agregar lógica para el comportamiento del enemigo, como perseguir al jugador, atacar, etc.
        if (vida <= 0 && !estaMuerto)
        {
            rb.isKinematic = true;
            estaMuerto = true; // Cerramos el candado para que no se repita
            
            // Apagamos colliders y scripts para que el jugador no le siga pegando al cadáver
            Collider[] todosLosColliders = GetComponentsInChildren<Collider>();

            foreach (Collider col in todosLosColliders)
            {
                col.enabled = false;
            }
            StopAllCoroutines();
            
            // ¡MUY IMPORTANTE! Ahora se llama con StartCoroutine
            StartCoroutine(Morir()); 
        }
    }
    
    // Seguro anti-multigolpe
  

protected virtual void OnTriggerEnter(Collider other)
    {
         if (other.CompareTag("Ataque") && !recibiendoGolpe) 
         {
                GameManager.Instance.ActivarHitStop(); 
                RecibirDaño(); // Restamos vida SIEMPRE
                Vector3 puntoDeImpacto = other.ClosestPoint(transform.position);
                GameObject Efecto = Instantiate(EffectoDeGolpeo, puntoDeImpacto, transform.rotation);
                float tamañoAleatorio = Random.Range(0.3f, 1f);

                // Le aplicamos ese mismo número a la X, la Y y la Z para que no se deforme
                Efecto.transform.localScale = new Vector3(tamañoAleatorio, tamañoAleatorio, tamañoAleatorio);
                

                // --- SISTEMA DE HYPER ARMOR ---
                // Si está en medio de un ataque imparable, nos salimos aquí y no la aturdimos
                if (tieneHyperArmor)
                {
                    return; 
                }

                // --- SI NO TIENE ARMADURA, APLICAMOS KNOCKBACK Y STUN ---
                animator.SetTrigger("GetHit");
                recibiendoGolpe = true; 
                StopAllCoroutines(); 
                rb.linearVelocity = Vector3.zero; 

                Vector3 direccionEmpuje = transform.position - other.transform.position;
                direccionEmpuje.y = 0; 
                
                if (direccionEmpuje == Vector3.zero) direccionEmpuje = -transform.forward;
                else direccionEmpuje = direccionEmpuje.normalized;
                
                StartCoroutine(RutinaKnockbackEnemigo(direccionEmpuje));
         }
    }

    // --- RUTINA MANUAL EXTREMA ---
    // --- RUTINA MANUAL CON COLISIONES ---
    IEnumerator RutinaKnockbackEnemigo(Vector3 direccion)
    {
        float duracionEmpuje = 0.2f; 
        float tiempoPasado = 0f;
        float velocidadEmpuje = PlayerScript.fuerzaGolpe * 2f; 
        estaGolpeado = true; 

        while (tiempoPasado < duracionEmpuje)
        {
            tiempoPasado += Time.fixedDeltaTime; // Usamos el tiempo de las físicas
            
            // MovePosition empuja al enemigo PERO respetando las paredes
            Vector3 nuevaPosicion = transform.position + direccion * velocidadEmpuje * Time.fixedDeltaTime;
            rb.MovePosition(nuevaPosicion);
            
            yield return new WaitForFixedUpdate(); // Nos sincronizamos con el motor físico
        }

        RestaurarColor();
    }

    void RestaurarColor()
    {
        estaGolpeado = false; // Desactivamos el estado de golpeado para que el enemigo pueda actuar de nuevo
        
        
        // Devolvemos el control físico para la gravedad
        rb.isKinematic = false; 
        
        // Quitamos el candado de los golpes
        recibiendoGolpe = false; 
    }
    protected virtual void Moverse()
    {
        // Aquí puedes implementar la lógica para reducir la salud del enemigo, reproducir animaciones de daño, etc.
    }
    protected virtual void RecibirDaño()
    {

        vida -= PlayerScript.attackDamage;
        // Aquí puedes implementar la lógica para reducir la salud del enemigo, reproducir animaciones de daño, etc.
    }
  protected virtual IEnumerator Morir()
    {
        // --- 1. MONEDAS (Entre 0 y 4) ---
        // En Random.Range para números enteros (int), el último número es EXCLUSIVO. 
        // Por eso ponemos (0, 5) para que el resultado sea 0, 1, 2, 3 o 4.
        animator.SetTrigger("isDead");
        yield return new WaitForSeconds(2f);
        int cantidadMonedas = Random.Range(0, 5); 
        for (int i = 0; i < cantidadMonedas; i++)
        {
            InstanciarBotin(GameManager.Instance.monedaPrefab);
        }

        // --- 2. EXPERIENCIA (Entre 5 y 7) ---
        // Ponemos (5, 8) para que nos dé 5, 6 o 7.
        int cantidadExperiencia = Random.Range(5, 8);
        for (int i = 0; i < cantidadExperiencia; i++)
        {
            // Tiramos un dado de 100 caras (con decimales)
            float ruleta = Random.Range(0f, 100f);

            // 1. Premio legendario (100 XP) -> 0.5% de probabilidad (Cae del 0 al 0.5)
            if (ruleta <= 0.5f) 
            {
                InstanciarBotin(GameManager.Instance.exp100Prefab);
            }
            // 2. Premio Épico (50 XP) -> 1% de probabilidad (Cae del 0.5 al 1.5)
            else if (ruleta <= 1.5f) 
            {
                InstanciarBotin(GameManager.Instance.exp50Prefab);
            }
            // 3. Premio Raro (20 XP) -> 5% de probabilidad (Cae del 1.5 al 6.5)
            else if (ruleta <= 6.5f) 
            {
                InstanciarBotin(GameManager.Instance.exp20Prefab);
            }
            // 4. Premio Poco Común (10 XP) -> 15% de probabilidad (Cae del 6.5 al 21.5)
            else if (ruleta <= 21.5f) 
            {
                InstanciarBotin(GameManager.Instance.exp10Prefab);
            }
            // 5. Premio Normal Alto (5 XP) -> 30% de probabilidad (Cae del 21.5 al 51.5)
            else if (ruleta <= 51.5f) 
            {
                InstanciarBotin(GameManager.Instance.exp5Prefab);
            }
            // 6. Premio Común (1 XP) -> 48.5% restante (El resto de la ruleta)
            else 
            {
                InstanciarBotin(GameManager.Instance.exp1Prefab);
            }
        }

        // --- 3. CORAZONES (Basado en la Suerte) ---
        // Matemática: Si luk 4 = 50%, significa que cada 1 punto de luk te da un 12.5% de probabilidad (50 / 4).
        // Si tienes luk 1 (la base), tienes 12.5% de que caiga. Si tienes luk 8, tendrás 100%.
        float probabilidadCorazon = GameManager.Instance.suerte * 10.5f; 
        
        // Tiramos un dado de 100 caras. Si sale menor o igual a tu probabilidad, ¡Premio!
        if (Random.Range(0f, 100f) <= probabilidadCorazon)
        {
            InstanciarBotin(GameManager.Instance.corazonPrefab);
        }

        // Finalmente, destruimos al enemigo
        Destroy(gameObject);
    }

    // --- FUNCIÓN DE APOYO PARA QUE EL BOTÍN NO EXPLOTE ---
    private void InstanciarBotin(GameObject prefab)
    {
        if (prefab == null) return; // Por seguridad, por si olvidas poner el prefab en el GameManager

        // Creamos un pequeño círculo imaginario alrededor del enemigo para que el botín se esparza
        Vector3 offsetAleatorio = new Vector3(Random.Range(-0.5f, 0.5f), 0.5f, Random.Range(-0.5f, 0.5f));
        
        // Instanciamos el objeto en la posición del enemigo + el pequeño desplazamiento
        Instantiate(prefab, transform.position + offsetAleatorio, Quaternion.identity);
    }


}
