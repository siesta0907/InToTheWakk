using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/*
 * 화면에 바로 보이는 UI들을 관리합니다.
 * 항상 보이는 HUD같은 것들이나, 스테이지 넘어갈때 연출인 Fade등이 예시입니다.
 */
public class GameUIManager : MonoBehaviour
{
	[Header("하위 UI")]
	[SerializeField] private Hud hud;
	[SerializeField] private GameObject fade;
	[SerializeField] private GameObject gameover;

	public static GameUIManager instance;

	void Awake()
	{
		#region Singleton
		if (instance == null)
		{
			instance = this;
			DontDestroyOnLoad(this);
		}
		else
		{
			Destroy(this.gameObject);
		}
		#endregion
	}

	public void FadeIn()
	{
		// fade.GetComponent<Animator>().SetTrigger("FadeIn");
		fade.GetComponent<Animator>().SetTrigger("FadeIn_Elevator");
	}

	public void FadeOut()
	{
		// fade.GetComponent<Animator>().SetTrigger("FadeOut");
		fade.GetComponent<Animator>().SetTrigger("FadeOut_Elevator");
	}

	public void ShowGameOverScreen()
	{
		// TODO: 이후에 작업할 게임오버 스크린
		gameover.SetActive(true);
	}

	public void GoMainMenu()
	{
		// 싱글톤 모두 삭제
		Player player = FindObjectOfType<Player>();
		Player.instance = null;

		GameData gameData = FindObjectOfType<GameData>();
		GameData.instance = null;

		SfxSoundManager sfxManager = FindObjectOfType<SfxSoundManager>();
		SfxSoundManager.instance = null;

		instance = null;

		if (player)
			Destroy(player.gameObject);

		if (gameData)
			Destroy(gameData.gameObject);

		if (sfxManager)
			Destroy(sfxManager.gameObject);

		Destroy(this.gameObject);
		SceneManager.LoadScene("Title");
	}

	public void QuitGame()
	{
		Application.Quit();
	}
}
