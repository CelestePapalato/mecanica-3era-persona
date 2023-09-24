using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BolaController : MonoBehaviour
{
    // Constantes

    [Header("Variables de movimiento")]
    [SerializeField]
    private float fuerzaDeMovimiento = 20f;
    [SerializeField]
    private float aceleracion = 30f;
    [SerializeField]
    private float friccion = 20f;
    [SerializeField]
    private float drag = 5f;
    [SerializeField]
    private ForceMode modoDrag = ForceMode.Acceleration;
    [SerializeField]
    [Range(0f, 0.3f)] private float suavizadoDeRotacion;

    [Header("Variables de salto")]
    public LayerMask capaPiso;
    [SerializeField]
    private float magnitudSalto = 2f;

    // Variables

    private Vector2 inputDirection = Vector2.zero;
    private float velocidadMovimiento;
    private float velocidadRotacion;
    private bool sobreInclinacion = false;

    // Componentes

    private Rigidbody rb;
    private SphereCollider col;

    // C�mara
    private GameObject camara;

    // Referencia para obtener vector adelante
    // Es importante usar este vector y no rotar la bola directamente porque
    // interferimos con la f�sica de la misma
    private Vector3 adelante;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = rb.GetComponent<SphereCollider>();
        camara = GameObject.FindGameObjectWithTag("MainCamera");
        adelante = transform.forward;
    }

    // Update is called once per frame
    void Update()
    {
        verificarInput();
        mover();
        saltar();
    }

    void verificarInput()
    {
        float x_axis = Input.GetAxis("Horizontal");
        float y_axis = Input.GetAxis("Vertical");

        inputDirection = new Vector2(x_axis, y_axis).normalized;
    }

    private void mover()
    {
        // Obtenemos la velocidad de movimiento que debe tener la bola. Si el jugador no se est� moviendo,
        // la velocidad es 0; sino, su velocidad es fuerzaDeMovimiento
        float velocidadObjetivo = fuerzaDeMovimiento;

        // Si el jugador est� movimiento, la raz�n de cambio de la velocidad es la aceleraci�n, sino
        // la fricci�n
        float razonDeCambioVelocidad = aceleracion;
        int sentido = 1;

        // Solo rotamos la direcci�n de la bola si el jugador ha ingresado input
        if (inputDirection != Vector2.zero)
        {
            // Primero, Modificamos la rotaci�n del vector adelante
            // Obtenemos el �ngulo en el que estamos
            float rotacionActual = Mathf.Atan2(adelante.x, adelante.z) * Mathf.Rad2Deg;

            // Obtenemos el �ngulo al que hay que rotar
            float rotacionObjetivo = Mathf.Atan2(inputDirection.x, inputDirection.y) * Mathf.Rad2Deg;

            // Usamos el m�todo SmoothDampAngle para que el cambio al nuevo �ngulo sea progresivo
            float rotacion = Mathf.SmoothDampAngle(rotacionActual, rotacionObjetivo, ref velocidadRotacion, suavizadoDeRotacion);

            // Rotamos el vector adelante
            adelante = Quaternion.Euler(0f, rotacion, 0f) * Vector3.forward;      
        }

        // Si el jugador no est� ingresando input, cambiamos las variables correspondientes
        else
        {
            velocidadObjetivo = 0;
            razonDeCambioVelocidad = friccion;
            sentido = -1;
        }

        // Hacemos que la velocidad de movimiento alcance la velocidad objetivo progresivamente
        velocidadMovimiento = Mathf.Lerp(velocidadMovimiento, velocidadObjetivo, Time.deltaTime * razonDeCambioVelocidad);

        // Obtenemos el vector de fuerza
        Vector3 velocidad = adelante * velocidadMovimiento * sentido;

        // Cambiamos la direcci�n del vector seg�n d�nde mira la c�mara
        // Quaternion * Vector = OK
        // Vector * Quaternion = ERROR
        velocidad = Quaternion.Euler(0f, camara.transform.eulerAngles.y, 0f) * velocidad;

        // Aplicamos la fuerza velocidad
        rb.AddForce(velocidad);

        // Si la est� sobre terreno no inclinado, limitamos la velocidad.
        // Esto ser�a un intento de imitaci�n del drag del rigidbody
        // El drag del rigidbody tambi�n afecta a la gravedad, y yo no quiero eso
        if ((sobreInclinacion || !estaEnPiso()) && rb.velocity.magnitude > 0)
        {
            agregarDrag();
        }
    }

    private void agregarDrag()
    {
        // Inicializamos el vector velocidadExcedida como el opuesto a la velocidad del rigidbody
        Vector3 velocidadExcedida = rb.velocity * -1;

        // Seteamos el valor de velocidad excedida del eje y en 0 para no afectar a la gravedad
        velocidadExcedida.y = 0;

        //Multiplicamos la velocidad excedida por esta fracci�n. El objetivo es
        // suavizar el efecto de la desacelerada que har� la bola
        // El denominador nunca ser� cero porque para eso el deltaTime debe ser 1,
        // y por su naturaleza es particularmente imposible que suceda.
        velocidadExcedida *= 1f / (1f - Time.deltaTime * drag);

        // Aplicamos la aceleraci�n que detendr� a la bola
        rb.AddForce(velocidadExcedida, modoDrag);

        /*Otros m�todos para limitar la velocidad del rigidbody:
            >>> Vector3.ClampMagnitude
            >>> Vector3.SmoothDamp
            >>> Operaciones de resta y suma directas con la velocidad del rigidbody
            >>> No es recomendable asignarle un nuevo vector al rigidbody
        */
    }

    private void saltar()
    {
        if (estaEnPiso() && Input.GetKeyDown(KeyCode.Space))
        {
            rb.AddForce(magnitudSalto * Vector3.up, ForceMode.Impulse);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Vamos a chequear si estamos sobre terreno inclinado. Lo vamos a usar para saber si deber�amos
        // limitar la velocidad. Solo la limitamos en suelo plano.
        // El suelo es el �nico objeto con collision y no trigger, as� que no deber�a detectar otras cosas

        // Obtenemos la normal
        Vector3 inclinacionTerreno = collision.gameObject.transform.up;

        // Sacamos su �ngulo con respecto al plano XZ
        float anguloTerreno = Vector3.Angle(inclinacionTerreno, new Vector3(1, 0, 1));

        // Si el �ngulo no es 90�, el terreno est� inclinado. A�ado un margen de 5 grados de tolerancia.
        sobreInclinacion = !(Mathf.Abs(anguloTerreno - 90) > 5);
    }

    private bool estaEnPiso() 
    { return Physics.CheckCapsule(col.bounds.center, new Vector3(col.bounds.center.x, col.bounds.min.y, col.bounds.center.z), col.radius * .9f, capaPiso); 
    }
}

