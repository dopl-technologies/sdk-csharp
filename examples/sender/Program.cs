using System;
using System.Collections.Generic;
using Google.Protobuf.WellKnownTypes;
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

            var teleroboticSdk = new TeleroboticSdk();
            teleroboticSdk.OnGetCatheterDataEvent += GetCatheterCoordinates;
            teleroboticSdk.OnGetElectricalSignalDataEvent += GetElectricalSignalData;
            teleroboticSdk.OnGetRobotControllerDataEvent += GetRobotControllerData;
            
            var connectTask = teleroboticSdk.Connect();
            while (_running) { }
            teleroboticSdk.Disconnect();

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
                Random r = new Random();
                data.Add(new CatheterData
                {
                    SensorId = (UInt32)i,
                    Coordinates = new CatheterCoordinates
                    {
                        Position = new Coordinates
                        {
                            X = r.Next(-10, 10) / 1000f,
                            Y = r.Next(-00, 10) / 1000f,
                            Z = r.Next(-10, 10) / 1000f
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
                    Created = Timestamp.FromDateTime(DateTime.UtcNow),
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
