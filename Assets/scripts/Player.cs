﻿using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {
    public Laser m_LaserPrefab;
    public Rocket m_RocketPrefab;

    public float m_playerSpeed = 0.1f;
    public Vector3 m_direction = new Vector3(1.0f, 0.0f, 0.0f);
    private Vector3 m_AimDirection = new Vector3(1.0f, 0.0f, 0.0f);

    private Animator m_animator;
    private GameObject m_LaserSpawner;
    private GameObject m_RocketSpawner;

    public float m_LaserPerSecond = 100.0f;
    private float m_lastLaser;
    public float m_RocketPerSecond = 10.0f;
    private float m_lastRocket;


    public bool m_CanShotAndWalk = false;
    private bool m_IsShooting = false;

    private Rigidbody2D m_body;

    private LifeManager m_lifeManager;

    public GameObject m_DeathExplosion;

	// Use this for initialization
	void Start () {
		Time.timeScale = 1.0f;
        m_lastLaser = 0.0f;
        m_lastRocket = 0.0f;

	    m_direction = new Vector3(1.0f, 0.0f, 0.0f);
        m_AimDirection = new Vector3(1.0f, 0.0f, 0.0f);
        m_animator = GetComponent<Animator>();
        m_body = GetComponent<Rigidbody2D>();

        m_LaserSpawner = transform.Find("LaserSpawner").gameObject;
        m_RocketSpawner = transform.Find("RocketSpawner").gameObject;
        m_lifeManager = GetComponent<LifeManager>();

        m_lifeManager.OnDeath += () => {
            Instantiate(m_DeathExplosion, transform.position, transform.rotation);
            Destroy(GetComponent<SpriteRenderer>());
        };

        ScoreManager sm = GetComponent<ScoreManager>();
        sm.OnScoreChange += () => {
            print(sm.getScore().ToString());
        };
	}
	
	// Update is called once per frame
	void Update () {
	}

    void FixedUpdate() {
        if (!m_lifeManager.IsDead()) {
            m_lastLaser = m_lastLaser + Time.deltaTime;
            m_lastRocket = m_lastRocket + Time.deltaTime;

            Vector2 direction2D = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            if (((direction2D.x != 0) || (direction2D.y != 0)) && (m_CanShotAndWalk || (!m_IsShooting))) {
                direction2D.Normalize();
                m_direction = new Vector3(direction2D.x, direction2D.y, 0);
				m_body.velocity = new Vector2(direction2D.x * m_playerSpeed * Time.deltaTime, direction2D.y * m_playerSpeed * Time.deltaTime);
                //m_body.AddForce();

                m_animator.SetBool("walking", true);
            } else {
                m_body.velocity = Vector2.zero;
                m_animator.SetBool("walking", false);
            }

            float ax = Input.GetAxis("AimX");
            float ay = Input.GetAxis("AimY");
            if ((ax != 0) || (ay != 0)) {
                Vector2 dir2 = new Vector2(ax, ay);
                dir2.Normalize();
                m_AimDirection = new Vector3(dir2.x, dir2.y, 0.0f);
            }
            float angle = Vector3.Angle(m_AimDirection, new Vector3(0.0f, 1.0f, 0.0f));
            if (m_AimDirection.x > 0.0f) {
                angle = 360.0f - angle;
            }
            transform.localEulerAngles = new Vector3(0.0f, 0.0f, angle);
            if (Input.GetAxis("Fire1") >= 0.5f) {
                m_animator.SetBool("firingLaser", true);
                m_IsShooting = true;
                if (canShootLaser()) {
                    shootLaser();
                }
            } else if (Input.GetAxis("Fire1") <= -0.5f) {
                m_IsShooting = true;
                if (canShootRocket()) {
					m_animator.SetTrigger("launchRocket");
					StartCoroutine(shootRocket());
                }
            } else {
                m_IsShooting = false;
                m_animator.SetBool("firingLaser", false);
            }
        } else {
            m_body.velocity = Vector2.zero;
            m_animator.SetBool("walking", false);
            m_animator.SetBool("firingLaser", false);

			int score = GetComponent<ScoreManager>().getScore();
			PlayerPrefs.SetInt("score", score);

			int best = PlayerPrefs.HasKey("bestscore") ? PlayerPrefs.GetInt("bestscore") : 0;

			if (score > best)
				PlayerPrefs.SetInt("bestscore", score);

			PlayerPrefs.Save();

			Application.LoadLevel(2);
        }
    }

    void shootLaser() {
        m_lastLaser = 0.0f;
        Vector3 position = transform.position;
        if (m_LaserSpawner) {
            position = m_LaserSpawner.transform.position;
        }
        Laser laser = (Laser)Instantiate(m_LaserPrefab, position, gameObject.transform.rotation);
        laser.m_direction = m_AimDirection;
		
        laser.SetSpawner(this.gameObject);
    }

    bool canShootLaser() {
        return m_lastLaser > (1.0f / m_LaserPerSecond);
    }
    IEnumerator shootRocket() {
		m_lastRocket = 0.0f;
		yield return new WaitForSeconds(0.57f);
		Vector3 position = transform.position;
		if (m_RocketSpawner) {
			position = m_RocketSpawner.transform.position;
		}
        Rocket rocket = (Rocket)Instantiate(m_RocketPrefab, position, gameObject.transform.rotation);
        rocket.m_direction = m_AimDirection;
        rocket.SetSpawner(this.gameObject);
    }

    bool canShootRocket() {
        return m_lastRocket > (1.0f / m_RocketPerSecond);
    }

    void OnTriggerEnter2D(Collider2D hit) {
        if (!hit.gameObject.name.Equals("Agent")) {
            LifeManager manager = hit.gameObject.GetComponent<LifeManager>();
            if (manager != null) {
                //manager.DoDamage(m_damage);
            }
        }
    }

    public LifeManager getLifeManager() {
        return m_lifeManager;
    }
}
