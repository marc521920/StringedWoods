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
    public float rotacionYActual; // Guarda la rotación de la sala en la que estamos

    public int tipoDeSala;

    // 0 = normal
    // 1 = pasillo
    // 2 = especial

    public float suavizadoSeguimiento = 5f; // Velocidad a la que la cámara persigue al jugador
    public float offsetZ = 7f; // La distancia Z que vimos en tu otra función

    // Candado para evitar que el LateUpdate pelee con la corrutina
    private bool enTransicion = false;

    void Start()
    {
        rotacionYActual = -angleYinicial;
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

        if (tipoDeSala == 0)
        {
            // --- MODO 0: SALA NORMAL (Rotación en el sitio) ---
            
            // EJE X (Rotación en Y)
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

            // EJE Z (Rotación en X)
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
        else if (tipoDeSala == 2)
        {
            // --- MODO 1: PASILLO VERTICAL (Seguir en Z) ---
            // Pasa del centro. Solo sigue la Z del jugador.
            Vector3 posicionDestino = new Vector3(transform.position.x, transform.position.y, player.transform.position.z + offsetZ);
            
            transform.position = Vector3.Lerp(transform.position, posicionDestino, suavizadoSeguimiento * Time.deltaTime);
            transform.rotation = Quaternion.Euler(angleXinicial, rotacionYActual, angleZ);
        }
        else if (tipoDeSala == 1)
        {
            // --- MODO 2: SCROLL LATERAL (Seguir en X) ---
            // Pasa del centro. Solo sigue la X del jugador.
            Vector3 posicionDestino = new Vector3(player.transform.position.x, transform.position.y, transform.position.z);
            
            transform.position = Vector3.Lerp(transform.position, posicionDestino, suavizadoSeguimiento * Time.deltaTime);
            transform.rotation = Quaternion.Euler(angleXinicial, rotacionYActual, angleZ);
        }
    }

    // CAMBIO: He añadido ", float gradosExtraY" al final del paréntesis
    public void CambioDeReferencia(GameObject referenciaNuevaIzquierda, GameObject referenciaNuevaDerecha, GameObject referenciaNuevaDelante, GameObject referenciaNuevaDetras, Vector3 posicionNuevaSala, int sala, float gradosExtraY)
    {
        tipoDeSala = sala;
        posicionInicial = posicionNuevaSala; 
        rotacionYActual = -angleYinicial + gradosExtraY;

        Vector3 posicionNuevaCamara;

        // AQUÍ ESTÁ LA CLAVE: Decidimos a dónde viaja la cámara según el modo
        if (tipoDeSala == 0)
        {
            // Sala Normal: Viaja al centro de la sala
            posicionNuevaCamara = new Vector3(posicionNuevaSala.x, transform.position.y, posicionNuevaSala.z + 7f);
        }
        else if (tipoDeSala == 1)
        {
            // Pasillo en Z: IGNORA EL CENTRO. Viaja a donde esté el jugador en ese momento
            posicionNuevaCamara = new Vector3(transform.position.x, transform.position.y, player.transform.position.z + offsetZ);
        }
        else 
        {
            // Pasillo en X: IGNORA EL CENTRO. Viaja a donde esté el jugador en ese momento
            posicionNuevaCamara = new Vector3(player.transform.position.x, transform.position.y, transform.position.z);
        }
        
        StartCoroutine(TransicionCamara(posicionNuevaCamara));
        
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
        
        // CAMBIO: El destino de la rotación ahora usa 'rotacionYActual'
        Quaternion rotacionCentro = Quaternion.Euler(angleXinicial, rotacionYActual, angleZ); 

        float duracion = 0.4f; 
        float tiempo = 0f;

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            float porcentaje = tiempo / duracion;

            // Movemos la posición progresivamente
            transform.position = Vector3.Lerp(posicionInicialCam, destino, porcentaje);
            
            // Centramos la rotación suavemente durante el viaje hacia los nuevos grados
            transform.rotation = Quaternion.Slerp(rotacionInicialCam, rotacionCentro, porcentaje);

            yield return null; 
        }

        // Medidas de seguridad finales
        transform.position = destino;
        transform.rotation = rotacionCentro;
        
        enTransicion = false; // ABRIMOS EL CANDADO
    }
}