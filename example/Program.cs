using System;
using System.Collections.Generic;
using DoplTechnologies.Protos;
using DoplTechnologies.Sdk;

namespace Example
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

            const int deviceId = 8;
            TeleroboticSdk.Initialize(
                "session-service.beta.dopltechnologies.com:3000",
                "device-service.beta.dopltechnologies.com:3000",
                "state-manager-service.beta.dopltechnologies.com:3005",
                deviceId,
                3000,
                "",
                new[] { DataType.CatheterSensorCoordinates },
                new DataType[] { DataType.ElectricalSignals },
                0
            );

            TeleroboticSdk.OnGetCatheterDataEvent += GetCatheterCoordinates;
            TeleroboticSdk.OnElectricalSignalDataEvent += OnElectricalSignalsReceived;
            var connectTask = TeleroboticSdk.Connect(deviceId);

            while (_running) { }
            TeleroboticSdk.Disconnect(deviceId);

            await connectTask;
        }

        static void ConsoleCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            _running = false;
        }

        private static CatheterData[] GetCatheterCoordinates()
        {
            List<CatheterData> data = new List<CatheterData>();
            for(int i = 0; i < 10; i++)
            {
                data.Add(new CatheterData
                {
                    SensorId = (UInt32)i,
                    Coordinates = new CatheterCoordinates
                    {
                        Position = new Coordinates
                        {
                            X = 1,
                            Y = 2,
                            Z = 3
                        },
                        Rotation = new Quaternion
                        {
                            W = 1,
                            X = 2,
                            Y = 3,
                            Z = 4
                        }
                    },
                });
            }

            return data.ToArray();
        }

        private static void OnElectricalSignalsReceived(ElectricalSignalData[] signals)
        {
            foreach (var s in signals)
                Console.WriteLine(s);
        }
    }
}
