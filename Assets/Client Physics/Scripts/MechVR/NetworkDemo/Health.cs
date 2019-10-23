using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class Health : NetworkBehaviour
{
	public const int maxHealth = 100;
	[SyncVar(hook = "OnChangeHealth")]
	public int currentHealth = maxHealth;
	public RectTransform healthBar;
	public GameObject healthBar3d;
	public bool destroyOnDeath;
	private NetworkStartPosition[] spawnPoints;

	void Start()
	{
		if (isLocalPlayer)
		{
			spawnPoints = FindObjectsOfType<NetworkStartPosition>();
		}
	}

	public void TakeDamage(int amount)
	{
		if (!isServer)
		{
			return;
		}

		currentHealth -= amount;
		if (currentHealth <= 0)
		{
			if (destroyOnDeath)
			{
				Destroy(gameObject);
			}
			else
			{
				currentHealth = maxHealth;
				RpcRespawn();
				Debug.Log("Dead!");
			}
		}
	}

	void OnChangeHealth(int health)
	{
		currentHealth = health;
		healthBar.sizeDelta = new Vector2(health, healthBar.sizeDelta.y);
		healthBar3d.transform.localScale = new Vector3(health / (float)maxHealth,
			healthBar3d.transform.localScale.y,
			healthBar3d.transform.localScale.z);
	}

	[ClientRpc]
	void RpcRespawn()
	{
		if (isLocalPlayer)
		{
			Vector3 spawnPoint = Vector3.zero;

			// If there is a spawn point array and the array is not empty, pick a spawn point at random
			if (spawnPoints != null && spawnPoints.Length > 0)
			{
				spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)].transform.position;
			}

			transform.position = spawnPoint;
		}
	}
}
