using Cross.Sdk.Unity;
using Cross.Sdk.Unity.Model;
using Cross.Core.Common.Logging;
using Skibitsky.Unity;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityLogger = Cross.Sign.Unity.UnityLogger;

#if !UNITY_WEBGL
using mixpanel;
#endif

namespace Sample
{
    public class CrossSdkInit : MonoBehaviour
    {
        [SerializeField] private SceneReference _menuScene;

        private async void Start()
        {
            // Set up Cross logger to collect logs from CrossSdk
            CrossLogger.Instance = new UnityLogger();

            // CrossSdk configuration
            var CrossSdkConfig = new CrossSdkConfig
            {
                // Project ID provided by cross team
                projectId = "ef21cf313a63dbf63f2e9e04f3614029",
                metadata = new Metadata(
                    "CrossSdk Unity",   // your project name
                    "CrossSdk Unity Sample",    // your project description
                    "https://to.nexus",     // your project website
                    "https://contents.crosstoken.io/wallet/token/images/CROSSx.svg",    // your project logo icon
                    new RedirectData
                    {
                        // Used by native wallets to redirect back to the app after approving requests
                        Native = "cross-sdk-unity-sample://wc"
                    }
                )
            };

            Debug.Log("[CrossSdk Init] Initializing CrossSdk...");

            await CrossSdk.InitializeAsync(
                CrossSdkConfig
            );
#if !UNITY_WEBGL
            // The Mixpanel is used by the sample project to collect telemetry
            var clientId = await CrossSdk.Instance.SignClient.CoreClient.Crypto.GetClientId();
            Mixpanel.Identify(clientId);
#endif
            Debug.Log($"[CrossSdk Init] CrossSdk initialized. Loading menu scene...");
            SceneManager.LoadScene(_menuScene);
        }
    }
}