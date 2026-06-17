using UnityEngine;

public class CambiarAmbasTexturasTrigger : MonoBehaviour
{
    [Header("Configuración del Material")]
    [SerializeField] private Renderer objetoRenderer;


    [Header("Textura Base Color")]
    [SerializeField] private Texture2D nuevaTexturaBase; 

 
    [Header("Textura Emisión (Brillo)")]
    [SerializeField] private Texture2D nuevaTexturaEmision; 
    [SerializeField][ColorUsage(true, true)] private Color colorEmision = Color.white;

   
    private const string PropiedadBaseMap = "_BaseMap";
    private const string PropiedadEmissionMap = "_EmissionMap";
    private const string PropiedadEmissionColor = "_EmissionColor";

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            AplicarCambiosMaterial();
        }
    }

    private void AplicarCambiosMaterial()
    {
        
        if (objetoRenderer == null)
        {
            return;
        }

     
        Material mat = objetoRenderer.material;

       
        if (nuevaTexturaBase != null)
        {
            mat.SetTexture(PropiedadBaseMap, nuevaTexturaBase);
          
        }

    
        if (nuevaTexturaEmision != null)
        {
            mat.EnableKeyword("_EMISSION"); // Activa la casilla de emisión en el shader
            mat.SetTexture(PropiedadEmissionMap, nuevaTexturaEmision);
            mat.SetColor(PropiedadEmissionColor, colorEmision); // Aplica el color HDR
           
        }
    }
}