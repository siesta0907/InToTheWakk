using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using System.IO;

public class Hikiking : Enemy
{
	bool transMode = false;

	Coroutine attackCoroutine;

	protected override void Awake()
	{
		base.Awake();

		// TODO: 밸런스를 파일로 수정할 수 있게 해두었으므로 밸런스 조절후 스킬체크 콜백함수 전까지 삭제됩니다.
		// Load JSON
		/*
		string PATH = Application.dataPath + "/Data/Entity/Pungsin.json";
		if (File.Exists(PATH))
		{
			string loadjson = File.ReadAllText(PATH);
			PungsinData data = JsonUtility.FromJson<PungsinData>(loadjson);
			minDamage = data.minDamage;
			maxDamage = data.maxDamage;
			health = data.health;
			moveCount = data.moveCount;
			attackRange = data.attackRange;
			detectRange = data.detectRange;
			attackChance = data.attackChance;

			windCnt = data.windCnt;
			windAngle = data.windAngle;
			windDamage = data.windDamage;
			windSpeed = data.windSpeed;

			lightningCnt = data.lightningCnt;
			lightningDamage = data.lightningDamage;
			lightningRange = data.lightningRange;

			pushAmount = data.pushAmount;
		}
		*/
	}

	protected override void Start()
	{
		base.Start();

		player.SetPlayerTurn(false, 9.5f);
		anim.SetTrigger("StageStart");

		// 시작시 최대 체력의 10% 만큼 데미지를 주고 시작하는 스킬 발동
		StartCoroutine(StartSkillCoroutine());
	}


	protected override void EnemyTurnStart()
	{
		base.EnemyTurnStart();

		if (health <= 0 && !transMode)
			return;

		if (!isDead)
		{
			int r = Random.Range(0, 100);
			if (r <= 0)
			{
				// start pattern
				// StartCoroutine(Pattern_WindCoroutine());
			}
			else
			{
				MoveAndAttack();
			}
		}
	}


	// 시작시 1번 사용하는 스킬
	IEnumerator StartSkillCoroutine()
	{
		// 최대 체력의 10% 만큼 감소시킴
		yield return new WaitForSeconds(4.0f);

		anim.SetTrigger("StartSkill");
		yield return new WaitForSeconds(4.5f);
		float damage = player.health * 0.1f;
		player.TakeDamage((int)damage, this, true);
	}


	void MoveAndAttack()
	{
		float distance = Vector3.Distance(player.targetPos, transform.position);

		// Attack - 플레이어의 도착위치가 보스의 위치 차이가 공격범위 이내일때, 이동하지 않고 공격합니다.
		if (distance <= attackRange && Random.Range(0, 100) <= attackChance)
		{
			if (attackCoroutine != null)
				StopCoroutine(attackCoroutine);
			attackCoroutine = StartCoroutine(AttackCorotuine());

			// 애니메이션 재생 - 공격 (1페이즈)
			if (!transMode)
				anim.SetTrigger("Attack" + Random.Range(1, 3));
			else
				anim.SetTrigger("Attack" + Random.Range(1, 2) + "_Phase2");
		}
		else
		{
			Vector2Int tmp = new Vector2Int(Random.Range(-1, 1), Random.Range(-1, 1));
			Vector2Int playerPos = new Vector2Int((int)player.transform.position.x, (int)player.transform.position.y) + tmp;
			nav.MoveTo(playerPos, moveCount);
		}
	}


	IEnumerator AttackCorotuine()
	{
		// 플레이어의 이동을 기다리고 공격
		player.currentTurnDelay += attackDelay;
		yield return new WaitForSeconds(GameData.instance.turnDelay + attackDelay);

		player.TakeDamage(GetRandomDamage(), this, false);
	}


	protected override void OnDeath(Entity attacker)
	{
		// 2페이즈에서 죽은경우 - 클리어
		if (transMode)
		{
			base.OnDeath(attacker);
		}
		// 1페이즈에서 죽은경우 - 2페이즈 돌입
		else
		{
			StartCoroutine(StartPhaseCoroutine());
		}
	}

	IEnumerator StartPhaseCoroutine()
	{
		yield return null;
		player.SetPlayerTurn(false, 8.5f);
		anim.SetTrigger("Trans");
		anim.SetBool("TransMode", true);

		yield return new WaitForSeconds(6.5f);
		transMode = true;
		attackDelay += 0.18f;

		for (int i = 0; i < 60; i++)
		{
			AddHealth(health / 60);
			yield return null;
		}
	}
}
