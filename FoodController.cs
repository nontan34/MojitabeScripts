using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodController : MonoBehaviour {

	float life;
	float delta=0;

	// Use this for initialization
	void Start () {
		life = Random.Range (30.0f, 90.0f);
	}
	
	// Update is called once per frame
	void Update () {
		delta += Time.deltaTime;
		if (delta > 1.0f) {
			delta = 0;
			life -= 1.0f;
		}

		if (life < 0.0f) {
			Destroy (this.gameObject);
		}
	}
}
