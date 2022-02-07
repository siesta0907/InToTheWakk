using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Dialog : MonoBehaviour
{
	public DialogData testData;

	[SerializeField] private GameObject back;
	[SerializeField] private Image img_Portrait;
	[SerializeField] private TextMeshProUGUI text_Talker;
	[SerializeField] private TextMeshProUGUI text_Content;

	[Header("Setting")]
	[SerializeField] private float typingTime = 0.05f;

	int dialogIdx = 0;
	bool isOpen = false;
	bool isEnd = false;

	DialogData data;
	Coroutine typingCoroutine;

	void Update()
	{
		DialogInput();
	}

	void DialogInput()
	{
		if (Input.GetKeyDown(KeyCode.Q))
			ShowDialog(testData);

		if (isOpen && (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)))
		{
			// 만약 열려있는 상태인데 진행할 Dialog 정보가 없으면 Dialog를 끝냄
			if (data == null)
			{
				EndDialog();
				return;
			}

			// 이미 Dialog가 진행중인 상태에서 한번더 누른것이라면 스킵
			if (isEnd == false)
				SkipTyping();
			else
				StartCoroutine(NextDialogCoroutine());
		}
	}

	public void ShowDialog(DialogData data)
	{
		GameData.instance.uiMode = true;
		this.data = data;
		isOpen = true;
		back.SetActive(true);
		dialogIdx = -1;
		StartCoroutine(NextDialogCoroutine());
	}

	public void EndDialog()
	{
		GameData.instance.uiMode = false;
		this.data = null;
		isOpen = false;
		back.SetActive(false);
		StopAllCoroutines();
	}

	// 타이핑을 보여주는 중에 한번더 키를 누르면 타이핑이 스킵되고 전체 텍스트를 바로 보여줌
	public void SkipTyping()
	{
		if (isOpen)
		{
			StopAllCoroutines();
			isEnd = true;
			text_Content.text = data.scripts[dialogIdx].content;
		}
	}

	IEnumerator NextDialogCoroutine()
	{
		if (data != null && isOpen && dialogIdx + 1 < data.scripts.Length)
		{
			isEnd = false;
			dialogIdx += 1;

			DialogElement element = data.scripts[dialogIdx];
			text_Talker.text = element.talker;
			img_Portrait.sprite = element.portrait;

			for (int i = 0; i < element.content.Length; i++)
			{
				text_Content.text = element.content.Substring(0, i + 1);
				yield return new WaitForSeconds(typingTime);
			}

			isEnd = true;
		}
		else if (dialogIdx + 1 >= data.scripts.Length)
		{
			EndDialog();
		}

	}
}
