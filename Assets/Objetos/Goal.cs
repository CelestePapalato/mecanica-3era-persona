using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class Goal : MonoBehaviour
{
    public static Goal instance;

    [SerializeField]
    private TMPro.TextMeshProUGUI textoVictoria;
    [SerializeField]
    private TMPro.TextMeshProUGUI textoPuntos;

    private int actualesPuntos = 0;
    private int cantidadPuntos;

    private Collider col;
    private MeshRenderer meshRenderer;

    private bool metaActivada = false;

    private void Start()
    {
        instance = this;

        col = GetComponent<Collider>();
        meshRenderer = GetComponent<MeshRenderer>();

        activarComponentes(false);

        cantidadPuntos = GameObject.FindGameObjectsWithTag("Punto").Length;

        if (textoVictoria)
        {
            textoVictoria.enabled = false;
        }

        if (textoPuntos)
        {
            textoPuntos.enabled = true;
            textoPuntos.text = actualesPuntos.ToString() + " / " + cantidadPuntos.ToString();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (textoVictoria)
            {
                textoVictoria.enabled = true;
                Invoke("recargarEscena", 5f);   
            }
        }
    }

    public void puntoAgarrado()
    {
        actualesPuntos++;
        textoPuntos.text = actualesPuntos.ToString() + " / " + cantidadPuntos.ToString();
        if(actualesPuntos >= cantidadPuntos && !metaActivada)
        {
            textoPuntos.color = Color.red;
            activarComponentes(true);
            metaActivada = true;
        }
    }

    private void activarComponentes(bool value)
    {
        col.enabled = value;
        meshRenderer.enabled = value;
    }

    private void recargarEscena()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
