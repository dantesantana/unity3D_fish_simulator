using UnityEngine;
using System.Collections;

public class shark_boid_script : MonoBehaviour {
	
	// the global centre of the simulation
	public Vector3 simulation_centre = new Vector3(0f, 10.0f, 0f);
	// the global radius of the simulation
	public float simulation_radius = 20f;
	// the boid maximum velocity
	public float max_speed = 3f;
	// the distance where sepration is applied
	public float separation_distance = 3.0f;
	// the distance where cohesion is applied
	public float cohesion_distance = 6.0f;
	// the sepration strength
	public float separation_strength = 50.0f;
	// the distance where cohesion is applied
	public float cohesion_strength = 1.0f;
	
	// position in front of boid used for wandering paths
	public Transform wanderMarker;

	// the list of Boid neighbours
	private GameObject [] boids;
	
	// the cohesive position
	private Vector3 cohesion_pos;
	
	// the boid index
	private int boid_index;

	// my stuff

	private GameObject[] fishBoids;

	private int fish_boid_index;

	private bool isHungry;

	private int hunger;

	public float swimForce;

	private float timeOfHunger;

	// Use this for initialization
	void Start () {
		// initialise to null (this script might start while other boids are still being created)
		boids = null;
		// set boid index
		boid_index = 0;
		// create the cohesion vector
		cohesion_pos = new Vector3 (0f, 0f, 0f);

		fish_boid_index = 0;

		isHungry = true;

		hunger = 0;

		swimForce = 0.8f;

		timeOfHunger = 100000000.0f;
	}
	
	// Update is called once per frame
	void Update() {
		// if the boids list is null
		if (boids == null || fishBoids == null) {
			// get the other boids
			boids = GameObject.FindGameObjectsWithTag ("sharkboid");
			fishBoids = GameObject.FindGameObjectsWithTag ("fishboid");
		} else {
			// hunger decays over time
			if (Time.time % 10 == 0) {
				hunger = hunger - 1;
			}
			if (hunger >= 3) {
				isHungry = false;
				gameObject.GetComponent<Renderer>().material.color = Color.green;
				// sharks becoming hungry faster when they're full
				if (Time.time > timeOfHunger + 5.0f) {
					hunger = hunger - 2;
				}
			} else {
				// hungry when less than 3 fish digesting
				isHungry = true;
			}

			// deal with the boid escape case - must stay within simulation_radius
			if (Vector3.Distance(simulation_centre, transform.position) > simulation_radius) {
				//
				float step = 5.0f * Time.deltaTime;
				// set target direction
				Vector3 targetDir = simulation_centre - transform.position;
				Vector3 newDir = Vector3.RotateTowards(transform.forward, targetDir, step, 0.0F);
				transform.rotation = Quaternion.LookRotation(newDir);
				transform.position =  Vector3.MoveTowards(transform.position, simulation_centre, step);
				return;
			}
			
			// increment the index of the boid list
			boid_index++;
			fish_boid_index++;
			if (fish_boid_index >= fishBoids.Length) {
				fish_boid_index = 0;
			}
			// if the time count exceed the time delta
			if (boid_index >= boids.Length) {
				// computer the scale
				Vector3 cohesive_force = (cohesion_strength/Vector3.Distance(cohesion_pos, transform.position))*(cohesion_pos - transform.position);
				// apply force
				GetComponent<Rigidbody>().AddForce(cohesive_force);
				// zero the time counter
				boid_index = 0;
				// zero the cohesion vector
				cohesion_pos.Set(0f, 0f, 0f);
			}
			// position of boid at index
			Vector3 pos = boids[boid_index].transform.position;
			Quaternion rot = boids[boid_index].transform.rotation;
			// the distance
			float dist = Vector3.Distance(transform.position, pos);
			if (isHungry){
				float preyDist = 1000.0f;
				Vector3 preyPos = simulation_centre;
				Vector3 predictedPreyPos;
				// decides which fish is closest
				for (int i = 0; i < fishBoids.Length; i++) {
					if (Vector3.Distance(fishBoids[i].transform.position, transform.position) < preyDist && fishBoids[i].tag == "fishboid") {
						preyDist = Vector3.Distance(fishBoids[i].transform.position, transform.position);
						preyPos = fishBoids[i].transform.position;
					}
				}
				predictedPreyPos = preyPos + preyDist * fishBoids[fish_boid_index].GetComponent<Rigidbody>().velocity;
				GetComponent<Rigidbody>().AddForce((predictedPreyPos - transform.position) * swimForce);
				transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(predictedPreyPos - transform.position), 7f);
				gameObject.GetComponent<Renderer>().material.color = Color.red;
			} else if (isHungry == false && dist > 0f) {// if not this boid
				if (dist <= separation_distance) { // if within separation
					// compute the scale of separation
					float scale = separation_strength/dist;
					// add a separation force between this boid and its neighbour
					GetComponent<Rigidbody>().AddForce(scale * Vector3.Normalize(transform.position - pos));
				} else if (dist <= cohesion_distance && dist > separation_distance) { // if within cohesive distance but not separation
					// compute the cohesive position
					cohesion_pos = cohesion_pos + pos*(1f/(float)boids.Length);
					// alignment - small rotations are applied based on the alignments of the neighbours
					transform.rotation = Quaternion.RotateTowards(transform.rotation, rot, 1f);
					GetComponent<Rigidbody>().AddForce(transform.forward);
				} 
				else {
					//wander when not near any fish
					Vector3 randomTarget = new Vector3(Random.Range(-360f, 360f),Random.Range(-360f, 360f),Random.Range(-360f, 360f));
					Vector3 wanderVariation = randomTarget + wanderMarker.position;
					Quaternion wanderRotation = Quaternion.LookRotation(wanderVariation);
					transform.rotation = Quaternion.RotateTowards(transform.rotation, wanderRotation, 2.0f);
					GetComponent<Rigidbody>().AddForce(transform.forward * 2.0f);
				}
			}
		}
	}

	void OnTriggerEnter(Collider other) {
		if (other.gameObject.tag == "fishboid" && isHungry) {
			hunger = hunger + 1;
			other.gameObject.tag = "deadfish";
			timeOfHunger = Time.time;
		}
	}
}
