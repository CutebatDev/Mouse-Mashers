using TMPro;
using UnityEngine;

public class DamageNumber : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;

    [SerializeField] private float lifetime = 1f;
    [SerializeField] private float fadeTime = 0.5f;
    [SerializeField] private float startingHeight = 0.2f;

    [SerializeField] private float minHorizontal = 0.2f;
    [SerializeField] private float maxHorizontal = 0.6f;

    [SerializeField] private float minHeight = 0.3f;
    [SerializeField] private float maxHeight = 0.8f;

    private float timer;
    private Color startColor;

    private Vector3 startPosition;
    private Vector3 offset;
    private float height;

    private void Awake()
    {
        startColor = text.color;

        startPosition = transform.position + new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f), startingHeight);

        float side = Random.Range(-1f, 1f);

        offset = new Vector3(side * Random.Range(minHorizontal, maxHorizontal), startingHeight, 0);
    }

    public void SetDamage(float damage)
    {
        text.text = ((int)damage).ToString();
    }

    void Update()
    {
        timer += Time.deltaTime;

        float t = timer / lifetime;

        Vector3 pos = startPosition;

        // horizontal drift
        pos += offset * t;

        // random parabola
        pos.y += Mathf.Sin(t * Mathf.PI) * height;

        transform.position = pos;

        if (timer > lifetime - fadeTime)
        {
            float alpha = Mathf.Lerp(1, 0, (timer - (lifetime - fadeTime)) / fadeTime);

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
