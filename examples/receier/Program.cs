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

            var teleroboticSdk = new TeleroboticSdk();
            teleroboticSdk.OnCatheterDataEvent += OnCatheterDataReceived;
            teleroboticSdk.OnElectricalSignalDataEvent += OnElectricalSignalData;
            teleroboticSdk.OnRobotControllerDataEvent += OnRobotControllerData;
            
            var connectTask = teleroboticSdk.Connect();
            while (_running) { }
            teleroboticSdk.Disconnect();

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
