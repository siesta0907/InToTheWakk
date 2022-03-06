using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyDoor : MonoBehaviour
{
	public string stageName;    // 로드될 스테이지(Scene)의 이름
	[SerializeField] private GameObject elevator;
	[SerializeField] private GameObject fakeWakgood;

	Player player;
	GameUIManager um;

	void Awake()
	{
		player = FindObjectOfType<Player>();
		um = FindObjectOfType<GameUIManager>();
	}

	void Start()
	{
		player.OnTurnEnd += LoadNextStage;
	}

	public void whichDoor(string StageN)
    {
		stageName = StageN;
    }

	void LoadNextStage()
	{
		StopAllCoroutines();
		StartCoroutine(LoadNextStageCoroutine());
	}

	IEnumerator LoadNextStageCoroutine()
	{
		yield return new WaitForSeconds(GameData.instance.turnDelay);
		float distance = Vector2.Distance(transform.position, player.targetPos);

		if (distance < 0.1f)
		{
			// 연출용 우왁굳 활성화
			player.SetPlayerTurn(false, 2.8f);

			yield return new WaitForSeconds(1.0f);
			player.GetComponent<SpriteRenderer>().enabled = false;
			elevator.GetComponent<Animator>().enabled = true;
			fakeWakgood.SetActive(true);

			yield return new WaitForSeconds(0.5f);

			player.ResetDelegate();
			um.FadeOut();
			yield return new WaitForSeconds(2.0f);
			fakeWakgood.SetActive(false);
			player.GetComponent<SpriteRenderer>().enabled = true;
			SceneManager.LoadScene(stageName);
		}
	}
}
