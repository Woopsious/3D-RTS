using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;

public class ParticleCollisions : MonoBehaviour
{
	private ParticleSystem particle;
	private List<ParticleCollisionEvent> particleCollisionsEvents;

	public AudioSource WeaponHitAudio;
	public GameObject ParticleParentObj;
	public ParticleSystem WeaponHitParticles;

	public GameObject ParticleTest;

	public void Start()
	{
		particle = GetComponent<ParticleSystem>();
		particleCollisionsEvents = new List<ParticleCollisionEvent>();
	}

	public void OnParticleCollision(GameObject other)
	{
		ParticlePhysicsExtensions.GetCollisionEvents(particle, other, particleCollisionsEvents);
		//PlayHitSFXVFX(particleCollisionsEvents[0]);

		GameObject go = Instantiate(ParticleTest);
		go.transform.position = particleCollisionsEvents[0].intersection;
	}
	public void PlayHitSFXVFX(ParticleCollisionEvent particleCollision)
	{
		ParticleParentObj.transform.position = particleCollision.intersection;
		WeaponHitParticles.Play();
		WeaponHitAudio.Play();
	}
}
