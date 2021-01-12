using DoplTechnologies.Sdk;
using NUnit.Framework;

namespace DoplTechnologies.Sdk.Tests
{
    public class SdkTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Sdk_CallTest_VerifyResult()
        {
            Assert.AreEqual(-1, TeleroboticSdk.Test());
        }
    }
}