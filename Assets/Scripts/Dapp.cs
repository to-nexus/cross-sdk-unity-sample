using System;
using System.Collections.Generic;
using System.Numerics;
using Nethereum.ABI.EIP712;
using Nethereum.JsonRpc.Client;
using Nethereum.Web3;
using Cross.Sdk.Unity;
using Cross.Core;
using Cross.Sign.Models;
using Cross.Sign.Nethereum.Model;
using UnityEngine;
using UnityEngine.UIElements;
using ButtonUtk = UnityEngine.UIElements.Button;
using Newtonsoft.Json;

namespace Sample
{
    public class Dapp : MonoBehaviour
    {
        [SerializeField] private UIDocument _uiDocument;

        private ButtonStruct[] _buttons;
        private VisualElement _buttonsContainer;

        private void Awake()
        {
            Application.targetFrameRate = Screen.currentResolution.refreshRate;

            _buttonsContainer = _uiDocument.rootVisualElement.Q<VisualElement>("ButtonsContainer");

            BuildButtons();
        }

        private void BuildButtons()
        {
            _buttons = new[]
            {
                new ButtonStruct
                {
                    Text = "Connect",
                    OnClick = OnConnectButton,
                    AccountRequired = false
                },
                new ButtonStruct
                {
                    Text = "Network",
                    OnClick = OnNetworkButton
                },
                new ButtonStruct
                {
                    Text = "Personal Sign",
                    OnClick = OnPersonalSignButton,
                    AccountRequired = true
                },
                new ButtonStruct
                {
                    Text = "Sign Typed Data",
                    OnClick = OnSignTypedDataV4Button,
                    AccountRequired = true
                },
                new ButtonStruct
                {
                    Text = "Send 1 Cross",
                    OnClick = OnSendNativeButton,
                    AccountRequired = true
                },
                new ButtonStruct
                {
                    Text = "Send 1 ERC20",
                    OnClick = OnSendERC20Button,
                    AccountRequired = true
                },
                new ButtonStruct
                {
                    Text = "Send 1 ERC20 with FeePayer",
                    OnClick = OnSendERC20ButtonWithFeePayer,
                    AccountRequired = true
                },
                new ButtonStruct
                {
                    Text = "Get Balance",
                    OnClick = OnGetBalanceButton,
                    AccountRequired = true
                },
                new ButtonStruct
                {
                    Text = "Get Tokens",
                    OnClick = OnGetTokensButton,
                    AccountRequired = true
                },
                new ButtonStruct
                {
                    Text = "Read Contract",
                    OnClick = OnReadContractClicked,
                    AccountRequired = true,
                    ChainIds = new HashSet<string>
                    {
                        "eip155:612044"
                    }
                },
                new ButtonStruct
                {
                    Text = "Disconnect",
                    OnClick = OnDisconnectButton,
                    AccountRequired = true
                }
            };
        }

        private void RefreshButtons()
        {
            _buttonsContainer.Clear();

            foreach (var button in _buttons)
            {
                if (button.ChainIds != null && !button.ChainIds.Contains(CrossSdk.NetworkController?.ActiveChain?.ChainId))
                    continue;

                var buttonUtk = new ButtonUtk
                {
                    text = button.Text
                };
                buttonUtk.clicked += button.OnClick;

                if (button.AccountRequired.HasValue)
                {
                    switch (button.AccountRequired)
                    {
                        case true when !CrossSdk.IsAccountConnected:
                            buttonUtk.SetEnabled(false);
                            break;
                        case true when CrossSdk.IsAccountConnected:
                            buttonUtk.SetEnabled(true);
                            break;
                        case false when CrossSdk.IsAccountConnected:
                            buttonUtk.SetEnabled(false);
                            break;
                        case false when !CrossSdk.IsAccountConnected:
                            buttonUtk.SetEnabled(true);
                            break;
                    }
                }

                _buttonsContainer.Add(buttonUtk);
            }
        }

        private async void Start()
        {
            if (!CrossSdk.IsInitialized)
            {
                Notification.ShowMessage("CrossSdk is not initialized. Please initialize CrossSdk first.");
                return;
            }

            RefreshButtons();

            try
            {
                CrossSdk.ChainChanged += (_, e) =>
                {
                    RefreshButtons();

                    if (e.NewChain == null)
                    {
                        Notification.ShowMessage("Unsupported chain");
                        return;
                    }
                };

                CrossSdk.AccountConnected += async (_, e) => RefreshButtons();

                CrossSdk.AccountDisconnected += (_, _) => RefreshButtons();

                CrossSdk.AccountChanged += (_, e) => RefreshButtons();

                // After the scene and UI are loaded, try to resume the session from the storage
                var sessionResumed = await CrossSdk.ConnectorController.TryResumeSessionAsync();
                Debug.Log($"Session resumed: {sessionResumed}");
            }
            catch (Exception e)
            {
                Notification.ShowMessage(e.Message);
                throw;
            }
        }

        public void OnConnectButton()
        {
            CrossSdk.Connect();
        }

        public void OnNetworkButton()
        {
            CrossSdk.OpenModal(ViewType.NetworkSearch);
        }

        // retrieve native coin from blockchain node
        public async void OnGetBalanceButton()
        {
            Debug.Log("[CrossSdk Sample] OnGetBalanceButton");

            try
            {
                Notification.ShowMessage("Getting balance with Cross API...");

                await CrossSdk.UpdateBalance(); // update from cross api
                var account = await CrossSdk.GetAccountAsync();
                var balance = await CrossSdk.Evm.GetBalanceAsync(account.Address);

                Notification.ShowMessage($"Balance: {Web3.Convert.FromWei(balance)} ETH");
            }
            catch (Exception e)
            {
                Notification.ShowMessage($"{nameof(RpcResponseException)}:\n{e.Message}");
                Debug.LogException(e, this);
            }
        }

        // retrieve all native coin and tokens from cross api
        public async void OnGetTokensButton()
        {
            try
            {
                Notification.ShowMessage("Getting tokens...");

                await CrossSdk.UpdateBalance(); // update from cross api
                var tokens = CrossSdk.GetTokens();
                
                string message = "Tokens:\n";

                foreach (var token in tokens)
                {
                    string symbol = token.Symbol;
                    string numeric = token.Quantity.numeric;
                    int decimals = int.Parse(token.Quantity.decimals);

                    var balance = Web3.Convert.FromWei(BigInteger.Parse(numeric), decimals);
                    message += $"{symbol}: {balance}\n";
                }

                Notification.ShowMessage(message);
            }
            catch (Exception e)
            {
                Notification.ShowMessage($"{nameof(RpcResponseException)}:\n{e.Message}");
                Debug.LogException(e, this);
            }
        }

        // sign message via wallet
        public async void OnPersonalSignButton()
        {
            Debug.Log("[CrossSdk Sample] OnPersonalSignButton");

            try
            {
                var account = await CrossSdk.GetAccountAsync();

                const string message = "Hello from Unity!";

                // It's also possible to sign a message as a byte array
                // var messageBytes = System.Text.Encoding.UTF8.GetBytes(message);
                // var signature = await CrossSdk.Evm.SignMessageAsync(messageBytes);
                var customData = new CustomData
                {
                    Metadata = "You are about to sign a message. This is plain text type custom data."
                };

                var signature = await CrossSdk.Evm.SignMessageAsync(message, "0x", customData);
                var isValid = await CrossSdk.Evm.VerifyMessageSignatureAsync(account.Address, message, signature);

                Notification.ShowMessage($"Signature finished: {signature} valid? {isValid}");
            }
            catch (RpcResponseException e)
            {
                Notification.ShowMessage($"{nameof(RpcResponseException)}:\n{e.Message}");
                Debug.LogException(e, this);
            }
        }

        public async void OnDisconnectButton()
        {
            Debug.Log("[CrossSdk Sample] OnDisconnectButton");

            try
            {
                Notification.ShowMessage($"Disconnecting...");
                await CrossSdk.DisconnectAsync();
                Notification.Hide();
            }
            catch (Exception e)
            {
                Notification.ShowMessage($"{e.GetType()}:\n{e.Message}");
                Debug.LogException(e, this);
            }
        }

        // send 1 cross coin to specified address
        public async void OnSendNativeButton()
        {
            Debug.Log("[CrossSdk Sample] OnSendNativeButton");

            const string toAddress = "0x920A31f0E48739C3FbB790D992b0690f7F5C42ea";

            try
            {
                Notification.ShowMessage("Sending transaction...");

                var customData = new CustomData
                {
                    Metadata = new
                    {
                        Title = "Custom Data",
                        Description = "Your are about to send 1 cross to the address"
                    }
                };
                var value = Web3.Convert.ToWei(1);  // send 1 cross
                int txType = 0;   // 0 is legacy transaction, 2 is EIP-1559 transaction. If you want FeePayer, set 2.
                var result = await CrossSdk.Evm.SendTransactionAsync(
                    toAddress, // to: 받는 사람 주소
                    value,     // amount: 전송할 토큰 양
                    null,      // data: 전송할 데이터
                    txType,    // type: 0 is legacy transaction, 2 is EIP-1559 transaction. If you want FeePayer, set 2.
                    customData // custom data: 사용자 정의 데이터
                );
                Debug.Log("Transaction hash: " + result);

                Notification.ShowMessage($"Tx hash: {result} now polling tx...");

                try {
                    var tx = await CrossSdk.Evm.PollTransaction(result);
                    Notification.ShowMessage($"Successfully retrieved transaction {result}");
                }
                catch (Exception ex)
                {
                    Notification.ShowMessage($"Error: {ex.Message}");
                }
            }
            catch (Exception e)
            {
                Notification.ShowMessage($"Error sending transaction.\n{e.Message}");
                Debug.LogException(e, this);
            }
        }

        // send 1 ERC20 token to specified address
        public async void OnSendERC20Button()
        {
            Debug.Log("[CrossSdk Sample] OnSendERC20Button");
            const string toAddress = "0x920A31f0E48739C3FbB790D992b0690f7F5C42ea";
            const string ERC20_ADDRESS = "0x88f8146EB4120dA51Fc978a22933CbeB71D8Bde6";
            TextAsset abiText = Resources.Load<TextAsset>("Contracts/SampleERC20abi");
            string abi = abiText.text;
            var customData = new CustomData
            {
                Metadata = "Meta data is required in Unity Sdk."
            };
            try
            {
                Notification.ShowMessage("Sending transaction...");

                var amount = Web3.Convert.ToWei(1);  // 1 토큰을 wei로 변환

                // Call any contract method with arbitrary parameters
                // Using WriteContractAsync overload without gas, value, and type parameters:
                // - arguments: toAddress and amount for the transfer function
                var result = await CrossSdk.Evm.WriteContractAsync(
                    ERC20_ADDRESS,  // contract address
                    abi,            // abi
                    "transfer",     // method name in contract code
                    customData,
                    toAddress,      // to: 받는 사람 주소
                    amount          // amount: 전송할 토큰 양
                );

                Debug.Log("Transaction hash: " + result);

                Notification.ShowMessage($"Tx hash: {result} now polling tx...");

                try {
                    // Poll transaction with received tx hash to see if it is mined on blockchain
                    var tx = await CrossSdk.Evm.PollTransaction(result);
                    Notification.ShowMessage($"Successfully retrieved transaction {result}");
                }
                catch (Exception ex)
                {
                    Notification.ShowMessage($"Error: {ex.Message}");
                }
                
            }
            catch (Exception e)
            {
                Notification.ShowMessage($"Error sending transaction.\n{e.Message}");
                Debug.LogException(e, this);
            }
        }

        // send 1 ERC20 token to specified address with FeePayer
        public async void OnSendERC20ButtonWithFeePayer()
        {
            Debug.Log("[CrossSdk Sample] OnSendERC20Button");
            const string toAddress = "0x920A31f0E48739C3FbB790D992b0690f7F5C42ea";
            const string ERC20_ADDRESS = "0x88f8146EB4120dA51Fc978a22933CbeB71D8Bde6";
            TextAsset abiText = Resources.Load<TextAsset>("Contracts/SampleERC20abi");
            string abi = abiText.text;
            var customData = new CustomData
            {
                Metadata = "Meta data is required in Unity Sdk."
            };
            try
            {
                Notification.ShowMessage("Sending transaction...");

                var amount = Web3.Convert.ToWei(1);  // 1 토큰을 wei로 변환
                var txType = 2; // 2 is EIP-1559 transaction. If you want FeePayer, set 2.

                // Call any contract method with arbitrary parameters
                // Using WriteContractAsync overload with value, gas, and type parameters:
                // - arguments: toAddress and amount for the transfer function
                var result = await CrossSdk.Evm.WriteContractAsync(
                    ERC20_ADDRESS,  // contract address
                    abi,            // abi
                    "transfer",     // method name in contract code
                    customData,
                    0,              // value는 0 (ETH를 보내지 않음)
                    default,        // gas는 기본값 사용
                    txType,         // type: 0 is legacy transaction, 2 is EIP-1559 transaction. If you want FeePayer, set 2.
                    toAddress,      // to: 받는 사람 주소
                    amount     // amount: 전송할 토큰 양
                );

                Debug.Log("Transaction hash: " + result);

                Notification.ShowMessage($"Tx hash: {result} now polling tx...");

                try {
                    // Poll transaction with received tx hash to see if it is mined on blockchain
                    var tx = await CrossSdk.Evm.PollTransaction(result);
                    Notification.ShowMessage($"Successfully retrieved transaction {result}");
                }
                catch (Exception ex)
                {
                    Notification.ShowMessage($"Error: {ex.Message}");
                }
                
            }
            catch (Exception e)
            {
                Notification.ShowMessage($"Error sending transaction.\n{e.Message}");
                Debug.LogException(e, this);
            }
        }

        // planned to beused for permit, but not supported in wallet yet
        public async void OnSignTypedDataV4Button()
        {
            Debug.Log("[CrossSdk Sample] OnSignTypedDataV4Button");

            Notification.ShowMessage("Signing typed data...");

            var account = await CrossSdk.GetAccountAsync();

            Debug.Log("Get mail typed definition");
            var typedData = GetMailTypedDefinition();
            var mail = new Mail
            {
                From = new Person
                {
                    Name = "Cow",
                    Wallets = new List<string>
                    {
                        "0xCD2a3d9F938E13CD947Ec05AbC7FE734Df8DD826",
                        "0xDeaDbeefdEAdbeefdEadbEEFdeadbeEFdEaDbeeF"
                    }
                },
                To = new List<Person>
                {
                    new()
                    {
                        Name = "Bob",
                        Wallets = new List<string>
                        {
                            "0xbBbBBBBbbBBBbbbBbbBbbbbBBbBbbbbBbBbbBBbB",
                            "0xB0BdaBea57B0BDABeA57b0bdABEA57b0BDabEa57",
                            "0xB0B0b0b0b0b0B000000000000000000000000000"
                        }
                    }
                },
                Contents = "Hello, Bob!"
            };

            // Convert CAIP-2 chain reference to EIP-155 chain ID
            // This is equivalent to `account.ChainId.Split(":")[1]`, but allocates less memory
            var ethChainId = Utils.ExtractChainReference(account.ChainId);

            typedData.Domain.ChainId = BigInteger.Parse(ethChainId);
            typedData.SetMessage(mail);

            var jsonMessage = typedData.ToJson();

            try
            {
                var signature = await CrossSdk.Evm.SignTypedDataAsync(jsonMessage);

                var isValid = await CrossSdk.Evm.VerifyTypedDataSignatureAsync(account.Address, jsonMessage, signature);

                Notification.ShowMessage($"Signature valid: {isValid}");
            }
            catch (Exception e)
            {
                Notification.ShowMessage("Error signing typed data");
                Debug.LogException(e, this);
            }
        }

        // read contract state such as balance of token or other data
        public async void OnReadContractClicked()
        {
            if (CrossSdk.NetworkController.ActiveChain.ChainId != "eip155:612044")
            {
                Notification.ShowMessage("Please switch to Cross Testnet.");
                return;
            }

            const string contractAddress = "0x88f8146EB4120dA51Fc978a22933CbeB71D8Bde6"; // on Cross Testnet
            const string testAccountAddress = "0xC1B55cfc80D0e9fB9ce7e31ecEbA4782Fcc4455D";
            TextAsset abiText = Resources.Load<TextAsset>("Contracts/SampleERC20abi");
            string abi = abiText.text;

            Notification.ShowMessage("Reading smart contract state...");

            try
            {
                var tokenName = await CrossSdk.Evm.ReadContractAsync<string>(contractAddress, abi, "name");
                Debug.Log($"Token name: {tokenName}");

                var balance = await CrossSdk.Evm.ReadContractAsync<BigInteger>(contractAddress, abi, "balanceOf", new object[]
                {
                    testAccountAddress
                });
                var result = $"Test Account owns: {Web3.Convert.FromWei(balance)} {tokenName} tokens active chain.";

                Notification.ShowMessage(result);
            }
            catch (Exception e)
            {
                Notification.ShowMessage($"Contract reading error.\n{e.Message}");
                Debug.LogException(e, this);
            }
        }

        private TypedData<Domain> GetMailTypedDefinition()
        {
            return new TypedData<Domain>
            {
                Domain = new Domain
                {
                    Name = "Ether Mail",
                    Version = "1",
                    ChainId = 1,
                    VerifyingContract = "0xCcCCccccCCCCcCCCCCCcCcCccCcCCCcCcccccccC"
                },
                Types = MemberDescriptionFactory.GetTypesMemberDescription(typeof(Domain), typeof(Group), typeof(Mail), typeof(Person)),
                PrimaryType = nameof(Mail)
            };
        }

        public const string CryptoPunksAbi =
            @"[{""constant"":true,""inputs"":[{""name"":""_owner"",""type"":""address""}],""name"":""balanceOf"",""outputs"":[{""name"":""balance"",""type"":""uint256""}],""payable"":false,""stateMutability"":""view"",""type"":""function""},
        {""constant"":true,""inputs"":[],""name"":""name"",""outputs"":[{""name"":"""",""type"":""string""}],""payable"":false,""stateMutability"":""view"",""type"":""function""}]";
    }

    internal struct ButtonStruct
    {
        public string Text;
        public Action OnClick;
        public bool? AccountRequired;
        public HashSet<string> ChainIds;
    }
}