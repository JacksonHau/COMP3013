using UnityEngine;

public class PlayerSpawnPoint : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);

        if (player != null)
        {
            player.transform.position = transform.position;
        }
    }
}