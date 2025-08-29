using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using NUnit.Framework;
using System;
using System.Collections;
using System.Globalization; 

namespace MonoGame.Tests.Framework
{
    /// <summary>
    /// Performs a series of tests on MonoGame's value types ToString() methods,
    /// ensuring they always behave appropriately depending on the utilised <see cref="CultureInfo"/>.
    /// </summary>
    /// <remarks>
    /// All values to test on have been picked randomly.
    /// </remarks>
    internal class SerializationTest
    {
        #region Cultures to test on
        private static readonly CultureInfo AmericanCulture = new CultureInfo("en-US");
        private static readonly CultureInfo BritishCulture = new CultureInfo("en-GB");
        private static readonly CultureInfo ChineseCulture = new CultureInfo("zh-CN");
        private static readonly CultureInfo DutchCulture = new CultureInfo("nl-NL");
        private static readonly CultureInfo FrenchCulture = new CultureInfo("fr-FR");
        private static readonly CultureInfo GermanCulture = new CultureInfo("de-DE");
        private static readonly CultureInfo ItalianCulture = new CultureInfo("it-IT");
        private static readonly CultureInfo JapaneseCulture = new CultureInfo("ja-JP");
        #endregion

        /// <summary>
        /// We utilise <see cref="Convert.ChangeType(object, Type, IFormatProvider)"/> now with the invariant culture,
        /// to prevent rare issues with culture when converting from one type to another. 
        /// </summary>
        [Test]
        public void TestChangeType()
        {
            const string commaValue = "3,14";

            var commaResult = Convert.ChangeType(commaValue, typeof(double), GermanCulture);
            Assert.That(Math.Abs(3.14 - (double)commaResult) < double.Epsilon);

            // 3,14 in cultures like en-US or ja-JP is parsed as 314, as the comma is ignored.
            Assert.That(Math.Abs((double)Convert.ChangeType(commaValue, typeof(double), JapaneseCulture) - 3.14) > double.Epsilon);

            const string thousandValue = "1,234.56";

            var thousandResult = Convert.ChangeType(thousandValue, typeof(decimal), ChineseCulture);
            Assert.That(Math.Abs(1234.56d - (double)(decimal)thousandResult) < double.Epsilon);

            // this is not a valid format in Dutch culture, as it uses a comma for decimal separator.
            Assert.Throws<FormatException>(() => Convert.ChangeType(thousandValue, typeof(decimal), DutchCulture));

            const string dateValue = "31/12/2023";

            var dateResult = (DateTime)Convert.ChangeType(dateValue, typeof(DateTime), BritishCulture);
            Assert.That(new DateTime(2023, 12, 31), Is.EqualTo(dateResult));

            Assert.Throws<FormatException>(() => Convert.ChangeType(dateValue, typeof(DateTime), AmericanCulture));

            const string dateTimeValue = "05/31/2025 01:30 PM";

            // 2025-05-31 13:30:00
            var dateTimeResult = Convert.ChangeType(dateTimeValue, typeof(DateTime), AmericanCulture);
            Assert.That(new DateTime(2025, 5, 31, 13, 30, 0), Is.EqualTo(dateTimeResult));

            // FormatException (expects 24h clock, not "PM")
            Assert.Throws<FormatException>(() => Convert.ChangeType(dateTimeValue, typeof(DateTime), FrenchCulture));
        }

        /// <summary>
        /// Tests the <see cref="Alpha8.ToString()" /> and <see cref="Alpha8.ToString(IFormatProvider)"/> methods.
        /// </summary>
        [Test]
        public void TestAlpha8()
        {
            var item = new Alpha8(0.25f);
            // serialize with the culture explicitly set.
            var invariant = item.ToString(CultureInfo.InvariantCulture);
            var american = item.ToString(AmericanCulture);
            var british = item.ToString(BritishCulture);
            var chinese = item.ToString(ChineseCulture);
            var dutch = item.ToString(DutchCulture);
            var french = item.ToString(FrenchCulture);
            var german = item.ToString(GermanCulture);
            var italian = item.ToString(ItalianCulture);
            var japanese = item.ToString(JapaneseCulture);

            var originalCulture = CultureInfo.CurrentCulture;

            // serialize by setting the culture to utilise.
            CultureInfo.CurrentCulture = AmericanCulture;
            var americanCurrent = item.ToString();
            Assert.That(american, Is.EqualTo(americanCurrent).Using(StringComparer.InvariantCulture as IComparer));
            CultureInfo.CurrentCulture = BritishCulture;
            var britishCurrent = item.ToString();
            Assert.That(british, Is.EqualTo(britishCurrent).Using(StringComparer.InvariantCulture as IComparer));
            CultureInfo.CurrentCulture = ChineseCulture;
            var chineseCurrent = item.ToString();
            Assert.That(chinese, Is.EqualTo(chineseCurrent).Using(StringComparer.InvariantCulture as IComparer));
            CultureInfo.CurrentCulture = DutchCulture;
            var dutchCurrent = item.ToString();
            Assert.That(dutch, Is.EqualTo(dutchCurrent).Using(StringComparer.InvariantCulture as IComparer));
            CultureInfo.CurrentCulture = FrenchCulture;
            var frenchCurrent = item.ToString();
            Assert.That(french, Is.EqualTo(frenchCurrent).Using(StringComparer.InvariantCulture as IComparer));
            CultureInfo.CurrentCulture = GermanCulture;
            var germanCurrent = item.ToString();
            Assert.That(german, Is.EqualTo(germanCurrent).Using(StringComparer.InvariantCulture as IComparer));
            CultureInfo.CurrentCulture = ItalianCulture;
            var italianCurrent = item.ToString();
            Assert.That(italian, Is.EqualTo(italianCurrent).Using(StringComparer.InvariantCulture as IComparer));
            CultureInfo.CurrentCulture = JapaneseCulture;
            var japaneseCurrent = item.ToString();
            Assert.That(japanese, Is.EqualTo(japaneseCurrent).Using(StringComparer.InvariantCulture as IComparer));

            // Observe that the invariant culture is not equal to specific other cultures.
            Assert.That(invariant, Is.Not.EqualTo(dutch).Using(StringComparer.InvariantCulture as IComparer));
            Assert.That(invariant, Is.Not.EqualTo(french).Using(StringComparer.InvariantCulture as IComparer));
            Assert.That(invariant, Is.Not.EqualTo(german).Using(StringComparer.InvariantCulture as IComparer));
            Assert.That(invariant, Is.Not.EqualTo(italian).Using(StringComparer.InvariantCulture as IComparer));

            CultureInfo.CurrentCulture = originalCulture;
        }


        /// <summary>
        /// Tests the <see cref="HalfSingle.ToString()" /> and <see cref="HalfSingle.ToString(IFormatProvider)"/> methods.
        /// </summary>
        [Test]
        public void TestHalfSingle()
        {
            var item = new HalfSingle(12.55f);
            // serialize with the culture explicitly set.
            var invariant = item.ToString(CultureInfo.InvariantCulture);
            var american = item.ToString(AmericanCulture);
            var british = item.ToString(BritishCulture);
            var chinese = item.ToString(ChineseCulture);
            var dutch = item.ToString(DutchCulture);
            var french = item.ToString(FrenchCulture);
            var german = item.ToString(GermanCulture);
            var italian = item.ToString(ItalianCulture);
            var japanese = item.ToString(JapaneseCulture);

            var originalCulture = CultureInfo.CurrentCulture;

            // serialize by setting the culture to utilise.
            CultureInfo.CurrentCulture = AmericanCulture;
            var americanCurrent = item.ToString();
            Assert.That(american, Is.EqualTo(americanCurrent).Using(StringComparer.InvariantCulture as IComparer));
            CultureInfo.CurrentCulture = BritishCulture;
            var britishCurrent = item.ToString();
            Assert.That(british, Is.EqualTo(britishCurrent).Using(StringComparer.InvariantCulture as IComparer));
            CultureInfo.CurrentCulture = ChineseCulture;
            var chineseCurrent = item.ToString();
            Assert.That(chinese, Is.EqualTo(chineseCurrent).Using(StringComparer.InvariantCulture as IComparer));
            CultureInfo.CurrentCulture = DutchCulture;
            var dutchCurrent = item.ToString();
            Assert.That(dutch, Is.EqualTo(dutchCurrent).Using(StringComparer.InvariantCulture as IComparer));
            CultureInfo.CurrentCulture = FrenchCulture;
            var frenchCurrent = item.ToString();
            Assert.That(french, Is.EqualTo(frenchCurrent).Using(StringComparer.InvariantCulture as IComparer));
            CultureInfo.CurrentCulture = GermanCulture;
            var germanCurrent = item.ToString();
            Assert.That(german, Is.EqualTo(germanCurrent).Using(StringComparer.InvariantCulture as IComparer));
            CultureInfo.CurrentCulture = ItalianCulture;
            var italianCurrent = item.ToString();
            Assert.That(italian, Is.EqualTo(italianCurrent).Using(StringComparer.InvariantCulture as IComparer));
            CultureInfo.CurrentCulture = JapaneseCulture;
            var japaneseCurrent = item.ToString();
            Assert.That(japanese, Is.EqualTo(japaneseCurrent).Using(StringComparer.InvariantCulture as IComparer));

            // Observe that the invariant culture is not equal to specific other cultures.
            Assert.That(invariant, Is.Not.EqualTo(dutch).Using(StringComparer.InvariantCulture as IComparer));
            Assert.That(invariant, Is.Not.EqualTo(french).Using(StringComparer.InvariantCulture as IComparer));
            Assert.That(invariant, Is.Not.EqualTo(german).Using(StringComparer.InvariantCulture as IComparer));
            Assert.That(invariant, Is.Not.EqualTo(italian).Using(StringComparer.InvariantCulture as IComparer));

            CultureInfo.CurrentCulture = originalCulture;
        }



        /// <summary>
        /// Tests the <see cref="HalfVector2.ToString()" /> and <see cref="HalfVector2.ToString(IFormatProvider)"/> methods.
        /// </summary>
        [Test]
        public void TestHalfVector2()
        {
            var item = new HalfVector2(new Vector2(1.5f, 2.4f));
            // serialize with the culture explicitly set.
            var invariant = item.ToString(CultureInfo.InvariantCulture);
            var american = item.ToString(AmericanCulture);
            var british = item.ToString(BritishCulture);
            var chinese = item.ToString(ChineseCulture);
            var dutch = item.ToString(DutchCulture);
            var french = item.ToString(FrenchCulture);
            var german = item.ToString(GermanCulture);
            var italian = item.ToString(ItalianCulture);
            var japanese = item.ToString(JapaneseCulture);

            var originalCulture = CultureInfo.CurrentCulture;

            // serialize by setting the culture to utilise.
            CultureInfo.CurrentCulture = AmericanCulture;
            var americanCurrent = item.ToString();
            Assert.That(american, Is.EqualTo(americanCurrent).Using(StringComparer.InvariantCulture as IComparer));
            CultureInfo.CurrentCulture = BritishCulture;
            var britishCurrent = item.ToString();
            Assert.That(british, Is.EqualTo(britishCurrent).Using(StringComparer.InvariantCulture as IComparer));
            CultureInfo.CurrentCulture = ChineseCulture;
            var chineseCurrent = item.ToString();
            Assert.That(chinese, Is.EqualTo(chineseCurrent).Using(StringComparer.InvariantCulture as IComparer));
            CultureInfo.CurrentCulture = DutchCulture;
            var dutchCurrent = item.ToString();
            Assert.That(dutch, Is.EqualTo(dutchCurrent).Using(StringComparer.InvariantCulture as IComparer));
            CultureInfo.CurrentCulture = FrenchCulture;
            var frenchCurrent = item.ToString();
            Assert.That(french, Is.EqualTo(frenchCurrent).Using(StringComparer.InvariantCulture as IComparer));
            CultureInfo.CurrentCulture = GermanCulture;
            var germanCurrent = item.ToString();
            Assert.That(german, Is.EqualTo(germanCurrent).Using(StringComparer.InvariantCulture as IComparer));
            CultureInfo.CurrentCulture = ItalianCulture;
            var italianCurrent = item.ToString();
            Assert.That(italian, Is.EqualTo(italianCurrent).Using(StringComparer.InvariantCulture as IComparer));
            CultureInfo.CurrentCulture = JapaneseCulture;
            var japaneseCurrent = item.ToString();
            Assert.That(japanese, Is.EqualTo(japaneseCurrent).Using(StringComparer.InvariantCulture as IComparer));

            // Observe that the invariant culture is not equal to specific other cultures.
            Assert.That(invariant, Is.Not.EqualTo(dutch).Using(StringComparer.InvariantCulture as IComparer));
            Assert.That(invariant, Is.Not.EqualTo(french).Using(StringComparer.InvariantCulture as IComparer));
            Assert.That(invariant, Is.Not.EqualTo(german).Using(StringComparer.InvariantCulture as IComparer));
            Assert.That(invariant, Is.Not.EqualTo(italian).Using(StringComparer.InvariantCulture as IComparer));

            CultureInfo.CurrentCulture = originalCulture;
        }

        /// <summary>
        /// Tests the <see cref="HalfVector4.ToString()" /> and <see cref="HalfVector4.ToString(IFormatProvider)"/> methods.
        /// </summary>
        [Test]
        public void TestHalfVector4()
        {
            var item = new HalfVector4(1.5f, 2.4f, 3.5f, 0.5f);
            // serialize with the culture explicitly set.
            var invariant = item.ToString(CultureInfo.InvariantCulture);
            var american = item.ToString(AmericanCulture);
            var british = item.ToString(BritishCulture);
            var chinese = item.ToString(ChineseCulture);
            var dutch = item.ToString(DutchCulture);
            var french = item.ToString(FrenchCulture);
            var german = item.ToString(GermanCulture);
            var italian = item.ToString(ItalianCulture);
            var japanese = item.ToString(JapaneseCulture);

            var originalCulture = CultureInfo.CurrentCulture;

            // serialize by setting the culture to utilise.
            CultureInfo.CurrentCulture = AmericanCulture;
            var americanCurrent = item.ToString();
            Assert.That(american, Is.EqualTo(americanCurrent).Using(StringComparer.InvariantCulture as IComparer));
            CultureInfo.CurrentCulture = BritishCulture;
            var britishCurrent = item.ToString();
            Assert.That(british, Is.EqualTo(britishCurrent).Using(StringComparer.InvariantCulture as IComparer));
            CultureInfo.CurrentCulture = ChineseCulture;
            var chineseCurrent = item.ToString();
            Assert.That(chinese, Is.EqualTo(chineseCurrent).Using(StringComparer.InvariantCulture as IComparer));
            CultureInfo.CurrentCulture = DutchCulture;
            var dutchCurrent = item.ToString();
            Assert.That(dutch, Is.EqualTo(dutchCurrent).Using(StringComparer.InvariantCulture as IComparer));
            CultureInfo.CurrentCulture = FrenchCulture;
            var frenchCurrent = item.ToString();
            Assert.That(french, Is.EqualTo(frenchCurrent).Using(StringComparer.InvariantCulture as IComparer));
            CultureInfo.CurrentCulture = GermanCulture;
            var germanCurrent = item.ToString();
            Assert.That(german, Is.EqualTo(germanCurrent).Using(StringComparer.InvariantCulture as IComparer));
            CultureInfo.CurrentCulture = ItalianCulture;
            var italianCurrent = item.ToString();
            Assert.That(italian, Is.EqualTo(italianCurrent).Using(StringComparer.InvariantCulture as IComparer));
            CultureInfo.CurrentCulture = JapaneseCulture;
            var japaneseCurrent = item.ToString();
            Assert.That(japanese, Is.EqualTo(japaneseCurrent).Using(StringComparer.InvariantCulture as IComparer));

            // Observe that the invariant culture is not equal to specific other cultures.
            Assert.That(invariant, Is.Not.EqualTo(dutch).Using(StringComparer.InvariantCulture as IComparer));
            Assert.That(invariant, Is.Not.EqualTo(french).Using(StringComparer.InvariantCulture as IComparer));
            Assert.That(invariant, Is.Not.EqualTo(german).Using(StringComparer.InvariantCulture as IComparer));
            Assert.That(invariant, Is.Not.EqualTo(italian).Using(StringComparer.InvariantCulture as IComparer));

            CultureInfo.CurrentCulture = originalCulture;
        }
    }
}
