﻿using InControl;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class Player : MonoBehaviour
{

    public delegate void PlayerWasKilled(Player killer, Player victim);
    public static PlayerWasKilled OnPlayerKilled;

    public GameObject Particles;

	public float speed = 6000f;
    public float rotateSpeed = 10f;
    public Animator animationController;
    public Animator secondAnimationController;

	public InputDevice Device { get; set; }

	Renderer cachedRenderer;

	private Rigidbody rigidbody;

	public bool isWolf { get; set; }

	private Vector3 faceDirection;
	private Vector3 hitDirection;
	private float attackRange = 0.5f;

	private int hittableMask = 1 << 8;

	private bool isKilled = false;
	private PlayerManager playerManager;
    

	private int score = 0;
	public string playerIndex;

    public Image groundIndicator;

	private bool isGameStarted = false;

	private bool pushDown = false;

	// Use this for initialization
	void Start () {
		Device = null;
		rigidbody = GetComponent<Rigidbody> ();
		playerManager = GameObject.Find ("PlayerManager").GetComponent<PlayerManager> ();
		playerIndex = gameObject.name.Replace ("Player_", "");
        

        
    }

	void Update()
	{

		if(!isGameStarted && SceneManager.GetActiveScene().name=="Demo Scene") {
			isGameStarted = true;
			RespawnPlayer ();
            groundIndicator.color = new Color(Random.value, Random.value, Random.value);
            Invoke("RemoveIndicator", 5);
        }

		if(isGameStarted)
			Controls();
		
	    if (Device == null) return;

        if (isWolf)
	    {
	        Device.Vibrate(0.1f, 0.1f);
	    }
	    else
	    {
	        Device.StopVibration();
	    }

		if (Input.GetKey (KeyCode.A)) {
			RespawnPlayer ();
		}
	}

	public void RespawnPlayer() {
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        var particles = Instantiate(Particles);
        if (particles) particles.transform.position = transform.position;
        Vector3 startPosition = GameObject.Find ("Plane"+playerIndex).transform.position;
		startPosition.y = 4;
		transform.position = startPosition;
		pushDown = true;
	}

	void Controls() {
		if (isWolf) {
			if (Device.Action1.WasPressed) {
				Attack ();
			}
		}
	}

	public void MakeWolf() {
		isWolf = true;
		tag = "Wolf";
	}

	public void MakeSheep() {
		isWolf = false;
		tag = "Player";
	}

	void Attack() {
		Vector3 attackPos = transform.position;
		hitDirection = faceDirection;
		attackPos += hitDirection * (attackRange/1.5f);
		Collider[] hitColliders = Physics.OverlapSphere(attackPos, attackRange, hittableMask);
		for(int i = 0; i < hitColliders.Length; i++){
			if (hitColliders [i].tag != "Wolf") {
				hitColliders[i].SendMessage("TakeDamage",  this);
                return; //To only kill one
				//UpdatePlayerScoreGUI ();
			}
		}
	}

	void TakeDamage(object damageInflicter)
	{

		isKilled = true;
	    if (OnPlayerKilled != null)
	    {
            if (SoundManager.instance)
			    SoundManager.instance.PlayWolfBiteSuccess ();
	        OnPlayerKilled.Invoke((Player) damageInflicter, this);
	    }
	}

	// Update is called once per frame
	void FixedUpdate ()
	{
	    var isMoving = false;
		if (Device!=null) {
			faceDirection = new Vector3(Device.LeftStickX, 0.0f, Device.LeftStickY);
			RotateTowardsDirection(faceDirection);
			rigidbody.velocity = new Vector3(faceDirection.x * speed , rigidbody.velocity.y, faceDirection.z * speed);
			//if(pushDown)
				//rigidbody.AddForce (-Vector3.up*400f);
		    isMoving = Mathf.Abs(faceDirection.x) + Mathf.Abs(faceDirection.z) > 0.05f;
		}
	    if (!animationController) return;
        animationController.SetBool("Moving", isMoving);
        secondAnimationController.SetBool("Moving", true);
	}

    public void RotateTowardsDirection(Vector3 direction)
    {
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction),
                Time.deltaTime * rotateSpeed);
        }
    }

	void OnCollisionEnter(Collision coll)
	{
		if (coll.gameObject.name == "Level") {
			pushDown = false;
			rigidbody.velocity = Vector3.zero;
		}
	}

    void RemoveIndicator()
    {
        groundIndicator.enabled = false;
    }
}
