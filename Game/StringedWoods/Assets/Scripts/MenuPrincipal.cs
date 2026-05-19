using UnityEngine;
using UnityEngine.SceneManagement; // <--- MUY IMPORTANTE: La llave para cambiar de escena

public class MenuPrincipal : MonoBehaviour
{
    // Función que llamaremos al pulsar el botón "Jugar"
    public void Jugar()
    {
        // Esto cargará la escena que esté en la posición 1 de tus Build Settings
        // También puedes usar el nombre exacto: SceneManager.LoadScene("Nivel1");
        SceneManager.LoadScene(1); 
    }

    // Función que llamaremos al pulsar el botón "Salir"
    public void Salir()
    {
        // Application.Quit() cierra el juego en la versión final (.exe o móvil)
        Application.Quit();
        
        // Ponemos un Debug.Log porque Application.Quit() no hace NADA dentro del editor de Unity, 
        // así sabrás que el botón funciona cuando lo pruebes.
        Debug.Log("¡El juego se ha cerrado!");
    }
}