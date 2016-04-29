using NUnit.Framework;
using System;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

namespace MonoGame.Tests
{
    [TestFixture]
    public class GamePadTests
    {
        private Array GetPlayerIndexes() // gets a list of enum values to test
        {
            return Enum.GetValues(typeof(PlayerIndex));
        }

        private Array GetGamePadDeadZoneIndexes()
        {
            return Enum.GetValues(typeof(GamePadDeadZone));
        }

        [Test] // generate tests for each PlayerIndex enum value
        public void GetCapabilities_ShouldNotReturnNull_GivenPlayerIndex([ValueSource("GetPlayerIndexes")] PlayerIndex pi)
        {
            Assert.IsNotNull(GamePad.GetCapabilities(pi));
        }

        [Test] // generate tests for each PlayerIndex enum value
        public void GetCapabilities_ShouldReturnGamePadCapabilities_GivenPlayerIndex([ValueSource("GetPlayerIndexes")] PlayerIndex pi)
        {
            Assert.IsInstanceOf<GamePadCapabilities>(GamePad.GetCapabilities(pi));
        }

        [Test]
        public void GetCapabilies_ShouldNotReturnNull_GivenInt([Range(-1, 4)] int i) // range to test negative, PlayerIndex, and higher values
        {
            Assert.IsInstanceOf<GamePadCapabilities>(GamePad.GetCapabilities(i));
        }

        [Test]
        public void GetCapabilities_ShouldReturnGamePadCapabilities_WhenGivenInt([Range(-1, 4)] int i) // range to test negative, PlayerIndex, and higher values
        {
            Assert.IsInstanceOf<GamePadCapabilities>(GamePad.GetCapabilities(i));
        }

        [Test] // generate tests for each PlayerIndex enum value
        public void GetState_ShouldNotReturnNull_WhenGivenPlayerIndex([ValueSource("GetPlayerIndexes")] PlayerIndex pi)
        {
            Assert.IsNotNull(GamePad.GetState(pi));
        }

        [Test] // generate tests for each PlayerIndex enum value
        public void GetState_ShouldReturnGamePadState_WhenGivenPlayerIndex([ValueSource("GetPlayerIndexes")] PlayerIndex pi)
        {
            Assert.IsInstanceOf<GamePadState>(GamePad.GetState(pi));
        }

        [Test]
        public void GetState_ShouldNotReturnNull_WhenGivenInt([Range(-1, 4)] int i) // range to test negative, PlayerIndex, and higher values
        {
            Assert.IsNotNull(GamePad.GetState(i));
        }
        [Test]
        public void GetState_ShouldReturnGamePadState_WheGivenInt([Range(-1, 4)] int i) // range to test negative, PlayerIndex, and higher values
        {
            Assert.IsInstanceOf<GamePadState>(GamePad.GetState(i));
        }

        [Test]
        public void GetState_ShouldNotReturnNull_WhenGivenPlayerIndex_GamePadDeadZone(
            [ValueSource("GetPlayerIndexes")] PlayerIndex pi, // generate tests for each combination of PlayerIndex and GamePadDeadZone enum values
            [ValueSource("GetGamePadDeadZoneIndexes")] GamePadDeadZone gpdz
            )
        {
            Assert.IsNotNull(GamePad.GetState(pi, gpdz));
        }

        [Test]
        public void GetState_ShouldReturnGamePadState_WhenGivenPlayerIndex_GamePadDeadZone(
            [ValueSource("GetPlayerIndexes")] PlayerIndex pi, // generate tests for each combination of PlayerIndex and GamePadDeadZone enum values
            [ValueSource("GetGamePadDeadZoneIndexes")] GamePadDeadZone gpdz
            )
        {
            Assert.IsInstanceOf<GamePadState>(GamePad.GetState(pi, gpdz));
        }

        [Test]
        public void GetState_ShouldNotReturnNull_WhenGivenInt_GamePadDeadZone(
            [Range(-1, 4)] int i, // generate tests for each combination of the index range and GamePadDeadZone enum values
            [ValueSource("GetGamePadDeadZoneIndexes")] GamePadDeadZone gpdz
            )
        {
            Assert.IsNotNull(GamePad.GetState(i, gpdz));
        }

        [Test]
        public void GetState_ShouldReturnGamePadState_WhenGivenInt_GamePadeDeadZone(
            [Range(-1, 4)] int i, // generate tests for each combination of the index range and GamePadDeadZone enum values
            [ValueSource("GetGamePadDeadZoneIndexes")] GamePadDeadZone gpdz
            )
        {
            Assert.IsInstanceOf<GamePadState>(GamePad.GetState(i, gpdz));
        }

        [Test]
        public void SetVibration_ShouldNotReturnNull_WhenGivenPlayerIndex_Float_Float(
            [ValueSource("GetPlayerIndexes")] PlayerIndex pi, // generate tests for each combination of the PlayerIndex enum values and both float ranges
            [Range(0.0f, 1.0f, 0.25f)] float lm,
            [Range(0.0f, 1.0f, 0.25f)] float rm
            )
        {
            Assert.IsNotNull(GamePad.SetVibration(pi, lm, rm));
        }

        [Test]
        public void SetVibration_ShouldReturnBool_WhenGivenPlayerIndex_Float_Float(
            [ValueSource("GetPlayerIndexes")] PlayerIndex pi, // generate tests for each combination of the PlayerIndex enum values and both float ranges
            [Range(0.0f, 1.0f, 1.0f)] float lm,
            [Range(0.0f, 1.0f, 1.0f)] float rm
            )
        {
            Assert.IsInstanceOf<bool>(GamePad.SetVibration(pi, lm, rm));
        }

        [Test]
        public void MaximumGamePadCount_ShouldNotReturnNull_WhenGet()
        {
            Assert.IsNotNull(GamePad.MaximumGamePadCount);
        }

        [Test]
        public void MaximumGamePadCount_ShouldReturnInt_WhenGet()
        {
            Assert.IsInstanceOf<int>(GamePad.MaximumGamePadCount);
        }
    }
}