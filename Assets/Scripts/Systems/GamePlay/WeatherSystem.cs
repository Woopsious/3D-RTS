using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class WeatherSystem : NetworkBehaviour
{
	public ParticleSystem snowParticleSystem;

	public readonly int maxSnowEmissionRate = 1000;
	public readonly int minSnowEmissionRate = 250;

	public readonly int maxVelocity = 10;
	public readonly int minVelocity = -10;

	public NetworkVariable<int> newSnowEmissionRate;
	public NetworkVariable<float> newXDirectionConstant;
	public NetworkVariable<float> newZDirectionConstant;

	public readonly float maxFogDensity = 0.035f;
	public readonly float minFogDensity = 0.01f;

	public NetworkVariable<float> newFogDensity;

	//weather changes every 90s to 180s (set lower for now)
	public readonly float minWeatherTimer = 15;
	public readonly float maxWeatherTimer = 20;

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
		if (MultiplayerManager.Instance.IsHost)
			GetNewWeatherSettings();
	}
	public void GetNewWeatherSettings()
	{
		changeWeatherTimer -= Time.deltaTime;
		if (changeWeatherTimer < 0)
		{
			GetNewSnowEmisionRate();
			GetNewXDirectionVelocity();
			GetNewZDirectionVelocity();

			MultiplayerManager.Instance.SyncWeatherServerRPC();

			changeWeatherTimer = Random.Range(minWeatherTimer, maxWeatherTimer);
		}
	}
	public void ChangeWeather()
	{
		ChangeEmissionRate();
		ChangeXDirectionVelocity();
		ChangeZDirectionVelocity();
		ChangeFallVelocity();
		ChangeFogDensity();
	}

	//functions to change snow particle effects
	public void GetNewSnowEmisionRate()
	{
		newSnowEmissionRate.Value = Random.Range(minSnowEmissionRate, maxSnowEmissionRate);
	}
	public void GetNewXDirectionVelocity()
	{
		int xVelocity = Random.Range(minVelocity, maxVelocity);
		newXDirectionConstant.Value = xVelocity;
	}
	public void GetNewZDirectionVelocity()
	{
		int zVelocity = Random.Range(minVelocity, maxVelocity);
		newZDirectionConstant.Value = zVelocity;
	}
	public void ChangeEmissionRate()
	{
		var emission = snowParticleSystem.emission;
		emission.rateOverTime = newSnowEmissionRate.Value;
	}
	public void ChangeXDirectionVelocity()
	{
		var vel = snowParticleSystem.velocityOverLifetime;
		vel.x = new ParticleSystem.MinMaxCurve(newXDirectionConstant.Value - 2.5f, newXDirectionConstant.Value + 2.5f);
	}
	public void ChangeZDirectionVelocity()
	{
		var vel = snowParticleSystem.velocityOverLifetime;
		vel.z = new ParticleSystem.MinMaxCurve(newZDirectionConstant.Value - 2.5f, newZDirectionConstant.Value + 2.5f);
	}
	public void ChangeFallVelocity()
	{
		var vel = snowParticleSystem.velocityOverLifetime;
		vel.y = new ParticleSystem.MinMaxCurve(-2f, -10f);
	}

	//functions to change fog density
	public void ChangeFogDensity()
	{
		float snowEmissionRate = newSnowEmissionRate.Value;
		newFogDensity.Value = (float)snowEmissionRate / 25000;

		if (RenderSettings.fogDensity < newFogDensity.Value)
			StartCoroutine(IncreaseFogDensityOvertime());
		else if (RenderSettings.fogDensity > newFogDensity.Value)
			StartCoroutine(DecreaseFogDensityOvertime());
	}
	public IEnumerator IncreaseFogDensityOvertime()
	{
		yield return new WaitForSeconds(0.1f);

		if (RenderSettings.fogDensity < newFogDensity.Value)
		{
			RenderSettings.fogDensity += 0.0001f;
			StartCoroutine(IncreaseFogDensityOvertime());
		}
	}
	public IEnumerator DecreaseFogDensityOvertime()
	{
		yield return new WaitForSeconds(0.1f);

		if (RenderSettings.fogDensity > newFogDensity.Value)
		{
			RenderSettings.fogDensity -= 0.0001f;
			StartCoroutine(DecreaseFogDensityOvertime());
		}
	}
}
