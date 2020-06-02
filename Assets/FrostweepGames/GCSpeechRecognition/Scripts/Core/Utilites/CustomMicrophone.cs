using UnityEngine;

namespace FrostweepGames.Plugins.Native
{
	public class CustomMicrophone
	{
		public static AudioClip Start(string deviceName, bool loop, int lengthSec, int frequency)
		{
#if UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE || UNITY_EDITOR || UNITY_WSA
			return Microphone.Start(deviceName, loop, lengthSec, frequency);
#elif UNITY_WEBGL
			throw new System.NotImplementedException("WEBGL microphone not implemented yet");
#else
			throw new System.NotImplementedException("microphone not implemented yet");
#endif	
		}

		public static bool IsRecording(string deviceName)
		{
#if UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE || UNITY_EDITOR || UNITY_WSA
			return Microphone.IsRecording(deviceName);
#elif UNITY_WEBGL
			// todo improve
			return false;
#else
			return false;
#endif
		}

		public static void GetDeviceCaps(string deviceName, out int minFreq, out int maxfreq)
		{
#if UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE || UNITY_EDITOR || UNITY_WSA
			Microphone.GetDeviceCaps(deviceName, out minFreq, out maxfreq);
#elif UNITY_WEBGL
			// todo improve
			minFreq = 0;
			maxfreq = 0;
#else
			minFreq = 0;
			maxfreq = 0;
#endif
		}

		public static int GetPosition(string deviceName)
		{
#if UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE || UNITY_EDITOR || UNITY_WSA
			return Microphone.GetPosition(deviceName);
#elif UNITY_WEBGL
			// todo improve
			return 0;
#else
			return 0;
#endif
		}

		public static void End(string deviceName)
		{
#if UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE || UNITY_EDITOR || UNITY_WSA
			Microphone.End(deviceName);
#elif UNITY_WEBGL
			throw new System.NotImplementedException("WEBGL microphone not implemented yet");
#else
			throw new System.NotImplementedException("microphone not implemented yet");
#endif
		}

		public static bool HasConnectedMicrophoneDevices()
		{
#if UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE || UNITY_EDITOR || UNITY_WSA
			return Microphone.devices.Length > 0;
#elif UNITY_WEBGL
			// todo improve
			return false;
#else
			return false;
#endif
		}

		public static string[] GetMicrophoneDevices()
		{
#if UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE || UNITY_EDITOR || UNITY_WSA
			return Microphone.devices;
#elif UNITY_WEBGL
			// todo improve
			return new string[0];
#else
			return new string[0];
#endif
		}
	}
}