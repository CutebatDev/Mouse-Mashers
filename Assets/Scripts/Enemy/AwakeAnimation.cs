using DG.Tweening;
using UnityEngine;

public class AwakeAnimation : MonoBehaviour
{

    [SerializeField] private float animationDuration = .2f;
    [SerializeField] private GameObject visualsObject;
    private Vector3 _defaultScale;
    
    void Start()
    {
        Sequence sequence = DOTween.Sequence();

        visualsObject.transform.localScale = new Vector3(
            _defaultScale.x,
            0.01f,
            _defaultScale.z
            );
        
        sequence.Append(visualsObject.transform.DOScale(_defaultScale,animationDuration)).SetEase(Ease.OutCirc);
    }

}
