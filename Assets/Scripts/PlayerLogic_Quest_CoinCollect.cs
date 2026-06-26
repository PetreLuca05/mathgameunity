using UnityEngine;

public class PlayerLogic_Quest_CoinCollect : MonoBehaviour
{
    public GameObject backPackModel;
    public int collectedCoins = 0;
    public int coinsToCollect = 0;
    public float moveSpeed = 1.5f;
    public float archHeight = 2f;
    public GameObject coinPrefab; // Assign the coin prefab to drop
    public Vector2 dropRadiusRange = new Vector2(1f, 3f);
    public LayerMask groundLayer;
    public float dropDelay = 0.1f;

    void Start()
    {
        gameObject.SetActive(false);
    }

    public void SetupMission(int coinsNeeded)
    {
        gameObject.SetActive(true);
        collectedCoins = 0;
        coinsToCollect = coinsNeeded;
    }

    // Called by Coin when it finishes its animation to the player
    public void OnCoinCollected()
    {
        collectedCoins++;
        if (collectedCoins <= coinsToCollect)
        {
            Debug.Log("Coin collection mission completed!");
        }
        else if (collectedCoins > coinsToCollect)
        {
            StartCoroutine(DropAllCoinsOnGround(collectedCoins));
            collectedCoins = 0;
            Debug.Log("Too many coins collected! All coins dropped on the ground.");
        }
    }

    System.Collections.IEnumerator DropAllCoinsOnGround(int count)
    {
        float angleStep = 360f / Mathf.Max(count, 1);
        for (int i = 0; i < count; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            float radius = Random.Range(dropRadiusRange.x, dropRadiusRange.y);
            Vector3 dir = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
            Vector3 arcTarget = transform.position + dir * radius + Vector3.up * 5f;

            // Raycast down from arcTarget to find the ground
            RaycastHit hit;
            Vector3 groundPos = arcTarget;
            Vector3 groundNormal = Vector3.up;
            if (Physics.Raycast(arcTarget, Vector3.down, out hit, 10f, groundLayer))
            {
                groundPos = hit.point;
                groundNormal = hit.normal;
            }
            else
            {
                groundPos = arcTarget - Vector3.up * 4.5f; // fallback if no ground found
                groundNormal = Vector3.up;
            }

            // Spawn a target transform at ground position
            GameObject dropTarget = new GameObject("DroppedCoinTarget");
            dropTarget.transform.position = groundPos;
            dropTarget.transform.up = groundNormal;
            Destroy(dropTarget, 2f);
            // Optionally, parent to player for cleanup: dropTarget.transform.parent = this.transform;
            if (coinPrefab != null)
            {
                GameObject droppedCoin = Instantiate(coinPrefab, transform.position, Quaternion.identity);
                droppedCoin.transform.position = transform.position; // spawn at player
                Coin coinScript = droppedCoin.GetComponent<Coin>();
                if (coinScript != null)
                {
                    coinScript.GoToGround(dropTarget.transform);
                }
            }
            yield return new WaitForSeconds(dropDelay);
        }
    }
}
