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
    public float rotacionYActual; 

    private float posicionYInicial;

    private float posicionZPasillos = 0f;

    public int tipoDeSala;

    // 0 = normal
    // 1 = pasillo X (sin girar)
    // 2 = pasillo X (girado 90 grados)

    public float suavizadoSeguimiento = 5f; 
    public float offsetZ = 7f; 

    private bool enTransicion = false;

    void Start()
    {
        posicionYInicial = transform.position.y;
        rotacionYActual = -angleYinicial;
        player = GameObject.FindGameObjectWithTag("Player"); 
        limitXRight = GameObject.FindGameObjectWithTag("limitXRight"); 
        limitXLeft = GameObject.FindGameObjectWithTag("limitXLeft"); 
        limitZForward = GameObject.FindGameObjectWithTag("limitZForward"); 
        limitZBackward = GameObject.FindGameObjectWithTag("limitZBackward"); 

        posicionInicial = player.transform.position; 
        
        angleY = transform.rotation.eulerAngles.y; 
        angleX = transform.rotation.eulerAngles.x; 
        angleZ = transform.rotation.eulerAngles.z; 

        angleXinicial = angleX; 
        angleYinicial = angleY; 
    }

    void LateUpdate()
    {
        if (enTransicion == true) return; 

        if (tipoDeSala == 0)
        {
            // --- MODO 0: SALA NORMAL ---
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

            transform.rotation = Quaternion.Euler(angleX, -angleY, angleZ); 
        }
        else if (tipoDeSala == 1)
        {
            // --- MODO 1: SCROLL LATERAL EN X (Rotación normal) ---
            Vector3 posicionDestino = new Vector3(player.transform.position.x, transform.position.y, transform.position.z );
            
            transform.position = Vector3.Lerp(transform.position, posicionDestino, suavizadoSeguimiento * Time.deltaTime);
            transform.rotation = Quaternion.Euler(angleXinicial, rotacionYActual, angleZ);
        }
        else if (tipoDeSala == 2)
        {
            // --- MODO 2: SCROLL LATERAL EN X (Rotación a 270 grados + Dinámica perfecta) ---
            
            // 1. Movimiento (Sigue a la X)
            Vector3 posicionDestino = new Vector3(player.transform.position.x + 8f, transform.position.y, posicionInicial.z);
            transform.position = Vector3.Lerp(transform.position, posicionDestino, suavizadoSeguimiento * Time.deltaTime);

            // 2. Rotación cruzada por la rotación de 90º de la sala
            float anguloYModo2 = 270f;
            float anguloXModo2 = angleXinicial;

            // --- GIRO EN Y (Mirar Izquierda/Derecha) ---
            // Como la sala está girada, la Izquierda y Derecha físicas de la sala ahora se miden en el eje Z del mundo
            float paredDerRotada = Mathf.Max(limitXLeft.transform.position.z, limitXRight.transform.position.z);
            float paredIzqRotada = Mathf.Min(limitXLeft.transform.position.z, limitXRight.transform.position.z);

            if (player.transform.position.z >= posicionInicial.z)
            {
                progresoY = Mathf.InverseLerp(posicionInicial.z, paredDerRotada, player.transform.position.z);
                anguloYModo2 = Mathf.Lerp(270f, 320f, progresoY);
            }
            else 
            {
                progresoY = Mathf.InverseLerp(posicionInicial.z, paredIzqRotada, player.transform.position.z);
                anguloYModo2 = Mathf.Lerp(270f, 220f, progresoY);
            }

            // --- GIRO EN X (Inclinación Arriba/Abajo) ---
            // Como la sala está girada, el Frente y Atrás físicos de la sala ahora se miden en el eje X del mundo
            float paredFondoRotada = Mathf.Max(limitZForward.transform.position.x, limitZBackward.transform.position.x);
            float paredFrenteRotada = Mathf.Min(limitZForward.transform.position.x, limitZBackward.transform.position.x);

            if (player.transform.position.x >= posicionInicial.x)
            {
                progresoX = Mathf.InverseLerp(posicionInicial.x, paredFondoRotada, player.transform.position.x);
                anguloXModo2 = Mathf.Lerp(angleXinicial, 22f, progresoX);
            }
            else 
            {
                progresoX = Mathf.InverseLerp(posicionInicial.x, paredFrenteRotada, player.transform.position.x);
                anguloXModo2 = Mathf.Lerp(angleXinicial, 15f, progresoX);
            }

            // Aplicamos la rotación
            transform.rotation = Quaternion.Euler(anguloXModo2, anguloYModo2, angleZ);
        }
    }

    public void CambioDeReferencia(GameObject referenciaNuevaIzquierda, GameObject referenciaNuevaDerecha, GameObject referenciaNuevaDelante, GameObject referenciaNuevaDetras, Vector3 posicionNuevaSala, int sala, float gradosExtraY)
    {
        tipoDeSala = sala;
        posicionInicial = posicionNuevaSala; 
        rotacionYActual = -angleYinicial + gradosExtraY;

        Vector3 posicionNuevaCamara;
        
        

        if (tipoDeSala == 0)
        {
            GameManager.Instance.salaEspecial = false;
            posicionNuevaCamara = new Vector3(posicionNuevaSala.x, posicionYInicial, posicionNuevaSala.z + 7f);
        }
        else if (tipoDeSala == 1)
        {
            if (posicionZPasillos == 0f)
            {
                GameManager.Instance.salaEspecial = false;
                posicionNuevaCamara = new Vector3(player.transform.position.x, posicionYInicial, transform.position.z);
                posicionZPasillos = transform.position.z;
                
            }
            else
            {
                GameManager.Instance.salaEspecial = false;
                posicionNuevaCamara = new Vector3(player.transform.position.x, posicionYInicial, posicionZPasillos);
                
            }
            
        }
        else 
        {
            GameManager.Instance.salaEspecial = true;
            posicionNuevaCamara = new Vector3(player.transform.position.x, posicionYInicial -4f, posicionNuevaSala.z + 7f);
        }
        
        StartCoroutine(TransicionCamara(posicionNuevaCamara));
        
        limitXLeft = referenciaNuevaIzquierda;
        limitXRight = referenciaNuevaDerecha;
        limitZBackward = referenciaNuevaDetras;
        limitZForward = referenciaNuevaDelante;
    }

    IEnumerator TransicionCamara(Vector3 destino)
    {
        enTransicion = true; 

        Vector3 posicionInicialCam = transform.position; 
        Quaternion rotacionInicialCam = transform.rotation; 
        
        Quaternion rotacionCentro = Quaternion.Euler(angleXinicial, tipoDeSala == 2 ? 270f : rotacionYActual, angleZ); 

        float duracion = 0.4f; 
        float tiempo = 0f;

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            float porcentaje = tiempo / duracion;

            transform.position = Vector3.Lerp(posicionInicialCam, destino, porcentaje);
            transform.rotation = Quaternion.Slerp(rotacionInicialCam, rotacionCentro, porcentaje);

            yield return null; 
        }

        transform.position = destino;
        transform.rotation = rotacionCentro;
        
        enTransicion = false; 
    }
}