using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamaraController : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Cuando la cámara está en la máxima distancia")]
    private float distanciaYAlJugador = 3.5f;
    [SerializeField]
    [Range(0f, 10f)]private float sensibilidadHorizontal = 5f;
    [SerializeField]
    [Range(0f, 10f)] private float sensibilidadVertical = 0f;
    [SerializeField]
    [Range(0f, 2f)] private float velocidadMovimiento;
    [SerializeField]
    [Range(0f, 5f)] private float velocidadLookAt;
    [SerializeField]
    private float minDistanciaJugador = 6f;
    [SerializeField]
    private float maxDistanciaJugador = 17f;

    private GameObject player;

    private Vector3 currentVelocity;

    private Quaternion rotacionObjetivo;

    private float distanciaJugador;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindWithTag("Player");
        distanciaJugador = maxDistanciaJugador;
        transform.LookAt(player.transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        // Hacemos que la cámara rote y siga el movimiento de la bola
        Quaternion rotacionOriginal = transform.rotation;
        transform.LookAt(player.transform.position);
        rotacionObjetivo = transform.rotation;
        transform.rotation = rotacionOriginal;
        transform.rotation = Quaternion.Slerp(rotacionOriginal, rotacionObjetivo, velocidadLookAt * Time.deltaTime);

        //Movemos la cámara según input del jugador
        inputCamara();
        actualizarDistanciaAlJugador();
        corregirDistanciaAlJugador();
    }

    void inputCamara()
    {
        // Rotamos alrededor de la bola según input
        float rotacionHorizontal = Input.GetAxis("Mouse X");
        float rotacionVertical = Input.GetAxis("Mouse Y");
        transform.RotateAround(player.transform.position, new Vector3(0, 1, 0), rotacionHorizontal * sensibilidadHorizontal);
        transform.RotateAround(player.transform.position, new Vector3(1, 0, 0), rotacionVertical * sensibilidadVertical);
    }

    Vector3 actualDistanciaAlJugador()
    {
        return player.transform.position - transform.position;
    }

    void actualizarDistanciaAlJugador()
    {
        distanciaJugador += Input.mouseScrollDelta.y;
        distanciaJugador = Mathf.Clamp(distanciaJugador, minDistanciaJugador, maxDistanciaJugador);
    }

    void corregirDistanciaAlJugador()
    {
        Vector3 vectorCorrector = transform.position;

        // Corregimos la distancia al jugador
        if ((Mathf.Abs(actualDistanciaAlJugador().magnitude - distanciaJugador) > 0))
        {
            Vector3 distanciaDeseada = actualDistanciaAlJugador().normalized * distanciaJugador;
            distanciaDeseada *= - 1;
            Vector3 posicionEsperada = player.transform.position + distanciaDeseada;
            vectorCorrector = posicionEsperada;
        }

        // Calculamos la altura a la que debe estar la cámara usando identidades trigonométricas
        float anguloXAlJugador = Mathf.Asin(distanciaYAlJugador / maxDistanciaJugador);
        float offset = distanciaJugador * Mathf.Sin(anguloXAlJugador);
        offset += player.transform.position.y;
        vectorCorrector.y = offset;

        transform.position = Vector3.SmoothDamp(transform.position, vectorCorrector, ref currentVelocity, velocidadMovimiento);

        /*
        // Ahora corregimos el ángulo X de la cámara, para asegurarnos
        // que siempre esté en el mismo ángulo en el eje x
        Vector3 vectorCorrector;
        vectorCorrector = transform.rotation.eulerAngles;
        vectorCorrector.x = anguloXAlJugador;
        transform.rotation = Quaternion.Euler(vectorCorrector);
        */
    }
}
