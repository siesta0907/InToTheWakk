using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class Tooltip : MonoBehaviour
{
	public static Tooltip instance;

	[SerializeField] private GameObject body;
	[SerializeField] private TextMeshProUGUI title;
	[SerializeField] private TextMeshProUGUI desc;

	RectTransform tf;

	void Awake()
	{
		if (instance == null)
		{
			instance = this;
		}

		tf = GetComponent<RectTransform>();
	}

	void Update()
	{
		//tf.position = Input.mousePosition;
	}

	public void ShowTooltip(Item item, PointerEventData eventData)
	{
		title.text = item.itemName;
		title.color = item.nameColor;
		desc.text = item.itemDesc;

		tf.position = eventData.position;
		body.SetActive(true);

		if (item.itemType == ItemType.Equipment)
		{
			if (item.itemPart == EquipmentPart.Weapon)
			{
				desc.text += "\n\n";
				desc.text += "<color=yellow>공격력:</color>	" + item.minDamage + " ~ " + item.maxDamage + "          \n";
				desc.text += "<color=yellow>공격범위:</color>	" + item.attackRange + "\n";
				desc.text += "<color=yellow>명중률:</color>	" + item.accuracy + "% \n";
			}

			desc.text += "\n";
			if (item.crit > 0)
				desc.text += "+ " + item.crit + "% 크리티컬 확률 \n";

			if (item.def > 0)
				desc.text += "+ " + item.def + "% 방어력";
		}
	}

	public void HideTooltip()
	{
		title.text = "";
		desc.text = "";
		body.SetActive(false);
	}
}
