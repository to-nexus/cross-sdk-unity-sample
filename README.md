# Cross Unity SDK

Cross Unity SDK enables seamless integration of blockchain wallet connectivity into Unity projects. It supports EVM-compatible chains and provides functionalities such as wallet connection, signature handling, token balance retrieval, and transaction execution.

## Features

- **Wallet Connection**: Connect to various wallets using WalletConnect protocol.
- **Signature Handling**: Support for personal sign and EIP-712 typed data signing.
- **Token Management**: Retrieve native and ERC20 token balances.
- **Transaction Execution**: Send native and ERC20 token transactions.
- **Session Management**: Handle session resumption and disconnection.
- **Chain Management**: Switch between supported blockchain networks.

## Installation

1. **Required Unity Version**

   Unity version should be equal or greater than 6000.0.23f1. If you encounter any version compatibility issues, please feel free to contact us.​

3. **Add Scoped Registry**

   Modify your Unity project's `manifest.json` to include the Cross SDK scoped registry:

   ```json
   {
     "scopedRegistries": [
       {
         "name": "Cross",
         "url": "https://package.cross-nexus.com/repository/cross-sdk-unity/",
         "scopes": [
           "com.nexus.cross"
         ]
       }
     ],
     "dependencies": {
       "com.nexus.cross.sdk.unity": "1.0.0",
       "com.nexus.cross.core": "1.0.0",
       "com.nexus.cross.core.common": "1.0.0",
       "com.nexus.cross.core.crypto": "1.0.0",
       "com.nexus.cross.core.network": "1.0.0",
       "com.nexus.cross.core.storage": "1.0.0",
       "com.nexus.cross.sdk.unity": "1.0.0",
       "com.nexus.cross.sign": "1.0.0",
       "com.nexus.cross.sign.nethereum": "1.0.0",
       "com.nexus.cross.sign.nethereum.unity": "1.0.0",
       "com.nexus.cross.sign.unity": "1.0.0",
       "com.nexus.cross.unity.dependencies": "1.0.0"
       // add more wanted dependencies
     }
   }

## Usage

1. **Initialize the SDK**

   ```csharp
   var config = new CrossSdkConfig
   {
       projectId = "your_project_id",
       metadata = new Metadata(
           name: "Your App Name",
           description: "App Description",
           url: "https://yourapp.com",
           iconUrl: "https://yourapp.com/icon.png"
       )
   };

   await CrossSdk.InitializeAsync(config);
   ```

2. **Connect to Wallet**

  ```csharp
  CrossSdk.Connect();
  ```

3. **Sign a Message**

  ```csharp
  var account = await CrossSdk.GetAccountAsync();
  var message = "Hello from Unity!";
  var customData = new CustomData{ Metadata = "You are about to sign a message. This is plain text type custom data." }
  var signature = await CrossSdk.Evm.SignMessageAsync(message, "0x", customData);   // use 0x for 2nd parameter in case of address is undefined
  ```

4. **Send Cross Coin**

  ```csharp
    const string toAddress = "0x920A31f0E48739C3FbB790D992b0690f7F5C42ea";  // receipient address
    const value = var value = Web3.Convert.ToWei(1);  // convert to wei to send 1 cross
    var result = await CrossSdk.Evm.SendTransactionAsync(toAddress, value, null, null); // data or custom data can be null
  ```

5. **Send ERC20 Token**

  ```csharp
    const string toAddress = "0x920A31f0E48739C3FbB790D992b0690f7F5C42ea";  // receipient address
    const string ERC20_ADDRESS = "0x88f8146EB4120dA51Fc978a22933CbeB71D8Bde6";  // ERC20 token contract address
    TextAsset abiText = Resources.Load<TextAsset>("Contracts/SampleERC20abi");  // JSON formatted abi for the token contract file
    string abi = abiText.text;

    var value = Web3.Convert.ToWei(1);

    // Call any contract method with arbitrary parameters
    // In this case, WriteContractAsync executes a contract method ('transfer') with custom data and parameters
    var result = await CrossSdk.Evm.WriteContractAsync(
        ERC20_ADDRESS,  // contract address
        abi,  //abi
        "transfer", // method name in contract code
        null,   // custom data can be null
        toAddress,
        value
    );
  ```

6. Custom Data

  When signing messages or sending transactions, you can utilize custom data to help users understand the purpose of the action. This custom data supports plain text, JSON, and HTML formats.​

7. License

  Apache 2.0



