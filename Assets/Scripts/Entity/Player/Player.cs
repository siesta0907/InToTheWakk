﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using System;


/*
 * 플레이어 스크립트입니다.
 * 다음과 같은 내용들이 포함되어있습니다.
 * 
 * 1) 마우스로 가져다 댄 타일정보 및 이동가능 타일인지
 * 2) 마우스로 가져다 댄 객체의 정보를 시각적으로 보여줌
 * 3) 플레이어와 관련된 정보들
 * 4) 플레이에 필요한 행동들 (공격, 이동, 보스전 Timer...)
 * 5) 그 외 연출 (Camera Shake...)
 * 
 */
public class Player : Entity
{
	// < 이벤트 >
	public event Action OnTurnEnd;		// 플레이어의 턴이 종료되면 호출됨
	public event Action OnPlayerDead;	// 플레이어가 죽은경우 호출됨

	// < 설정 >
	[Header("타일 표식")]
	public GameObject previewTile;  // 마우스를 가져다 댔을 때 보여주는 오브젝트
	public Color color_OK;			// 이동 가능한 색깔
	public Color color_NO;          // 이동 불가능한 색깔

	// < 필요한 컴포넌트 >
	PlayerInput playerInput;		// 플레이어가 입력한 키값을 받아오기 위해 사용
	TileChecker tileChecker;		// 마우스 위치 타일을 표시하기 위해 사용, 실제 움직임과 관련없음
	TargetChecker targetChecker;    // 마우스로 선택한 대상 (공격에서 사용)
	CameraShake cameraShake;		// 카메라 흔들림 효과를 위해 사용
	NavMesh2D nav;                  // 2D 네비게이션
	Inventory inv;

	// < 필요한 컴포넌트 - UI >
	Hud hud;                        // 플레이어의 상태를 표시할 HUD
	Timer timer;                    // 보스전 등에서 사용될 Timer
	EntityInfo entityInfo;			// 선택한 대상의 정보를 표시하기 위한 Entity Info

	SpriteRenderer tileRenderer;    // 이동 가능한 프리뷰 타일의 색을 변경하기 위해 사용됨

	// < 그 외 >
	public Vector3 targetPos { get; private set; }			// 이동할 위치를 미리 저장해주는 변수입니다. (Enemy 스크립트에서 사용됨)
	[HideInInspector] public float currentTurnDelay = 0.0f;	// 턴 딜레이 변수입니다. (이 시간이 모두 소모되면 턴이 돌아옵니다.)
	public bool playerTurn = true;							// 플레이어 턴 체크 변수입니다.
	bool canPush = true;                                    // 플레이어를 밀 수 있는지 체크하는 변수입니다.

	RuntimeAnimatorController defaultAnimController;        // 아무런 무기도 장착하지 않았을때 애니메이터

	public static Player instance;

	int moveLoop = 0;

	protected override void Awake()
    {
		base.Awake();

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

		playerInput = GetComponent<PlayerInput>();
		tileChecker = GetComponent<TileChecker>();
		targetChecker = GetComponent<TargetChecker>();
		cameraShake = GetComponent<CameraShake>();
		nav = GetComponent<NavMesh2D>();

		hud = FindObjectOfType<Hud>();
		timer = FindObjectOfType<Timer>();
		entityInfo = FindObjectOfType<EntityInfo>();
		inv = FindObjectOfType<Inventory>();

		tileRenderer = previewTile.GetComponent<SpriteRenderer>();

		defaultAnimController = anim.runtimeAnimatorController;

    }


	protected override void Start()
	{
		base.Start();
		hud.InitOwner(this);
		timer.OnTimerEnd += PlayerTurnEnd;

		UpdateTargetPos();
		inv.UpdateStatText(this);
	}


    void Update()
    {
		if(!isDead)
		{
			TurnCheck();		// 일정시간 뒤 돌아오는 턴을 체크
			ShowPreviewTile();  // 타일에 마우스를 올렸을 때 효과를 보여줌
			ShowEntityInfo();	// 엔티티 정보를 보여줌

			// * Player Action부분(입력을 받아 턴을 소비)
			TryAttack();
			TryMove();

			// * Player Animation 부분
			MoveAnimation();
		}
    }


	// 설정된 딜레이에 따른 턴 체크
	private void TurnCheck()
	{
		currentTurnDelay -= Time.deltaTime;
		if (currentTurnDelay <= 0 && playerTurn == false)
		{
			PlayerTurnStart();
		}
	}


	// 클릭하려는 타일을 보여줌 (벽이 아니고, 플레이어 턴이며, 이동중이지 않고, 타겟이 없는경우)
	private void ShowPreviewTile()
	{
		if (/*!GameData.instance.uiMode*/
			GameData.instance.uiLevel == 0 && !tileChecker.TileIsWall() && playerTurn
			&& nav.velocity == Vector3.zero && targetChecker.selectedEntity == null
			&& moveLoop <= 0)
		{
			previewTile.SetActive(true);
			previewTile.transform.position = tileChecker.GetTilePosition();

			ChangePreviewTileColor();
		}
		else
		{
			previewTile.SetActive(false);
		}
	}


	// 이동 가능한 범위인지 확인하고 색을 변경함
	private void ChangePreviewTileColor()
	{
		if (tileChecker.GetDistance() <= moveCount)
			tileRenderer.color = color_OK;
		else
			tileRenderer.color = color_NO;
	}


	// 선택한 대상의 정보를 보여줍니다.
	private void ShowEntityInfo()
	{
		if (targetChecker.selectedEntity)
			entityInfo.ShowInfo(targetChecker.selectedEntity);
		else
			entityInfo.CloseInfo();
	}


	// 대상을 공격함 (턴 소비)
	private void TryAttack()
	{
		if (/* !GameData.instance.uiMode */
			GameData.instance.uiLevel == 0 && playerInput.LButtonClick && playerTurn && targetChecker.selectedEntity
			&& !targetChecker.selectedEntity.invincible && targetChecker.GetDistance() <= attackRange)
		{
			// 공격이 여러번할 수 없도록 임시로 입력을 막습니다.
			SetPlayerTurn(false, 1.0f);
			timer.StopTimer(true);

			// TODO: 이후에 지울 Debug.Log
			Debug.Log(targetChecker.selectedEntity.transform.name + "을 공격함!");

			// 애니메이션 재생 - 공격
			anim.SetTrigger("AttackDefault");
			SfxSoundManager.instance.PlaySound(inv.GetWeaponSound());
			LookEntity(targetChecker.selectedEntity, false);

			StartCoroutine(AttackCoroutine(attackDelay, targetChecker.selectedEntity));
		}
	}

	IEnumerator AttackCoroutine(float time, Entity target)
	{
		yield return new WaitForSeconds(time);

		// 피해를 입히고 플레이어 턴을 끝냅니다.
		// * 장착한 아이템에 따라 피해를 입히지 못할 수 있습니다.

		float randAcc = UnityEngine.Random.Range(1.0f, 100.0f);
		if (randAcc <= inv.GetWeaponAccuracy())
		{
			target.TakeDamage(GetRandomDamage(), this);
		}
		else
		{
			Debug.Log("공격 적중실패");
		}
		timer.StopTimer(false);
		PlayerTurnEnd();
	}


	// 클릭시 이동 (턴 소비)
	private void TryMove()
	{
		if (/*!GameData.instance.uiMode*/
			GameData.instance.uiLevel == 0 && playerInput.LButtonClick && playerTurn && targetChecker.selectedEntity == null) // 왼쪽 버튼을 클릭한 경우
		{
			// 벽이 아니고, 거리가 움직일수 있는 범위보다 작고, 움직이는 상태가 아니면
			if (!tileChecker.TileIsWall() && /*tileChecker.GetDistance() <= moveCount &&*/ nav.velocity == Vector3.zero
				&& moveLoop <= 0)
			{
				// 마우스 좌표를 불러옴
				Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

				// 타일 좌표에 맞게 Int로 변환후 저장
				int x = Mathf.RoundToInt(mousePos.x);
				int y = Mathf.RoundToInt(mousePos.y);

				int pathCnt = nav.CheckPathCount(new Vector2Int(x, y));

				GameManager gm = FindObjectOfType<GameManager>();
				if (pathCnt > moveCount && gm.continueMove == false)
				{
					return;
				}

				moveLoop = pathCnt / moveCount;
				if (pathCnt % moveCount != 0)
					moveLoop++;

				mousePos = new Vector3(x, y, 0);

				// 움직이려는 위치가 현재 위치와 다르다면
				if(mousePos != transform.position)
				{
					// 이동위치 저장, 이동, 턴종료 알림
					StartCoroutine(AutoMove(x, y));
				}
			}
		}
	}

	IEnumerator AutoMove(int x, int y)
	{
		while (moveLoop > 0)
		{
			if (currentTurnDelay <= 0 && playerTurn)
			{
				if (nav.CheckPath(new Vector2Int(x, y)) == false)
				{
					moveLoop = 0;
					break;
				}

				if (moveLoop == 1)
					targetPos = new Vector3(x, y, 0);
				else
					targetPos = transform.position;
				nav.MoveTo(new Vector2Int(x, y), moveCount);
				PlayerTurnEnd();

				moveLoop--;
				SfxSoundManager.instance.PlaySound("Walk");
			}
			yield return new WaitForSeconds(0.25f);
		}
	}


	// 플레이어의 턴이 돌아왔을 때
	private void PlayerTurnStart()
	{
		playerTurn = true;

		// 현재 스테이지 게임매니저에서 타이머를 사용한다면 타이머 세팅
		GameManager gm = FindObjectOfType<GameManager>();
		if (gm && gm.activeTimer) timer.SetTimer(gm.time);
	}


	// 턴 종료시 무슨 행동을 할것인지 (배고픔 감소... 등)
	public void PlayerTurnEnd()
	{
		// 턴 증가, 딜레이 리셋, 포만감 감소
		GameData.instance.turn += 1;
		currentTurnDelay = GameData.instance.turnDelay;
		playerTurn = false;
		timer.ClearTimer();
		AddSatiety(-GameData.instance.decreaseSatiety);

		if(OnTurnEnd != null)
			OnTurnEnd();
	}

	private void MoveAnimation()
	{
		// 애니메이션 처리
		if (nav.velocity.magnitude > 0)
			anim.SetBool("IsMove", true);
		else
			anim.SetBool("IsMove", false);

		// 방향 처리
		if(nav.velocity.x != 0)
			sr.flipX = (nav.velocity.x < 0) ? true : false;

	}


	// Delegate를 초기화합니다. (스테이지 이동시 사용됩니다)
	public void ResetDelegate()
	{
		OnTurnEnd = null;
		OnPlayerDead = null;
	}


	// targetPos를 현재위치로 업데이트합니다. (적들은 targetPos 기준으로 행동하기 때문에 필요합니다.)
	public void UpdateTargetPos()
	{
		targetPos = new Vector3(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y), Mathf.RoundToInt(transform.position.z));
	}


	// 강제로 턴 설정 (자원, 턴 횟수는 영향받지 않음)
	/* PlayerTurnEnd가 호출되지 않습니다. */
	public void SetPlayerTurn(bool turn, float delay)
	{
		playerTurn = turn;
		currentTurnDelay = GameData.instance.turnDelay + delay;
	}


	// 플레이어 밀기
	public void Push(Vector2 dir, int amount)
	{
		if(canPush == true)
			StartCoroutine(PushCoroutine(dir, amount));
	}


	IEnumerator PushCoroutine(Vector2 dir, int amount)
	{
		canPush = false;

		// 이동 가능범위 계산
		Vector2 originPos = transform.position;
		Vector2 targetPos = transform.position;

		for (int i = amount; i > 0; i--)
		{
			if (!Physics2D.Raycast(originPos + dir, dir, i - 1))
			{
				targetPos = originPos + (dir * i);
				break;
			}
		}

		// 이동
		nav.navVolume.SetWallAtPosition(transform.position, false);
		while (true)
		{
			transform.position = Vector2.Lerp(transform.position, targetPos, 0.15f);
			if (Vector2.Distance(transform.position, targetPos) < 0.05f)
			{
				transform.position = targetPos;
				break;
			}
			yield return null;
		}
		nav.navVolume.SetWallAtPosition(transform.position, true);
		canPush = true;
	}

	public void SetWeaponAnimation(RuntimeAnimatorController animator)
	{
		if (animator)
			anim.runtimeAnimatorController = animator;
		else
		{
			anim.runtimeAnimatorController = defaultAnimController;
		}
	}

	public void EatFood(float health, float satiety, float mana)
	{
		anim.SetTrigger("Eat");
		SetPlayerTurn(false, 1.0f);
		timer.StopTimer(true);
		SfxSoundManager.instance.PlaySound("Eat");
		StartCoroutine(EatFoodCoroutine(health, satiety, mana));
	}

	IEnumerator EatFoodCoroutine(float health, float satiety, float mana)
	{
		yield return new WaitForSeconds(1.0f);

		// 피해를 입히고 플레이어 턴을 끝냅니다.
		timer.StopTimer(false);
		AddHealth(health);
		AddSatiety(satiety);
		AddMana(mana);
		PlayerTurnEnd();
	}


	// Override Method
	protected override void OnDeath(Entity attacker)
	{
		base.OnDeath(attacker);
		// TODO: 이후에 지울 Debug.Log
		Debug.Log("플레이어가 죽었습니다!");
		// 애니메이션 재생 - 사망
		anim.SetTrigger("Dead");
		timer.ClearTimer();

		if(OnPlayerDead != null)
			OnPlayerDead();
	}


	protected override void OnHit(Entity victim)
	{
		base.OnHit(victim);
		cameraShake.Play(Camera.main, 0.16f, 0.1f);
	}


	public override void TakeDamage(int damage, Entity attacker)
	{
		base.TakeDamage(damage, attacker);

		// 애니메이션 재생 - 피격
		if(!isDead)
			anim.SetTrigger("HitReact");

		Debug.Log(attacker.transform.name + "에게 공격당함!");
		SfxSoundManager.instance.PlaySound("Wakgood_Hit");
	}
}
