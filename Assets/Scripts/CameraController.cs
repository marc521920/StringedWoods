using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    public GameObject player; 
    public GameObject limitXRight; 
    public GameObject limitXLeft; 
    public GameObject limitZForward; 
    public GameObject limitZBackward; 

    public float angleY = 0f; 
    public float angleX = 0f; 
    private float angleZ = 0f; // NUEVO: Guardamos el ángulo Z aquí

    public float progresoY;
    public float progresoX;
    private Vector3 posicionInicial; 
    public float angleXinicial; // NUEVO: Guardamos el ángulo Y inicial para usarlo como referencia

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player"); 
        limitXRight = GameObject.FindGameObjectWithTag("limitXRight"); 
        limitXLeft = GameObject.FindGameObjectWithTag("limitXLeft"); 
        limitZForward = GameObject.FindGameObjectWithTag("limitZForward"); 
        limitZBackward = GameObject.FindGameObjectWithTag("limitZBackward"); 

        posicionInicial = player.transform.position; 
        
        // Guardamos TODA la rotación inicial de forma estática
        angleY = transform.rotation.eulerAngles.y; 
        angleX = transform.rotation.eulerAngles.x; 
        angleZ = transform.rotation.eulerAngles.z; 

        angleXinicial = angleX; // Velocidad de rotación suave
    }

    // CAMBIO IMPORTANTE: LateUpdate se ejecuta después de que el jugador ya se movió
    void LateUpdate()
    {
        // --- EJE X (Rotación en Y) ---
        if (player.transform.position.x >= posicionInicial.x)
        {
            progresoY = Mathf.InverseLerp(posicionInicial.x, limitXLeft.transform.position.x, player.transform.position.x);
            
            angleY = Mathf.Lerp(180f, 190f, progresoY);
            
        }
        else 
        {
            progresoY = Mathf.InverseLerp(posicionInicial.x, limitXRight.transform.position.x, player.transform.position.x);
            angleY = Mathf.Lerp(180f, 170f, progresoY);
        }

        // --- EJE Z (Rotación en X) ---
        if (player.transform.position.z >= posicionInicial.z)
        {
            progresoX = Mathf.InverseLerp(posicionInicial.z, limitZBackward.transform.position.z, player.transform.position.z);
            angleX = Mathf.Lerp(angleXinicial, 22f, progresoX);
        }
        else 
        {
            progresoX = Mathf.InverseLerp(posicionInicial.z, limitZForward.transform.position.z, player.transform.position.z);
            angleX = Mathf.Lerp(angleXinicial, 15f, progresoX);
        }

        // Aplicamos la rotación usando nuestro 'angleZ' guardado, sin preguntarle a Unity
        transform.rotation = Quaternion.Euler(angleX, -angleY, angleZ); 
    }
    public void CambioDeReferencia(GameObject referenciaNuevaIzquierda , GameObject referenciaNuevaDerecha , GameObject referenciaNuevaDelante , GameObject referenciaNuevaDetras, Vector3 posicionNuevaSala)
    {
        Vector3 posicionNuevaCamara = new Vector3(posicionNuevaSala.x, transform.position.y, posicionNuevaSala.z + 7f);
        StartCoroutine(TransicionCamara(posicionNuevaCamara));
        limitXLeft = referenciaNuevaIzquierda;
        limitXRight = referenciaNuevaDerecha;
        limitZBackward = referenciaNuevaDetras;
        limitZForward = referenciaNuevaDelante;
        
    }
    IEnumerator TransicionCamara(Vector3 destino)
    {
        Vector3 posicionInicial = transform.position; // Guardamos dónde está la cámara ahora mismo
        float duracion = 0.4f; // 0.4 segundos es una velocidad "medio rápida" ideal para esto
        float tiempo = 0f;

        // Mientras no haya pasado el tiempo establecido, movemos la cámara poco a poco
        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            
            // Vector3.Lerp calcula la posición intermedia entre A y B basándose en el porcentaje de tiempo
            transform.position = Vector3.Lerp(posicionInicial, destino, tiempo / duracion);
            
            yield return null; // Esperamos al siguiente frame
        }

        // Medida de seguridad: al terminar, nos aseguramos de que esté EXACTAMENTE en el destino
        transform.position = destino;
    }
}