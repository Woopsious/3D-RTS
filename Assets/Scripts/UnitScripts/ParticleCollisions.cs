using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;

public class ParticleCollisions : MonoBehaviour
{
	private ParticleSystem particle;
	private List<ParticleCollisionEvent> particleCollisionsEvents;
	public GameObject ParticleHitObj;

	public void Start()
	{
		particle = GetComponent<ParticleSystem>();
		particleCollisionsEvents = new List<ParticleCollisionEvent>();
	}

	public void OnParticleCollision(GameObject other)
	{
		ParticlePhysicsExtensions.GetCollisionEvents(particle, other, particleCollisionsEvents);

		GameObject go = Instantiate(ParticleHitObj);
		go.transform.position = particleCollisionsEvents[0].intersection;
	}
}
