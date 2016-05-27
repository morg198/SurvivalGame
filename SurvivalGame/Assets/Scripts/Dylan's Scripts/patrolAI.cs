using UnityEngine;
using System.Collections;


public class patrolAI : MonoBehaviour {

	public Transform[] points;

	public GameObject player;

	public bool shouldPatrol = true;

	private int destinationPoint;

	public NavMeshAgent agent;


	public bool enemySpotted;

	public float fieldOfView = 150f;

	private Vector3 lastSighting;

	private SphereCollider col;

	public bool checkCollider = true;


	void Start () {
		
		col = GetComponent<SphereCollider>();


		NextPoint();
	
	}

	public void NextPoint()
	{
		
		if( points.Length == 0)
		{
			return;
		}

		if(!shouldPatrol)
		{
			return;
		}

		destinationPoint = Random.Range(0, points.Length - 1); 

		agent.destination = points[destinationPoint].position;

	}

	void Update () {


		if(shouldPatrol)
		{
			if(agent.remainingDistance <= 1.5f)     
			{
				NextPoint();
			}
		}
	
	}

	void OnTriggerStay(Collider other)
	{

		if(checkCollider == true)
		{
			//Add line of sight here
			enemySpotted = true;

		}
	}
}
