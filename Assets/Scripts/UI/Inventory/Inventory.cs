using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Inventory : MonoBehaviour
{
	[SerializeField] private Slot[] slots;
	[SerializeField] private GameObject body;

	[Header("Equipment Slots")]
	[SerializeField] private Slot weaponSlot;
	[SerializeField] private Slot helmetSlot;
	[SerializeField] private Slot chestplateSlot;
	[SerializeField] private Slot bootsSlot;
	[SerializeField] private Slot jewelySlot;

	[Header("Food Slots")]
	[SerializeField] private Slot[] foodSlots;

	[Header("Stat")]
	[SerializeField] private TextMeshProUGUI text_CritValue;
	[SerializeField] private TextMeshProUGUI text_DefValue;

	public Item[] testItems;

	Player player;
	bool isOpen;

	void Awake()
	{
		player = FindObjectOfType<Player>();
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.W))
			AddInventory(testItems[Random.Range(0, testItems.Length)], 1);

		if (Input.GetKeyDown(KeyCode.E))
		{
			if (isOpen)
				CloseInventory();
			else
				OpenInventory();
		}
	}

	public void AddInventory(Item item, int count)
	{
		// 1) 먼저 겹칠 수 있는 아이템이 있으면 겹침
		for (int i = 0; i < slots.Length; i++)
		{
			if (slots[i].item == item && slots[i].itemCount + count <= slots[i].item.maxCount)
			{
				slots[i].AddItemCount(count);
				return;
			}
		}

		// 2) 하나도 겹칠 수 없으면 비어있는 인벤토리에 아이템을 추가
		for (int i = 0; i < slots.Length; i++)
		{
			if (slots[i].item == null)
			{
				slots[i].SetItem(item, count);
				return;
			}
		}
	}

	public string GetWeaponSound()
	{
		if (weaponSlot.item == null)
			return "DefaultAttack";

		return weaponSlot.item.attackSound;
	}

	public void OpenInventory()
	{
		GameData.instance.uiMode = true;
		body.SetActive(true);
		isOpen = true;
	}

	public void CloseInventory()
	{
		GameData.instance.uiMode = false;
		body.SetActive(false);
		DragOperation.instance.SetDragSlot(null);
		Tooltip.instance.HideTooltip();
		isOpen = false;
	}

	public bool IsFull()
	{
		for (int i = 0; i < slots.Length; i++)
			if (slots[i].item == null)
				return false;
		return true;
	}

	// 장비 아이템 착용시
	public void OnEquipmentChange(EquipmentPart part, Item item)
	{
		// 무기 아이템의 경우 애니메이션을 추가로 변경
		if (part == EquipmentPart.Weapon)
		{
			// 장착한 아이템이 있다면 해당 아이템으로 애니메이션 변경
			if (item != null)
			{
				player.SetWeaponAnimation(item.weaponAnim);

				// 데미지와 공격범위 변경
				player.SetMinMaxDamage(item.minDamage, item.maxDamage);
				player.attackRange = item.attackRange;
			}
			// 없다면 기본 애니메이션(주먹)으로 변경
			else
			{
				player.SetWeaponAnimation(null);

				// 데미지와 공격범위 변경
				player.SetMinMaxDamage(1, 1);
				player.attackRange = 1;
			}
		}

		// 일반 스텟들도 적용 (크리티컬 확률, 방어력...)
		ApplyEquipmentStat();
	}

	// 무기 명중률을 가져옴
	public float GetWeaponAccuracy()
	{
		if (weaponSlot.item != null)
			return weaponSlot.item.accuracy;

		return 100.0f;
	}

	// 장착한 아이템들의 스텟을 계산
	public void ApplyEquipmentStat()
	{
		// 모든 아이템의 스텟을 합산한 값을 저장하기 위한 변수
		float crit = 0.0f;
		float def = 0.0f;

		// 슬롯에 아이템을 장착하고 있는경우 스텟들을 누적시킴
		if (weaponSlot.item != null)
		{
			crit += weaponSlot.item.crit;
			def += weaponSlot.item.def;
		}

		if (helmetSlot.item != null)
		{
			crit += helmetSlot.item.crit;
			def += helmetSlot.item.def;
		}

		if (chestplateSlot.item != null)
		{
			crit += chestplateSlot.item.crit;
			def += chestplateSlot.item.def;
		}

		if (bootsSlot.item != null)
		{
			crit += bootsSlot.item.crit;
			def += bootsSlot.item.def;
		}

		if (jewelySlot.item != null)
		{
			crit += jewelySlot.item.crit;
			def += jewelySlot.item.def;
		}

		// 범위를 벗어나지 않도록 함
		crit = Mathf.Clamp(crit, 0.0f, 100.0f);
		def = Mathf.Clamp(def, 0.0f, 100.0f);

		// 기본 크리티컬과 방어력으로 재설정
		player.ResetCriticalChance();
		player.ResetDefence();

		// 설정된 기본값에 장비에 포함된 스텟만큼 추가시킴
		player.AddCriticalChance(crit);
		player.AddDefence(def);

		UpdateStatText(player);
	}

	public void UpdateStatText(Entity entity)
	{
		if (entity)
		{
			text_CritValue.text = entity.crit + "%";
			text_DefValue.text = entity.def + "%";
		}
	}

	// 음식(소비) 아이템 착용시
	public void OnFoodChange()
	{

	}
}
