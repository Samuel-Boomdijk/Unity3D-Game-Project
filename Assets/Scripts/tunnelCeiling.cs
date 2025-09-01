using UnityEngine;

public class tunnelCeiling : MonoBehaviour
{
    private bool isCeilingTransparent;

    private void Start()
    {
        // Look for the ceiling object and its material
        GameObject ceiling = transform.parent.Find("tunnelCeiling").gameObject;
        if (ceiling != null)
        {
            Material mat = ceiling.GetComponent<Renderer>().material;

            // Ensure the ceiling starts fully opaque (alpha = 1)
            Color baseColor = mat.GetColor("_BaseColor");
            baseColor.a = 1f; // Fully opaque
            mat.SetColor("_BaseColor", baseColor);

            // Set material to opaque mode
            SetOpaqueMode(mat);

            isCeilingTransparent = false;
        }
        else
        {
            Debug.LogError("Could not find ceiling");
        }
    }

    // Check if the player is currently walking through the tunnel
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Make the ceiling a bit transparent if not already transparent
            if (!isCeilingTransparent)
            {
                // Look for the ceiling object and its material
                GameObject ceiling = transform.parent.Find("tunnelCeiling").gameObject;
                if (ceiling != null)
                {
                    Material mat = ceiling.GetComponent<Renderer>().material;

                    Color baseColor = mat.GetColor("_BaseColor");
                    baseColor.a = 0.3f; // Adjust the alpha value for transparency
                    mat.SetColor("_BaseColor", baseColor);

                    // Set to transparent mode
                    SetTransparentMode(mat);

                    isCeilingTransparent = true;
                }
                else
                {
                    Debug.LogError("Could not find ceiling");
                }
            }
        }
    }

    // Check if the player has stopped walking through the tunnel
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Return ceiling back to normal
            if (isCeilingTransparent)
            {
                // Look for the ceiling object and its material
                GameObject ceiling = transform.parent.Find("tunnelCeiling").gameObject;
                if (ceiling != null)
                {
                    Material mat = ceiling.GetComponent<Renderer>().material;

                    Color baseColor = mat.GetColor("_BaseColor");
                    baseColor.a = 1f; // Fully opaque
                    mat.SetColor("_BaseColor", baseColor);

                    // Set to opaque mode
                    SetOpaqueMode(mat);

                    isCeilingTransparent = false;
                }
                else
                {
                    Debug.LogError("Could not find ceiling");
                }
            }
        }
    }

    // Helper method to switch material to transparent mode
    private void SetTransparentMode(Material mat)
    {
        // Set material to Transparent mode (URP)
        mat.SetFloat("_Surface", 1);  // Surface type to Transparent
        mat.SetFloat("_Blend", 1);     // Blend mode (Standard or Custom Shader should use this)
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);     // Disable ZWrite to allow transparency
        mat.SetInt("_Queue", 3000);   // Ensure it renders after opaque objects
        mat.EnableKeyword("_ALPHABLEND_ON");
    }

    // Helper method to switch material back to opaque mode
    private void SetOpaqueMode(Material mat)
    {
        // Set material back to opaque mode (URP)
        mat.SetFloat("_Surface", 0);  // Surface type to Opaque
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
        mat.SetInt("_ZWrite", 1);     // Re-enable ZWrite for opaque objects
        mat.SetInt("_Queue", 2000);   // Ensure it renders before transparent objects
        mat.DisableKeyword("_ALPHABLEND_ON");
    }
}
