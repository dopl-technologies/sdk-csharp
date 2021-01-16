using System;
using System.Collections.Generic;
using DoplTechnologies.Protos;
using DoplTechnologies.Sdk;

namespace DoplTechnologies.Sdk.Examples.Receiver
{
    class Program
    {
        private static bool _running = false;

        static void Main(string[] args)
        {
            Run(args);
        }

        static async void Run(string[] args)
        {
            Console.WriteLine("Running");

            _running = true;
            Console.CancelKeyPress += new ConsoleCancelEventHandler(ConsoleCancelKeyPress);

            const int deviceId = 9;
            TeleroboticSdk.Initialize(
                "session-service.beta.dopltechnologies.com:3000",
                "device-service.beta.dopltechnologies.com:3000",
                "state-manager-service.beta.dopltechnologies.com:3005",
                deviceId,
                30001,
                "",
                new DataType[0],
                new DataType[]
                {
                    DataType.CatheterSensorCoordinates,
                    DataType.ElectricalSignals,
                    DataType.RobotControls
                },
                0
            );

            TeleroboticSdk.OnCatheterDataEvent += OnCatheterDataReceived;
            TeleroboticSdk.OnElectricalSignalDataEvent += OnElectricalSignalData;
            TeleroboticSdk.OnRobotControllerDataEvent += OnRobotControllerData;
            
            var connectTask = TeleroboticSdk.Connect(deviceId);
            while (_running) { }
            TeleroboticSdk.Disconnect(deviceId);

            await connectTask;
        }

        static void ConsoleCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            _running = false;
        }

        private static void OnCatheterDataReceived(CatheterData[] data)
        {
            Console.WriteLine("##### Catheter Data Start #####");
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(data, Newtonsoft.Json.Formatting.Indented);
            Console.WriteLine(json);
            Console.WriteLine("##### Catheter Data End #####");
        }

        private static void OnElectricalSignalData(ElectricalSignalData[] data)
        {
            Console.WriteLine("##### Electrical Signal Data Start #####");
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(data, Newtonsoft.Json.Formatting.Indented);
            Console.WriteLine(json);
            Console.WriteLine("##### Electrical Signal Data End #####");
        }

        private static void OnRobotControllerData(RobotControllerData data)
        {
            Console.WriteLine("##### Robot Controller Data Start #####");
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(data, Newtonsoft.Json.Formatting.Indented);
            Console.WriteLine(json);
            Console.WriteLine("##### Robot Controller Data End #####");
        }
    }
}
