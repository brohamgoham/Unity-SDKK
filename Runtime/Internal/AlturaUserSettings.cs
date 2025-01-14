using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace AlturaNFT
{   using Internal;
    
    /// <summary>
    /// User settings API
    /// </summary>
    [ExecuteInEditMode]
    public class AlturaUserSettings : MonoBehaviour
    {

        #region Parameter Defines

            [Space(20)]
            [Header("Called When API call starts")]
            public UnityEvent OnRequestStarted;
            [Header("Called After Successful API call")]
            public UnityEvent afterSuccess;
            [Header("Called After Error API call")]
            public UnityEvent afterError;
            
            [Header("Run Component when this Game Object is Set Active")]
            [SerializeField] private bool onEnable = false;
            public bool debugErrorLog = true;
            public bool debugLogRawApiResponse = false;

            [Header("Response")]
            public User_model model;
            
            private UnityAction<string> OnErrorAction;
            private UnityAction<User_model> OnCompleteAction;
            
            private string RequestUriInit = "https://api.alturanft.com/api/v2/user/verify_auth_code/";
            private string WEB_URL;
            private string _apiKey;
            private bool destroyAtEnd = false;
            private static bool running = false;
            
        #endregion
 
        
        private void Awake()
        {
            AlturaUser.Initialise();
            _apiKey = AlturaUser.GetUserApiKey();
            
        }
        
        private void OnEnable()
        {
            if (onEnable)
                Run();
        }
        
        /// <summary>
        /// Initialize creates a gameobject and assings this script as a component. This must be called if you are not refrencing the script any other way and it doesn't already exists in the scene.
        /// </summary>
        /// <param name="destroyAtEnd"> Optional bool parameter can set to false to avoid Spawned GameObject being destroyed after the Api process is complete. </param>
        public static AlturaUserSettings Initialize(bool destroyAtEnd = true)
        {
            string name = "AlturaNFT Initialize | Delete:";
            var existing = (GameObject.Find(name + destroyAtEnd));
            if (existing != null)
            {
                return existing.GetComponent<AlturaUserSettings>();
            }
            var _this = new GameObject(name + destroyAtEnd).AddComponent<AlturaUserSettings>();
            _this.destroyAtEnd = destroyAtEnd;
            _this.onEnable = false;
            _this.debugErrorLog = false;
            return _this;
        }

        /// <summary>
        /// Action on succesfull API Fetch.
        /// </summary>
        public AlturaUserSettings OnComplete(UnityAction<User_model> action)
        {
            this.OnCompleteAction = action;
            return this;
        }
            
        /// <summary>
        /// Action on Error
        /// </summary>
        /// <param name="UnityAction action"> string.</param>
        /// <returns> Information on Error as string text.</returns>
        public AlturaUserSettings OnError(UnityAction<string> action)
        {
            this.OnErrorAction = action;
            return this;
        }

        /// <summary>
        /// Runs ^_^
        /// </summary>
        public AlturaUserSettings Run()
        {
            if (!running)
            {
                running = true;
                WEB_URL = BuildUrl();
                StopAllCoroutines();
                StartCoroutine(CallAPIProcess());
                StartCoroutine(ForceEnd());
            }

            return this;
        }

        IEnumerator ForceEnd()
        {
            yield return new WaitForSeconds(5f);
            End();
        }

        private void OnDestroy()
        {
            running = false;
        }

        string BuildUrl()
        {
            WEB_URL = RequestUriInit;
            return WEB_URL;
        }


        private UnityWebRequest request;
        IEnumerator CallAPIProcess()
        {

            //Make request
            request = UnityWebRequest.Get(WEB_URL);
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("source",  AlturaUser.GetSource());
            request.SetRequestHeader("Authorization", _apiKey);
            string url = "https://api.alturanft.com/api/sdk/unity/";
            WWWForm form = new WWWForm();
            UnityWebRequest www = UnityWebRequest.Post(url + "AlturaUserSetting" + "?apiKey=" + _apiKey, form);
            //Make request
            if(OnRequestStarted!=null)
                OnRequestStarted.Invoke();
            yield return request.SendWebRequest();
            string jsonResult = System.Text.Encoding.UTF8.GetString(request.downloadHandler.data);
            
            if(debugLogRawApiResponse)
                Debug.Log(jsonResult);

            if (request.error != null)
            {
                if(OnErrorAction!=null)
                    OnErrorAction($"Response code: {request.responseCode}. Result {jsonResult}");
                if(debugErrorLog)
                    Debug.Log($"Response code: {request.responseCode}. Result {jsonResult}");
                if(afterError!=null)
                    afterError.Invoke();
            }
            else
            {
                //Fill Data Model from received class
               var model = JsonConvert.DeserializeObject<User_model>(jsonResult);
                        
                if(OnCompleteAction!=null)
                    OnCompleteAction.Invoke(model);
                        
                if(afterSuccess!=null)
                    afterSuccess.Invoke();
            }

            End();

        }

        public void End()
        {
            request.Dispose();
            if (destroyAtEnd)
            {
                if (Application.isEditor)
                    DestroyImmediate(gameObject);
                else
                    Destroy(this.gameObject);
            }
        }
    }
}
