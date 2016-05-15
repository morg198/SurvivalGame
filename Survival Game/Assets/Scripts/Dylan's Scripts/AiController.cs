using UnityEngine;
using System.Collections;

[RequireComponent( typeof(NavMeshAgent))]
public class AiController : MonoBehaviour {

	public patrolAI patrolScript;
	public FollowAi followScript;
	public IdleAi idleScript;

	public GameObject player;

	public float chaseTime = 10f;
	public float colTime = 30f;

	public NavMeshAgent agent;

	// Use this for initialization
	void Start () {
		patrolScript = GetComponent<patrolAI>();
		followScript = GetComponent<FollowAi>();
		player = GameObject.FindGameObjectWithTag("Player");
		agent = GetComponent<NavMeshAgent>();

		patrolScript.agent = agent;
		patrolScript.player = player;
		followScript.agent = agent;
		followScript.player = player;
	}

	// Update is called once per frame
	void Update () {

		if(patrolScript.shouldPatrol)
		{
			if(agent.autoBraking == true)
			{
				agent.autoBraking = false;
			}
		}
		if(followScript.shouldFollow == true)
		{
			if(agent.autoBraking == false)
			{
				agent.autoBraking = true;
			}
		}

		if(patrolScript.enemySpotted == true)
		{ 
			if(!followScript.shouldFollow )
			{
				patrolScript.shouldPatrol = false;
				followScript.shouldFollow = true;
				StartCoroutine("follow");
			}
		}

	}

	IEnumerator follow()
	{
		yield return new WaitForSeconds(chaseTime);
		patrolScript.enemySpotted = false;
		followScript.shouldFollow = false;
		patrolScript.checkCollider = false;
		patrolScript.shouldPatrol = true;
		patrolScript.NextPoint();
		StartCoroutine("checkColTimer");

	}

	IEnumerator checkColTimer()
	{
		yield return new WaitForSeconds(colTime);
		patrolScript.checkCollider = true;
	}
}
