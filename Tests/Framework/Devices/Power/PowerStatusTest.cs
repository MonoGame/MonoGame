using MonoGame.Framework.Devices.Power;
using NUnit.Framework;

namespace MonoGame.Tests.Devices.Power
{
	[TestFixture]
	public class PowerStatusTest
	{
		[Test]
		public void BatteryChargeStatusEnum()
		{
			Assert.AreEqual(0, (int)BatteryChargeStatus.Unknown);
			Assert.AreEqual(1, (int)BatteryChargeStatus.Full);
			Assert.AreEqual(2, (int)BatteryChargeStatus.Charging);
			Assert.AreEqual(3, (int)BatteryChargeStatus.OnBattery);
			Assert.AreEqual(4, (int)BatteryChargeStatus.NoBattery);
		}

		[Test]
		public void PowerLineStatusEnum()
		{
			Assert.AreEqual(0, (int)PowerLineStatus.Unknown);
			Assert.AreEqual(1, (int)PowerLineStatus.Online);
			Assert.AreEqual(2, (int)PowerLineStatus.Offline);
		}

		[Test]
		public void PowerStatusPropertiesAreInValidRanges()
		{
			var powerStatus = new PowerStatus();

			Assert.IsTrue(
				System.Enum.IsDefined(typeof(BatteryChargeStatus), powerStatus.BatteryChargeStatus),
				"BatteryChargeStatus returned an undefined enum value");

			Assert.IsTrue(
				System.Enum.IsDefined(typeof(PowerLineStatus), powerStatus.PowerLineStatus),
				"PowerLineStatus returned an undefined enum value");

			Assert.IsTrue(
				powerStatus.BatteryLifePercent == -1 ||
				(powerStatus.BatteryLifePercent >= 0 && powerStatus.BatteryLifePercent <= 100),
				"BatteryLifePercent should be -1 or between 0 and 100");
		}

		[Test]
		public void PowerStatusPropertiesRemainValidAcrossMultipleReads()
		{
			var powerStatus = new PowerStatus();

			for (var i = 0; i < 10; i++)
			{
				var batteryChargeStatus = powerStatus.BatteryChargeStatus;
				var powerLineStatus = powerStatus.PowerLineStatus;
				var batteryLifePercent = powerStatus.BatteryLifePercent;

				Assert.IsTrue(System.Enum.IsDefined(typeof(BatteryChargeStatus), batteryChargeStatus));
				Assert.IsTrue(System.Enum.IsDefined(typeof(PowerLineStatus), powerLineStatus));
				Assert.IsTrue(batteryLifePercent == -1 || (batteryLifePercent >= 0 && batteryLifePercent <= 100));
			}
		}
	}
}