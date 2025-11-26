using System.Threading.Tasks;
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
            // Wait for CrossSdk Instance to be set in Awake()
            while (CrossSdk.Instance == null)
            {
                await Task.Yield();
            }
            
            Debug.Log("[CrossSdk Init] CrossSdk Instance found!");
            
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
                ),
                // SIWE (Sign-In with Ethereum) configuration for Connect + Auth
                siweConfig = new SiweConfig
                {
                    // Note: SIWE is automatically enabled when using Authenticate() methods
                    // Use Connect() for regular connection, Authenticate() for SIWE authentication
                    
                    // Generate nonce for SIWE message (for production, get from backend!)
                    GetNonce = async () =>
                    {
                        Debug.Log("[SIWE] Generating nonce...");
                        return SiweUtils.GenerateNonce();
                    },
                    
                    // SIWE message parameters
                    GetMessageParams = () => new SiweMessageParams
                    {
                        Domain = "cross-sdk-unity-sample", // app or domain name
                        Uri = "https://to.nexus", // app or domain url
                        Statement = "Sign in to Cross SDK Unity Sample with your wallet" // statement to be signed by the wallet
                    },
                    
                    // Store session after successful authentication
                    GetSession = async (args) =>
                    {
                        var session = new SiweSession(args);
                        Debug.Log($"[SIWE] ✅ Authentication successful! Address: {args.Address}");
                        return session;
                    },
                    
                    // Sign out handler
                    SignOut = async () =>
                    {
                        Debug.Log("[SIWE] 🔒 Signed out");
                    },
                    
                    // Disable automatic SIWE modal (we control it manually)
                    OpenSiweViewOnSignatureRequest = false,
                    SignOutOnWalletDisconnect = true,
                    SignOutOnAccountChange = true,
                    SignOutOnChainChange = true
                }
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