using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using FrostweepGames.Plugins.Core;
using FrostweepGames.Plugins.Native;
using System.Collections;

#if UNITY_2018_3_OR_NEWER && UNITY_ANDROID && !NET_2_0 && !NET_2_0_SUBSET
using UnityEngine.Android;
#endif

namespace FrostweepGames.Plugins.GoogleCloud.SpeechRecognition
{
    public class MediaManager : IService, IMediaManager
    {
		public event Action MicrophoneDeviceSelectedEvent;

		public event Action RecordStartedEvent;
        public event Action RecordFailedEvent;
		public event Action<AudioClip> RecordEndedEvent;

		public event Action TalkBeganEvent;
        public event Action<AudioClip> TalkEndedEvent;

		private IVoiceDetectionManager _voiceDetectionManager;

		private ISpeechRecognitionManager _speechRecognitionManager;

		private AudioClip _microphoneWorkingAudioClip;

        private int _currentSamplePosition;

        private int _previousSamplePosition;

        private float[] _currentAudioSamples;

        private bool _isTalking;

        private List<float> _currentRecordingVoice;

		private float _maxVoiceFrame;

		private float _endTalkingDelay;

        public bool IsRecording { get; private set; }
		public string MicrophoneDevice { get; private set; }
		public AudioClip LastRecordedClip { get; private set; }
		public bool DetectVoice { get; private set; }

		public void Init()
        {
			_voiceDetectionManager = ServiceLocator.Get<IVoiceDetectionManager>();
			_speechRecognitionManager = ServiceLocator.Get<ISpeechRecognitionManager>();
		}

		public void Update()
        {
            if (IsRecording)
            {
                _currentSamplePosition = CustomMicrophone.GetPosition(MicrophoneDevice);
                _microphoneWorkingAudioClip.GetData(_currentAudioSamples, 0);

                if (DetectVoice)
                {
                    bool isTalking = _voiceDetectionManager.HasDetectedVoice(AudioClip2ByteConverter.FloatToByte(_currentAudioSamples));

					if (isTalking)
					{
						_endTalkingDelay = 0f;	
					}
					else
					{
						_endTalkingDelay += Time.deltaTime;
					}

                    if (!_isTalking && isTalking)
                    {
                        _isTalking = true;

#if NET_2_0 || NET_2_0_SUBSET
						if (TalkBeganEvent != null)
							TalkBeganEvent();
#else
						TalkBeganEvent?.Invoke();
#endif
					}
					else if (_isTalking && !isTalking && _endTalkingDelay >= _speechRecognitionManager.CurrentConfig.voiceDetectionEndTalkingDelay)
                    {
						_isTalking = false;

						LastRecordedClip = AudioConvert.Convert(_currentRecordingVoice.ToArray(), _microphoneWorkingAudioClip.channels);

#if NET_2_0 || NET_2_0_SUBSET
						if (TalkEnded != null)
							TalkEnded(LatestVoiceAudioClip);
#else
						TalkEndedEvent?.Invoke(LastRecordedClip);
#endif
						_currentRecordingVoice.Clear();
                    }
                    else if (_isTalking && isTalking)
                    {
                        AddAudioSamplesIntoBuffer();
                    }
                }
                else
                {
                    AddAudioSamplesIntoBuffer();
                }

                _previousSamplePosition = _currentSamplePosition;
            }
        }

        public void Dispose()
        {
			if (_microphoneWorkingAudioClip != null)
			{
				MonoBehaviour.Destroy(_microphoneWorkingAudioClip);
				_microphoneWorkingAudioClip = null;
			}

			if (LastRecordedClip != null)
			{
				MonoBehaviour.Destroy(LastRecordedClip);
				LastRecordedClip = null;
			}
        }

		public float GetLastFrame()
		{
			int minValue = 16000 / 8;

			if (_currentRecordingVoice == null || _currentRecordingVoice.Count < minValue)
				return 0;

			int position = Mathf.Clamp(_currentRecordingVoice.Count - (minValue + 1), 0, _currentRecordingVoice.Count-1);

			float sum = 0f;
			for(int i = position; i < _currentRecordingVoice.Count; i++)
			{
				sum += Mathf.Abs(_currentRecordingVoice[i]);
			}

			sum /= minValue;

			return sum;
		}

		public float GetMaxFrame()
		{
			return _maxVoiceFrame;
		}

		public void StartRecord(bool withVoiceDetection = false)
		{
			if (IsRecording)
				return;

			if(!ReadyToRecord())
			{
#if NET_2_0 || NET_2_0_SUBSET
				if (RecordFailedEvent != null)
					RecordFailedEvent();
#else
				RecordFailedEvent?.Invoke();
#endif
				return;
			}

			DetectVoice = withVoiceDetection;

			_maxVoiceFrame = 0;

			_currentRecordingVoice = new List<float>();

			if (_microphoneWorkingAudioClip != null)
			{
				MonoBehaviour.Destroy(_microphoneWorkingAudioClip);
			}

			if (LastRecordedClip != null)
			{
				MonoBehaviour.Destroy(LastRecordedClip);
			}

			_microphoneWorkingAudioClip = CustomMicrophone.Start(MicrophoneDevice, true, 1, 16000);

			_currentAudioSamples = new float[_microphoneWorkingAudioClip.samples * _microphoneWorkingAudioClip.channels];

			IsRecording = true;

#if NET_2_0 || NET_2_0_SUBSET
			if (RecordStartedEvent != null)
				RecordStartedEvent();
#else
			RecordStartedEvent?.Invoke();
#endif
		}

		public void StopRecord()
		{
			if (!IsRecording || !ReadyToRecord())
				return;

			IsRecording = false;

			CustomMicrophone.End(MicrophoneDevice);

			if (!DetectVoice)
			{
				LastRecordedClip = AudioConvert.Convert(_currentRecordingVoice.ToArray(), _microphoneWorkingAudioClip.channels);
			}

			if (_currentRecordingVoice != null)
			{
				_currentRecordingVoice.Clear();
			}

			_currentAudioSamples = null;
			_currentRecordingVoice = null;

#if NET_2_0 || NET_2_0_SUBSET
			if (RecordEndedEvent != null)
				RecordEndedEvent(LatestVoiceAudioClip);
#else
			RecordEndedEvent?.Invoke(LastRecordedClip);
#endif
		}

		public bool ReadyToRecord()
		{
			return HasConnectedMicrophoneDevices() && !string.IsNullOrEmpty(MicrophoneDevice);
		}

		public bool HasConnectedMicrophoneDevices()
		{
			return CustomMicrophone.HasConnectedMicrophoneDevices();
		}

		public void SetMicrophoneDevice(string device)
		{
			if(MicrophoneDevice == device)
			{
				Debug.LogWarning("you are trying to select microphone device that already selected");
				return;
			}

			MicrophoneDevice = device;

#if NET_2_0 || NET_2_0_SUBSET
			if (MicrophoneDeviceSelectedEvent != null)
				MicrophoneDeviceSelectedEvent();
#else
			MicrophoneDeviceSelectedEvent?.Invoke();
#endif
		}

		public string[] GetMicrophoneDevices()
		{
			return CustomMicrophone.GetMicrophoneDevices();
		}

		public void SaveLastRecordedAudioClip(string path)
		{
			if (LastRecordedClip != null)
			{
				try
				{
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_ANDROID || UNITY_IPHONE
					File.WriteAllBytes(path, AudioClip2PCMConverter.AudioClip2PCM(LastRecordedClip));
#endif
				}
				catch (Exception e)
				{
					Debug.LogException(e);
				}
			}
		}


		public IEnumerator OneTimeRecord(int durationSec, Action<AudioClip> callback, int sampleRate = 16000)
		{
			AudioClip clip = CustomMicrophone.Start(MicrophoneDevice, false, durationSec, sampleRate);

			yield return new WaitForSeconds(durationSec);

			CustomMicrophone.End(MicrophoneDevice);

#if !NET_2_0 && !NET_2_0_SUBSET
			callback?.Invoke(clip);
#else
			if (callback != null)
			{
				callback(clip);
			}
#endif
		}

#if UNITY_2018_3_OR_NEWER && !NET_2_0 && !NET_2_0_SUBSET
		public bool HasMicrophonePermission()
		{
#if UNITY_ANDROID
			return Permission.HasUserAuthorizedPermission(Permission.Microphone);
#else
			return true;
#endif
		}

		/// <summary>
		/// Currently works as synchronous function with callback when app unpauses
		/// could not work properly if has enabled checkbox regarding additional frame in pause
		/// </summary>
		/// <param name="callback"></param>
		public void RequestMicrophonePermission(Action<bool> callback)
		{
#if UNITY_ANDROID
			Permission.RequestUserPermission(Permission.Microphone);
#endif
			callback?.Invoke(HasMicrophonePermission());
		}
#endif

		private void AddAudioSamplesIntoBuffer()
		{
			if (_previousSamplePosition > _currentSamplePosition)
			{
				for (int i = _previousSamplePosition; i < _currentAudioSamples.Length; i++)
				{
					_currentRecordingVoice.Add(_currentAudioSamples[i]);

					if (_currentAudioSamples[i] > _maxVoiceFrame)
						_maxVoiceFrame = _currentAudioSamples[i];
				}

				_previousSamplePosition = 0;
			}

			for (int i = _previousSamplePosition; i < _currentSamplePosition; i++)
			{
				_currentRecordingVoice.Add(_currentAudioSamples[i]);

				if (_currentAudioSamples[i] > _maxVoiceFrame)
					_maxVoiceFrame = _currentAudioSamples[i];
			}

			_previousSamplePosition = _currentSamplePosition;
		}
	}
}