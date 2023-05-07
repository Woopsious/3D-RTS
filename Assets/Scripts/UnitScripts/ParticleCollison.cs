using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;

public class ParticleCollison : MonoBehaviour
{
	private ParticleSystem particle;
	private List<ParticleCollisionEvent> particleCollisionsEvents;

	public AudioSource WeaponHitAudio;
	public ParticleSystem WeaponHitParticles;

	public void Start()
	{
		particle = GetComponent<ParticleSystem>();
		particleCollisionsEvents = new List<ParticleCollisionEvent>();
	}

	public void OnParticleCollision(GameObject other)
	{
		ParticlePhysicsExtensions.GetCollisionEvents(particle, other, particleCollisionsEvents);
		PlayHitSFXVFX(particleCollisionsEvents[0]);
	}
	public void PlayHitSFXVFX(ParticleCollisionEvent particleCollision)
	{
		WeaponHitParticles.gameObject.transform.position = particleCollision.intersection;
		WeaponHitParticles.Play();
		WeaponHitAudio.Play();
	}
}
