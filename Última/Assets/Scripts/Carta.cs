using UnityEngine;
using UnityEngine.UI;
using System;

public class Carta : MonoBehaviour
{
    public event Action<int> OnCartaClickeada;
    
    [SerializeField] private Image imagenCarta;
    [SerializeField] private Button botonCarta;
    
    private Sprite diseno;
    private Sprite reverso;
    private int id;
    private bool estaVolteada = false;
    private bool estaEmparejada = false;
    
    public Sprite Diseno => diseno;
    public bool EstaVolteada => estaVolteada;
    public bool EstaEmparejada => estaEmparejada;

    public void Inicializar(Sprite disenoCarta, Sprite reversoCarta, int idCarta)
    {
        diseno = disenoCarta;
        reverso = reversoCarta;
        id = idCarta;
        imagenCarta.sprite = reverso;
        botonCarta.onClick.AddListener(() => OnCartaClickeada?.Invoke(id));
    }

    public void Voltear()
    {
        estaVolteada = !estaVolteada;
        imagenCarta.sprite = estaVolteada ? diseno : reverso;
    }

    public void Emparejar()
    {
        estaEmparejada = true;
        botonCarta.interactable = false; // Desactivar clics en cartas emparejadas
    }
}