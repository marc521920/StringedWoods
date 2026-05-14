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
    private float angleZ = 0f; 

    public float progresoY;
    public float progresoX;
    private Vector3 posicionInicial; 
    public float angleXinicial; 
    public float angleYinicial; 

    // NUEVO: Candado para evitar que el LateUpdate pelee con la corrutina
    private bool enTransicion = false;

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

        angleXinicial = angleX; 
        angleYinicial = angleY; 
    }

    void LateUpdate()
    {
        // ¡CANDADO! Si la cámara está viajando a otra sala, no calculamos nada aquí
        if (enTransicion == true) return; 

        // --- EJE X (Rotación en Y) ---
        if (player.transform.position.x >= posicionInicial.x)
        {
            progresoY = Mathf.InverseLerp(posicionInicial.x, limitXLeft.transform.position.x, player.transform.position.x);
            angleY = Mathf.Lerp(angleYinicial, 190f, progresoY);
        }
        else 
        {
            progresoY = Mathf.InverseLerp(posicionInicial.x, limitXRight.transform.position.x, player.transform.position.x);
            angleY = Mathf.Lerp(angleYinicial, 170f, progresoY);
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

        // Aplicamos la rotación
        transform.rotation = Quaternion.Euler(angleX, -angleY, angleZ); 
    }

    public void CambioDeReferencia(GameObject referenciaNuevaIzquierda, GameObject referenciaNuevaDerecha, GameObject referenciaNuevaDelante, GameObject referenciaNuevaDetras, Vector3 posicionNuevaSala)
    {
        // --- LA LÍNEA MÁGICA: Actualizamos el "Punto Cero" al centro de la nueva sala ---
        posicionInicial = posicionNuevaSala; 

        Vector3 posicionNuevaCamara = new Vector3(posicionNuevaSala.x, transform.position.y, posicionNuevaSala.z + 7f);
        
        // Iniciamos el viaje de la cámara
        StartCoroutine(TransicionCamara(posicionNuevaCamara));
        
        // Actualizamos los límites para la nueva sala
        limitXLeft = referenciaNuevaIzquierda;
        limitXRight = referenciaNuevaDerecha;
        limitZBackward = referenciaNuevaDetras;
        limitZForward = referenciaNuevaDelante;
    }

    IEnumerator TransicionCamara(Vector3 destino)
    {
        enTransicion = true; // CERRAMOS EL CANDADO

        Vector3 posicionInicialCam = transform.position; 
        Quaternion rotacionInicialCam = transform.rotation; // Guardamos cómo estaba girada al salir
        Quaternion rotacionCentro = Quaternion.Euler(angleXinicial, -angleYinicial, angleZ); // La rotación base (neutra)

        float duracion = 0.4f; 
        float tiempo = 0f;

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            float porcentaje = tiempo / duracion;

            // Movemos la posición progresivamente
            transform.position = Vector3.Lerp(posicionInicialCam, destino, porcentaje);
            
            // Centramos la rotación suavemente durante el viaje para que no empiece torcida en la nueva sala
            transform.rotation = Quaternion.Slerp(rotacionInicialCam, rotacionCentro, porcentaje);

            yield return null; 
        }

        // Medidas de seguridad finales
        transform.position = destino;
        transform.rotation = rotacionCentro;
        
        enTransicion = false; // ABRIMOS EL CANDADO
    }
}