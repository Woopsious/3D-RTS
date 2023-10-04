using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class WeatherSystem : MonoBehaviour
{
	public ParticleSystem snowParticleSystem;

	public readonly int maxSnowEmissionRate = 1000;
	public readonly int minSnowEmissionRate = 250;

	public readonly int maxVelocity = 10;
	public readonly int minVelocity = -10;

	//weather changes every 90s to 180s (set lower for now)
	public readonly float minWeatherTimer = 15;
	public readonly float maxWeatherTimer = 30;

	public float changeWeatherTimer;

	public void Start()
	{
		ChangeEmissionRate();
		ChangeFallVelocity();
		ChangeXDirectionVelocity();
		ChangeZDirectionVelocity();

		changeWeatherTimer = Random.Range(minWeatherTimer, maxWeatherTimer);
	}

	public void Update()
	{
		ChangeWeather();
	}
	public void ChangeWeather()
	{
		changeWeatherTimer -= Time.deltaTime;
		if (changeWeatherTimer < 0)
		{
			ChangeEmissionRate();
			ChangeXDirectionVelocity();
			ChangeZDirectionVelocity();

			changeWeatherTimer = Random.Range(minWeatherTimer, maxWeatherTimer);
		}
	}

	public void ChangeEmissionRate()
	{
		int newEmissionRate = Random.Range(minSnowEmissionRate, maxSnowEmissionRate);
		var emission = snowParticleSystem.emission;
		emission.rateOverTime = newEmissionRate;
	}
	public void ChangeXDirectionVelocity()
	{
		var vel = snowParticleSystem.velocityOverLifetime;
		int xVelocity = Random.Range(minVelocity, maxVelocity);

		vel.x = new ParticleSystem.MinMaxCurve(xVelocity - 2.5f, xVelocity + 2.5f);
	}
	public void ChangeZDirectionVelocity()
	{
		var vel = snowParticleSystem.velocityOverLifetime;
		int zVelocity = Random.Range(minVelocity, maxVelocity);

		vel.z = new ParticleSystem.MinMaxCurve(zVelocity - 2.5f, zVelocity + 2.5f);
	}
	public void ChangeFallVelocity()
	{
		var vel = snowParticleSystem.velocityOverLifetime;
		vel.y = new ParticleSystem.MinMaxCurve(-2f, -10f);
	}
}
