﻿using UnityEngine;
using System.Collections;

public class Sheep : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void TakeDamage() {
		GetComponent<NavMeshAgent> ().Stop ();
		gameObject.SetActive (false);
	//	Destroy ( this.gameObject );
	}
}
