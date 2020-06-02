using UnityEngine;
using System;
using Newtonsoft.Json;
using FrostweepGames.Plugins.Networking;
using FrostweepGames.Plugins.Core;

namespace FrostweepGames.Plugins.GoogleCloud.TextToSpeech
{
    public class TextToSpeechManager : IService, IDisposable, ITextToSpeechManager
    {
        public event Action<GetVoicesResponse> GetVoicesSuccessEvent;
        public event Action<PostSynthesizeResponse> SynthesizeSuccessEvent;

        public event Action<string> GetVoicesFailedEvent;
        public event Action<string> SynthesizeFailedEvent;

        private NetworkingService _networking;
        private GCTextToSpeech _gcTextToSpeech;

        public void Init()
        {
            _gcTextToSpeech = GCTextToSpeech.Instance;

            _networking = new NetworkingService();
            _networking.NetworkResponseEvent += NetworkResponseEventHandler;
        }

        public void Update()
        {
            _networking.Update();
        }

        public void Dispose()
        {
            _networking.NetworkResponseEvent -= NetworkResponseEventHandler;
            _networking.Dispose();
        }

        public string PrepareLanguage(Enumerators.LanguageCode lang)
        {
            return lang.ToString().Replace("_", "-");
        }

        public void GetVoices(GetVoicesRequest getVoicesRequest)
        {
            string uri = Constants.GET_LIST_VOICES;

            if (!_gcTextToSpeech.isUseAPIKeyFromPrefab)
                uri += Constants.API_KEY_PARAM + Constants.GC_API_KEY;
            else
                uri += Constants.API_KEY_PARAM + _gcTextToSpeech.apiKey;

            uri += "&languageCode=" + getVoicesRequest.languageCode;

            _networking.SendRequest(uri, string.Empty, NetworkEnumerators.RequestType.GET,
                new object[] { Enumerators.GoogleCloudRequestType.GET_VOICES });
        }

        public void Synthesize(PostSynthesizeRequest synthesizeRequest)
        {
            string uri = Constants.POST_TEXT_SYNTHESIZE;

            if (!_gcTextToSpeech.isUseAPIKeyFromPrefab)
                uri += Constants.API_KEY_PARAM + Constants.GC_API_KEY;
            else
                uri += Constants.API_KEY_PARAM + _gcTextToSpeech.apiKey;

            _networking.SendRequest(uri, JsonConvert.SerializeObject(synthesizeRequest), NetworkEnumerators.RequestType.POST, 
                new object[] { Enumerators.GoogleCloudRequestType.SYNTHESIZE });
        }

        private void NetworkResponseEventHandler(NetworkResponse response)
        {
            Enumerators.GoogleCloudRequestType googleCloudRequestType = (Enumerators.GoogleCloudRequestType)response.Parameters[0];

            if (!string.IsNullOrEmpty(response.Error))
            {
                ThrowFailedEvent(response.Error + "; " + response.Response, googleCloudRequestType);
            }
            else
            {
                if (response.Response.Contains("error"))
                {
                    if (_gcTextToSpeech.isFullDebugLogIfError)
                        Debug.Log(response.Error + "\n" + response.Response);

                    ThrowFailedEvent(response.Response, googleCloudRequestType);
                }
                else
                    ThrowSuccessEvent(response.Response, googleCloudRequestType);
            }
        }

        private void ThrowFailedEvent(string error, Enumerators.GoogleCloudRequestType type)
        {
            switch (type)
            {
                case Enumerators.GoogleCloudRequestType.GET_VOICES:
                    {
                        if (GetVoicesFailedEvent != null)
                            GetVoicesFailedEvent(error);
                    }
                    break;
                case Enumerators.GoogleCloudRequestType.SYNTHESIZE:
                    {
                        if (SynthesizeFailedEvent != null)
                            SynthesizeFailedEvent(error);
                    }
                    break;
                default: break;
            }
        }

        private void ThrowSuccessEvent(string data, Enumerators.GoogleCloudRequestType type)
        {
            switch (type)
            {
                case Enumerators.GoogleCloudRequestType.GET_VOICES:
                    {
                        if (GetVoicesSuccessEvent != null)
                            GetVoicesSuccessEvent(JsonConvert.DeserializeObject<GetVoicesResponse>(data));
                    }
                    break;        
                case Enumerators.GoogleCloudRequestType.SYNTHESIZE:
                    {
                        if (SynthesizeSuccessEvent != null)
                            SynthesizeSuccessEvent(JsonConvert.DeserializeObject<PostSynthesizeResponse>(data));
                    }
                    break;
                default: break;
            }
        }
    }
}