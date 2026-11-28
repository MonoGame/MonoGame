using MonoGame.Framework.Devices.Power;
using NUnit.Framework;

namespace MonoGame.Tests.Devices.Power
{
	[NonParallelizable]
	[TestFixture]
	public class PowerStatusTestDesktopGL
	{
		private Sdl.d_sdl_get_power_info _originalGetPowerInfo;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			_originalGetPowerInfo = Sdl.SDL_GetPowerInfo;
		}

		[TearDown]
		public void TearDown()
		{
			Sdl.SDL_GetPowerInfo = _originalGetPowerInfo;
		}

		private static void StubPowerInfo(Sdl.PowerState state, int percent)
		{
			Sdl.SDL_GetPowerInfo = (out int seconds, out int batteryPercent) =>
			{
				seconds = -1;
				batteryPercent = percent;
				return state;
			};
		}

		[Test]
		public void BatteryChargeStatus_MapsDesktopPowerState()
		{
			var cases = new[]
			{
				(new { State = Sdl.PowerState.Charged, Expected = BatteryChargeStatus.Full }),
				(new { State = Sdl.PowerState.Charging, Expected = BatteryChargeStatus.Charging }),
				(new { State = Sdl.PowerState.OnBattery, Expected = BatteryChargeStatus.OnBattery }),
				(new { State = Sdl.PowerState.NoBattery, Expected = BatteryChargeStatus.NoBattery }),
				(new { State = Sdl.PowerState.Unknown, Expected = BatteryChargeStatus.Unknown }),
				(new { State = (Sdl.PowerState)999, Expected = BatteryChargeStatus.Unknown })
			};

			foreach (var testCase in cases)
			{
				StubPowerInfo(testCase.State, 50);

				var powerStatus = new PowerStatus();

				Assert.AreEqual(testCase.Expected, powerStatus.BatteryChargeStatus);
			}
		}

		[Test]
		public void PowerLineStatus_MapsDesktopPowerState()
		{
			var cases = new[]
			{
				(new { State = Sdl.PowerState.OnBattery, Expected = PowerLineStatus.Offline }),
				(new { State = Sdl.PowerState.Charged, Expected = PowerLineStatus.Online }),
				(new { State = Sdl.PowerState.Charging, Expected = PowerLineStatus.Online }),
				(new { State = Sdl.PowerState.NoBattery, Expected = PowerLineStatus.Online }),
				(new { State = Sdl.PowerState.Unknown, Expected = PowerLineStatus.Online }),
				(new { State = (Sdl.PowerState)999, Expected = PowerLineStatus.Online })
			};

			foreach (var testCase in cases)
			{
				StubPowerInfo(testCase.State, 50);

				var powerStatus = new PowerStatus();

				Assert.AreEqual(testCase.Expected, powerStatus.PowerLineStatus);
			}
		}

		[TestCase(-1)]
		[TestCase(0)]
		[TestCase(37)]
		[TestCase(100)]
		public void BatteryLifePercent_UsesSdlPercentValue(int expectedPercent)
		{
			StubPowerInfo(Sdl.PowerState.Charging, expectedPercent);

			var powerStatus = new PowerStatus();

			Assert.AreEqual(expectedPercent, powerStatus.BatteryLifePercent);
		}
	}
}