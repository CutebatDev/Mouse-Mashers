using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class ErrorMessage : MonoBehaviour
{
    [SerializeField] public TMP_Text errorText;
    
    public void OnClick()
    {
        Destroy(gameObject);
    }
}