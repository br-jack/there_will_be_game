using UnityEngine;
using System;

public class PowerUpPickup : MonoBehaviour
{
    public PowerUpType powerUpType;
    public float effectAmount = 1.5f;
    public float effectDuration = 10f;

    public float fallSpeed = 1.5f;
    public float horizontalDriftAmount = 0.3f;
    public float horizontalDriftSpeed = 2f;

    public float hoverHeight = 0.2f;
    public float hoverSpeed = 2f;
    public float rotationSpeed = 60f;

    private Rigidbody rb;
    private bool landed = false;
    private Vector3 basePosition;
    private float spawnTime;
    public Action OnLanded;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        spawnTime = Time.time;

        if (rb != null)
        {
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    private void Update()
    {
        if (landed)
        {
            float yOffset = Mathf.Sin(Time.time * hoverSpeed) * hoverHeight;
            transform.position = new Vector3(basePosition.x, basePosition.y + yOffset, basePosition.z);
            transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
        }
        else
        {
            HandleFloatingFall();
        }
    }

    private void HandleFloatingFall()
    {
        float driftX = Mathf.Sin((Time.time - spawnTime) * horizontalDriftSpeed) * horizontalDriftAmount;
        Vector3 movement = new Vector3(driftX, -fallSpeed, 0f) * Time.deltaTime;
        transform.position += movement;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!landed)
        {
            landed = true;

            if (rb != null)
            {
                rb.isKinematic = true;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            basePosition = transform.position;
            OnLanded?.Invoke();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerPowerUpReceiver receiver = other.GetComponentInParent<PlayerPowerUpReceiver>();

        if (receiver != null)
        {
            if (powerUpType == PowerUpType.InfiniteFire)
            {
                receiver.ApplyPowerUp(powerUpType, 0f, 0f);
            }
            else
            {
                receiver.ApplyPowerUp(powerUpType, effectAmount, effectDuration);
            }

            Destroy(gameObject);
        }
    }
}
