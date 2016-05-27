using UnityEngine;
using System.Collections;

public class FollowAi : MonoBehaviour {

	public bool shouldFollow = false;
	public GameObject player;
	public NavMeshAgent agent;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if(shouldFollow)
		{
			agent.destination = player.transform.position;
		}

	}
}
