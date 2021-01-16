using System;
using System.Collections.Generic;
using DoplTechnologies.Protos;
using DoplTechnologies.Sdk;

namespace DoplTechnologies.Sdk.Examples.Sender
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
                30000,
                "",
                new DataType[]
                {
                    DataType.CatheterSensorCoordinates,
                    DataType.ElectricalSignals,
                    DataType.RobotControls
                },
                new DataType[0],
                0
            );

            TeleroboticSdk.OnGetCatheterDataEvent += GetCatheterCoordinates;
            TeleroboticSdk.OnGetElectricalSignalDataEvent += GetElectricalSignalData;
            TeleroboticSdk.OnGetRobotControllerDataEvent += GetRobotControllerData;
            
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
            Console.WriteLine("Sending Catheter Data");
            List<CatheterData> data = new List<CatheterData>();
            for(int i = 0; i < 3; i++)
            {
                data.Add(new CatheterData
                {
                    SensorId = (UInt32)i,
                    Coordinates = new CatheterCoordinates
                    {
                        Position = new Coordinates
                        {
                            X = (1 + i) / 1000,
                            Y = (2 + i) / 1000,
                            Z = (3 + i) / 1000
                        },
                        Rotation = new Quaternion
                        {
                            W = 1,
                            X = (1 + i) / 1000,
                            Y = (2 + i) / 1000,
                            Z = (3 + i) / 1000
                        }
                    },
                });
            }

            return data.ToArray();
        }

        private static ElectricalSignalData[] GetElectricalSignalData()
        {
            Console.WriteLine("Sending Electrical Signal Data");
            List<ElectricalSignalData> data = new List<ElectricalSignalData>();
            for(int i = 0; i < 5; i++)
            {
                data.Add(new ElectricalSignalData
                {
                    SignalId = (uint)i,
                    Value = i,
                });
            }

            return data.ToArray();
        }

        private static RobotControllerData GetRobotControllerData()
        {
            Console.WriteLine("Sending Robot Controller Data");
            Random rnd = new Random();
            return new RobotControllerData()
            {
                MovementVelocity = rnd.Next(-1000, 1000) / 1000.0f,
                RotationVelocity = rnd.Next(-1000, 1000) / 1000.0f,
                DeflectionVelocity = rnd.Next(-1000, 1000) / 1000.0f,
            };
        }
    }
}
