using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnnouncerSystem : MonoBehaviour
{
	public static AnnouncerSystem Instance;

	public AudioSource announcerSFX;

	[Header("Feedback Positive")]
    public List<AudioClip> actionsPositive;
    public List<AudioClip> actionsNegative;

	public AudioClip Mining;
	public AudioClip Moving;
	public AudioClip Engaging;

	[Header("Feedback Negative")]
	public AudioClip InvalidBuildingLocation;
	public AudioClip InvalidUnitPosition;

	[Header("Alerts")]
	public AudioClip Alert_BaseUnderAttack;
	public AudioClip Alert_BuildingLost;
	public AudioClip Alert_UnitUnderAttack;
	public AudioClip Alert_UnitLost;
	public AudioClip Alert_ResearchComplete;

	[Header("Flag Captures")]
	public AudioClip Flags_WeCapturedAFlag;
	public AudioClip Flags_WeCapturedEnemyFlag;
	public AudioClip Flags_EnemyCapturedAFlag;
	public AudioClip Flags_EnemyCapturedOurFlag;

	[Header("Other")]
	public AudioClip Victory;
	public AudioClip Defeat;

	public void Awake()
	{
		Instance = this;
	}

	//Positive Feedback
	public void PlayPositiveRepliesSFX()
	{
		CheckIfAudioIsPlaying();
		announcerSFX.clip = actionsPositive[GetRandomNumber(actionsPositive.Count)];
		announcerSFX.Play();
	}
	public void PlayPosReplyMiningSFX()
	{
		PlayAudioClip(Mining);
	}
	public void PlayPosReplyMovingSFX()
	{
		PlayAudioClip(Moving);
	}
	public void PlayPosReplyEngagingSFX()
	{
		PlayAudioClip(Engaging);
	}

	//Negative Feedback
	public void PlayNegativeRepliesSFX()
	{
		CheckIfAudioIsPlaying();
		announcerSFX.clip = actionsNegative[GetRandomNumber(actionsPositive.Count)];
		announcerSFX.Play();
	}
	public void PlayNegReplyInvalidBuildingLocationSFX()
	{
		PlayAudioClip(InvalidBuildingLocation);
	}
	public void PlayNegReplyInvalidUnitPositionSFX()
	{
		PlayAudioClip(InvalidUnitPosition);
	}

	//Alerts
	public void PlayAlertBaseUnderAttackSFX()
	{
		PlayAudioClip(Alert_BaseUnderAttack);
	}
	public void PlayAlertBuildingLostSFX()
	{
		PlayAudioClip(Alert_BuildingLost);
	}
	public void PlayAlertUnitUnderAttackSFX()
	{
		PlayAudioClip(Alert_UnitLost);
	}
	public void PlayAlertUnitLostSFX()
	{
		PlayAudioClip(Alert_UnitLost);
	}
	public void PlayAlertResearchCompleteSFX()
	{
		PlayAudioClip(Alert_ResearchComplete);
	}

	//Flag Captures
	public void PlayFlagsWeCapturedAFlagSFX()
	{
		PlayAudioClip(Flags_WeCapturedAFlag);
	}
	public void PlayFlagsWeCapturedEnemyFlagSFX()
	{
		PlayAudioClip(Flags_WeCapturedEnemyFlag);
	}
	public void PlayFlagsEnemyCapturedAFlagSFX()
	{
		PlayAudioClip(Flags_EnemyCapturedAFlag);
	}
	public void PlayFlagsEnemyCapturedOurFlagSFX()
	{
		PlayAudioClip(Flags_EnemyCapturedOurFlag);
	}

	//Victory/Defeat
	public void PlayVictorySFX()
	{
		PlayAudioClip(Victory);
	}
	public void PlayDefeatSFX()
	{
		PlayAudioClip(Defeat);
	}

	//reusable functions
	private int GetRandomNumber(int max)
	{
		return Random.Range(0, max);
	}
	private void CheckIfAudioIsPlaying()
	{
		if (announcerSFX.isPlaying)
			announcerSFX.Stop();
	}
	private void PlayAudioClip(AudioClip clip)
	{
		CheckIfAudioIsPlaying();
		announcerSFX.clip = clip;
		announcerSFX.Play();
	}
}
