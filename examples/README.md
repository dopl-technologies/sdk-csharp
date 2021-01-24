# Examples

This folder contains two dotnet applications that demonstrate how to send and receive data using the C# sdk.

![sender-receiver](https://user-images.githubusercontent.com/2764891/105623223-0516ef00-5dcd-11eb-9327-da85d2aa54a4.png)

## Running the examples

1. Open two terminals and run the following:

```shell
# Terminal 1
$ cd sender
$ dotnet run
Running
Initializing platform sdk
Initializing sdk
...
Waiting for session for device 11
Waiting for session to be connected
```

```shell
# Terminal 2
$ cd receiver
$ dotnet run
Running
Initializing platform sdk
Initializing sdk
...
Waiting for session for device 12
Waiting for session to be connected
```

2. Navigate to http://session-manager.beta.dopltechnologies.com/ and create a session
![image](https://user-images.githubusercontent.com/2764891/105623544-6c35a300-5dcf-11eb-91f1-243b24beac7a.png)

3. The sender should begin sending data, and the receiver should receiver it

## .dopltech.yml

Each example application contains a `.dopltech.yml` file that is used to connect the application to the server with the appropriate settings. Here is a detailed description of the configuration values:

| Property | Required | Default | Description |
| -------- | -------- | ------- | ----------- |
| deviceServiceAddress | yes | null | Url to the device service address |
| sessionServiceAddress | yes | null | Url to the session service address |
| stateManagerServiceAddress | yes | null | Url to the state manager service address |
| deviceId | yes | null | The id of the device. Contact admin@dopltechnologies to get a unique id for your device|
| devicePort | yes | null | The UDP port that is used to send and receive data. This port needs to be accessible on your network |
| deviceIp | no | `""` | The IP address of your computer. If not specified, the public IP address of your device is retrieved automatically |
| produces | no | `[]` | An array of integers that indicate the type of data this application produces. See the `DataType` enum in [common.proto](https://github.com/dopl-technologies/api-protos/blob/main/dopl/api/common.proto) for a list of values. Receiving applications will only listen for data types listed here  |
| consumes | no | `[]` | An array of integers that indicate the type of data this application receives. See the `DataType` enum in [common.proto](https://github.com/dopl-technologies/api-protos/blob/main/dopl/api/common.proto) for a list of values. Your application will only receive data that is listed in this array |
| sessionId | no | `0` | The id of the default session to connect to. `0` will force the application to wait for a new session. Once a session has been created, its id can be used for this value. This can rapidly speed up development |
| getFrameInterval | no | `10` | The frequency in milliseconds to get each frame of data |


## Creating your own application

1. Copy one of the example projects
1. Update the `deviceId` property in `.dopltech.yml` with your device's id. If you do not have a device id, contact ryan@dopltechnologies.com
1. Update `Program.cs` to send and receive the data you're interested in