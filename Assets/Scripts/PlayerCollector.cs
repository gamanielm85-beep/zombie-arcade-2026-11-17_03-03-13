using UnityEngine;

// Va en el jugador. Los coleccionables deben tener Is Trigger activado.
public class PlayerCollector : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        int points;
        string nombre;
        Color fx;

        switch (other.tag)
        {
            case "Normal":   points = 1;  nombre = "Munición";      fx = new Color(1f, 0.55f, 0.1f); break;
            case "Especial": points = 3;  nombre = "Cráneo dorado"; fx = new Color(1f, 0.85f, 0.2f); break;
            case "Trampa":   points = -1; nombre = "Barril tóxico"; fx = new Color(0.5f, 1f, 0.2f);  break;
            default: return;
        }

        Effects.Burst(other.transform.position, fx, 22, 4.5f, 0.18f, 0.55f);
        GameManager.Instance.AddScore(points, nombre);
        Destroy(other.gameObject);
    }
}
