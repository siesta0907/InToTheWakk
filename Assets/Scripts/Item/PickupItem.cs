using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PickupItem : MonoBehaviour
{
	[SerializeField] private Item item;
	[SerializeField] private Text text_Name;
	Player player;

	void Awake()
	{
		player = FindObjectOfType<Player>();
	}

	void Start()
	{
		player.OnTurnEnd += Pickup;
	}

	void Pickup()
	{
		StopAllCoroutines();
		StartCoroutine(PickupCoroutine());
	}

	IEnumerator PickupCoroutine()
	{
		yield return new WaitForSeconds(GameData.instance.turnDelay);
		float distance = Vector2.Distance(transform.position, player.targetPos);

		if (distance < 0.1f)
		{
			// 아이템을 주움
			player.OnTurnEnd -= Pickup;
			Inventory inventory = FindObjectOfType<Inventory>();
			if (inventory != null)
			{
				inventory.AddInventory(item, 1);
			}
			Destroy(this.gameObject);
		}
	}

	public void SetData(Item pickItem)
	{
		item = pickItem;
		text_Name.text = item.itemName;
		GetComponent<SpriteRenderer>().sprite = item.itemImage;
	}
}
