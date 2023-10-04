using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class WeatherSystem : MonoBehaviour
{
	public ParticleSystem snowParticleSystem;

	public readonly int maxSnowEmissionRate = 1000;
	public readonly int minSnowEmissionRate = 250;

	public int newSnowEmissionRate;

	public readonly int maxVelocity = 10;
	public readonly int minVelocity = -10;

	public readonly float maxFogDensity = 0.035f;
	public readonly float minFogDensity = 0.01f;

	public float newFogDensity;

	//weather changes every 90s to 180s (set lower for now)
	public readonly float minWeatherTimer = 10;
	public readonly float maxWeatherTimer = 15;

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
			ChangeFogDensity();

			changeWeatherTimer = Random.Range(minWeatherTimer, maxWeatherTimer);
		}
	}

	//functions to change fog density
	public void ChangeFogDensity()
	{
		newFogDensity = (float)newSnowEmissionRate / 25000;

		if (RenderSettings.fogDensity < newFogDensity)
			StartCoroutine(IncreaseFogDensityOvertime());
		else if (RenderSettings.fogDensity > newFogDensity)
			StartCoroutine(DecreaseFogDensityOvertime());
	}
	public IEnumerator IncreaseFogDensityOvertime()
	{
		yield return new WaitForSeconds(0.1f);

		if (RenderSettings.fogDensity < newFogDensity)
		{
			RenderSettings.fogDensity += 0.0001f;
			StartCoroutine(IncreaseFogDensityOvertime());
		}
	}
	public IEnumerator DecreaseFogDensityOvertime()
	{
		yield return new WaitForSeconds(0.1f);

		if (RenderSettings.fogDensity > newFogDensity)
		{
			RenderSettings.fogDensity -= 0.0001f;
			StartCoroutine(DecreaseFogDensityOvertime());
		}
	}

	//functions to change snow particle effects
	public void ChangeEmissionRate()
	{
		newSnowEmissionRate = Random.Range(minSnowEmissionRate, maxSnowEmissionRate);
		var emission = snowParticleSystem.emission;
		emission.rateOverTime = newSnowEmissionRate;
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
