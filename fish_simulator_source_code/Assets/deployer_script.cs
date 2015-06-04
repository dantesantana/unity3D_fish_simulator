using UnityEngine;
using System.Collections;

public class deployer_script : MonoBehaviour {
	
	// the Boid prefab transform
	public Transform fishBoid;

	// Shark prefarb
	public Transform sharkBoid;

	// the magnitude of the random position
	float posMag = 10.0f;

	// initialise the boids
	void Start () {
		for (int i = 0; i < 15; i++) {
			Instantiate(fishBoid, new Vector3(Random.Range(-posMag, posMag), Random.Range(5.0f, 15.0f), Random.Range(-posMag, posMag)), Random.rotation);
		}
		for (int i = 0; i < 5; i++) {
			Instantiate(sharkBoid, new Vector3(Random.Range(-posMag, posMag) * 3, Random.Range(5.0f, 15.0f) * 3, Random.Range(-posMag, posMag) * 3), Random.rotation);
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}