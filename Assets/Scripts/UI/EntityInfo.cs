using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/*
 * 객체의 정보를 왼쪽상단으로 보여줍니다.
 */
public class EntityInfo : MonoBehaviour
{
	[SerializeField] private GameObject background;
	[SerializeField] private Image img_Profile;
	[SerializeField] private TextMeshProUGUI text_Name;
	[SerializeField] private TextMeshProUGUI text_Health;
	[SerializeField] private TextMeshProUGUI text_Str;
	[SerializeField] private TextMeshProUGUI text_AttackRange;

	public void ShowInfo(Entity entity)
	{
		background.SetActive(true);
		text_Name.text = "<color=#ffce00>" + entity.entityName + "</color>";
		text_Health.text = "체력: " + entity.curHealth + " / " + entity.health;

		if (entity.minDamage == entity.maxDamage)
			text_Str.text = "공격력: " + entity.minDamage;
		else
			text_Str.text = "공격력: " + entity.minDamage + " ~ " + entity.maxDamage;

		text_AttackRange.text = "공격범위: " + entity.attackRange;
	}

	public void CloseInfo()
	{
		background.SetActive(false);
	}
}
