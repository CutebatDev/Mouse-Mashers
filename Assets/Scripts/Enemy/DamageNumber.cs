using TMPro;
using UnityEngine;

public class DamageNumber : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;

    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float lifetime = 1f;
    [SerializeField] private float fadeTime = 0.5f;
    [SerializeField] private float horizontalOffset = 0.3f;
    [SerializeField] private float height = 0.2f;

    private float timer;
    private Color startColor;

    private void Awake()
    {
        startColor = text.color;
        transform.position += new Vector3(Random.Range(-horizontalOffset, horizontalOffset), 0, 0);
    }

    public void SetDamage(float damage)
    {
        text.text = ((int)damage).ToString();
    }

    void Update()
    {
        transform.position += Vector3.up * moveSpeed * Time.deltaTime;

        timer += Time.deltaTime;

        if (timer > lifetime - fadeTime)
        {
            float alpha = Mathf.Lerp(1, 0,
                (timer - (lifetime - fadeTime)) / fadeTime);

            text.color = new Color(
                startColor.r,
                startColor.g,
                startColor.b,
                alpha
            );
        }

        if (timer >= lifetime)
            Destroy(gameObject);
    }
}
