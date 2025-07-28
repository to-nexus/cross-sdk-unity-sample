# Cross Unity SDK Sample

Cross Unity SDK enables seamless integration of blockchain wallet connectivity into Unity projects. It supports EVM-compatible chains and provides functionalities such as wallet connection, signature handling, token balance retrieval, and transaction execution.

## Features

- **Wallet Connection**: Connect to various wallets using WalletConnect protocol.
- **Signature Handling**: Support for personal sign and EIP-712 typed data signing.
- **Token Management**: Retrieve native and ERC20 token balances.
- **Transaction Execution**: Send native and ERC20 token transactions.
- **Session Management**: Handle session resumption and disconnection.
- **Chain Management**: Switch between supported blockchain networks.
- **Smart Contract Interactions**: Interact with smart contracts and ERC20 tokens.

## Installation

1. **Required Unity Version**

   Unity version must be 2022.3 or higher. We recommend using 2022.3.62f1 or later for best compatibility.
   If you encounter any version-related issues, please don't hesitate to contact us.

2. **Add Scoped Registry**

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
       "com.nexus.cross.sdk.unity": "1.3.1",
       "com.nexus.cross.core": "1.3.1",
       "com.nexus.cross.core.common": "1.3.1",
       "com.nexus.cross.core.crypto": "1.3.1",
       "com.nexus.cross.core.network": "1.3.1",
       "com.nexus.cross.core.storage": "1.3.1",
       "com.nexus.cross.sign": "1.3.1",
       "com.nexus.cross.sign.nethereum": "1.3.1",
       "com.nexus.cross.sign.nethereum.unity": "1.3.1",
       "com.nexus.cross.sign.unity": "1.3.1",
       "com.nexus.cross.unity.dependencies": "1.3.1"
       // add more wanted dependencies
     }
   }
   ```

## Important Notes

### Method Overloading and Object Casting

When calling contract methods with `WriteContractAsync`, you **must** cast parameters to `(object)` to avoid method overloading resolution issues:

```csharp
// ✅ Correct - Cast parameters to object
var result = await CrossSdk.Evm.WriteContractAsync(
    contractAddress,
    abi,
    "transfer",
    customData,
    (object)toAddress,      // Cast to object
    (object)amount          // Cast to object
);

// ❌ Incorrect - May cause overloading resolution issues
var result = await CrossSdk.Evm.WriteContractAsync(
    contractAddress,
    abi,
    "transfer",
    customData,
    toAddress,              // No casting
    amount                  // No casting
);
```

This casting is required because the `WriteContractAsync` method has multiple overloads, and without explicit casting, the compiler may choose the wrong overload.

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
    var value = Web3.Convert.ToWei(1);  // convert to wei to send 1 cross
    var result = await CrossSdk.Evm.SendTransactionAsync(toAddress, value, null, null); // data or custom data can be null
  ```

5. **Send ERC20 Token**

  ```csharp
    const string toAddress = "0x920A31f0E48739C3FbB790D992b0690f7F5C42ea";  // receipient address
    const string ERC20_ADDRESS = "0x88f8146EB4120dA51Fc978a22933CbeB71D8Bde6";  // ERC20 token contract address
    TextAsset abiText = Resources.Load<TextAsset>("Contracts/SampleERC20abi");  // JSON formatted abi for the token contract file
    string abi = abiText.text;

    var result = await CrossSdk.Evm.WriteContractAsync(
        ERC20_ADDRESS,  // contract address
        abi,            // abi
        "transfer",     // method name in contract code
        null,           // customData can be null
        (object)toAddress,      // to: recepient address
        (object)amount          // amount: token amount to send
    );
  ```

6. **Custom Data**

  When signing messages or sending transactions, you can utilize custom data to help users understand the purpose of the action. This custom data supports plain text, JSON, and HTML formats.

## Setup

1. Open the project in Unity
2. Configure your project settings
3. Run the sample scene

## Dependencies

- Cross SDK Unity packages (version 1.3.1)
- Nethereum Unity
- Newtonsoft JSON
- Unity UI Toolkit

## Sample Features

The sample includes various buttons demonstrating different blockchain operations:

- Connect/Disconnect wallet
- Switch networks
- Check balances
- Send transactions
- Interact with smart contracts
- Sign messages

## License

Cross is released under the Apache 2.0 license. [See LICENSE](/LICENSE) for details. 