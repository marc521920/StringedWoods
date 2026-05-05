using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour
{
     private Rigidbody rb;
     public float velocidadBala;

     public GameObject player;
     public bool estaQuieta = false; // Nueva variable para controlar si la bala está en movimiento o no

     private Collider Collision;

    [Header("Ajustes de Desaparición")]
    public float tiempoDesvanecimiento = 0.5f; // Segundos que tarda en hacerse invisible
    
    private Material materialBala;
    private bool estaDesapareciendo = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       player = GameObject.FindWithTag("Player");
        Collision = GetComponent<CapsuleCollider>();
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = false; // La bala se moverá por la física desde el
         // Aseguramos que el material de la bala es el que queremos para el desvanecimiento
        
        Renderer rendererHijo = GetComponentInChildren<Renderer>();
        
        if (rendererHijo != null)
        {
            // ALERTA: Usar .material (en lugar de .sharedMaterial) 
            // hace una copia única para esta bala automáticamente.
            materialBala = rendererHijo.material;
        }
        
        
    }

    // Update is called once per frame
    void Update()
    {
        if (estaQuieta) return; // Si la bala está quieta, no hacemos nada
         rb.linearVelocity = transform.forward * velocidadBala;
         
         
            
         
        
    }
    private void OnTriggerEnter(Collider other) 
    {
        if (other.CompareTag("Enemy")) return; // Evitamos que la bala se destruya al chocar con el enemigo que la disparó
         if (!other.CompareTag("Player") && !other.CompareTag("Enemy"))
        {
            Collision.enabled = false; // Desactivamos el collider para evitar múltiples colisiones
            estaQuieta = true; // Detenemos la bala
            velocidadBala = 0;
            rb.isKinematic = true;
            if (!estaDesapareciendo) // Evitamos iniciar varias veces la corrutina si la bala choca con varias cosas
            {
                estaDesapareciendo = true;
                StartCoroutine(DestruirBala());
            }

        }
        
        // ¡Aquí está la clave! Hay que añadir .gameObject
        else if (other.gameObject.CompareTag("Player") && estaQuieta == false) 
        {
            Destroy(gameObject);
        }
       
    }
    IEnumerator DestruirBala()
    {
        yield return new WaitForSeconds(3f); // Esperamos 3 segundos antes de destruir la bala

        if (materialBala != null)
        {
            Color colorInicial = materialBala.color;
            float tiempoPasado = 0f;

            // Mientras no hayamos superado el tiempo límite...
            while (tiempoPasado < tiempoDesvanecimiento)
            {
                tiempoPasado += Time.deltaTime;
                
                // Calculamos el porcentaje de 0 a 1
                float porcentaje = tiempoPasado / tiempoDesvanecimiento;

                // Mathf.Lerp va pasando del valor 1 (totalmente opaco) al 0 (invisible) suavemente
                Color nuevoColor = colorInicial;
                nuevoColor.a = Mathf.Lerp(1f, 0f, porcentaje);
                
                materialBala.color = nuevoColor;

                yield return null; // Esperamos al siguiente fotograma
            }
        }
        else
        {
            // Si por algún motivo no encontró el material, esperamos el tiempo igualmente
            yield return new WaitForSeconds(tiempoDesvanecimiento);
        }

        // 3. Cuando ya es 100% invisible, borramos el objeto de verdad
        Destroy(gameObject);
    }
}
