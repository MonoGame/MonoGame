// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace MonoGame.Tests 
{
	partial class GameTest 
    {
		public static class Properties 
        {
			[TestFixture]
			[Category("GameTest")]
			public class Components : ReadOnlyPropertyFixtureBase<GameComponentCollection> 
            {
				public Components ()
					: base (g => g.Components)
				{ }

				public override void Has_correct_default_value () { }

				[Test]
				public void Is_available_before_Run ()
				{
					Assert.That (Game, HasThisProperty.Not.Null);
				}
			}

			[TestFixture]
			[Category("GameTest")]
			public class Content : ReadWritePropertyFixtureBase<ContentManager> {
				public Content ()
					: base (g => g.Content)
				{
					AddLegalValue (new ContentManager (new GameServiceContainer ()));

					AddIllegalValue<ArgumentNullException> (null);
				}

				[SetUp]
				public override void SetUp ()
				{
					base.SetUp ();
				}

				public override void Has_correct_default_value () { }

				[Test]
				public void Is_available_before_Run ()
				{
					Assert.That (Game, HasThisProperty.Not.Null);
				}

				[Test]
				public void Is_not_provided_as_a_service ()
				{
					Assert.IsNull (Game.Services.GetService (typeof (ContentManager)));
				}
			}

			[TestFixture]
			[Category("GameTest")]
			public class GraphicsDevice_ : ReadOnlyPropertyFixtureBase<GraphicsDevice> {
				public GraphicsDevice_ ()
					: base (g => g.GraphicsDevice)
				{ }

				public override void Has_correct_default_value () { }

				[Test]
				public void Is_invalid_without_IGraphicsDeviceService ()
				{
					Assert.IsNull (Game.Services.GetService (typeof (IGraphicsDeviceService)));
					Assert.Throws<InvalidOperationException> (() => {
						var device = ThisProperty;
					});
				}

				[Test]
				public void Is_valid_with_IGraphicsDeviceService ()
				{
					var service = new MockGraphicsDeviceService ();
					Game.Services.AddService (typeof (IGraphicsDeviceService), service);

					Assert.That (Game, HasThisProperty.SameAs (service.GraphicsDevice));
				}

				[Test]
				public void Is_not_available_in_graphical_game_before_Run ()
				{
					Game.MakeGraphical ();
					Assert.That (Game, HasThisProperty.Null);
				}

				private class MockGraphicsDeviceService : IGraphicsDeviceService
                {
                    #pragma warning disable 67
					public event EventHandler<EventArgs> DeviceCreated;
					public event EventHandler<EventArgs> DeviceDisposing;
					public event EventHandler<EventArgs> DeviceReset;
                    public event EventHandler<EventArgs> DeviceResetting;
                    #pragma warning restore 67

					// TODO: It might be nice to try to use a real, live
					//       GraphicsDevice here rather than null.
					public GraphicsDevice GraphicsDevice { get { return null; } }
				}
			}

			[TestFixture]
			[Category("GameTest")]
			public class InactiveSleepTime : ReadWritePropertyFixtureBase<TimeSpan> {
				public InactiveSleepTime ()
					: base (g => g.InactiveSleepTime)
				{
					DefaultValue = TimeSpan.FromSeconds (0.02);

					AddLegalValue (TimeSpan.Zero);
					AddLegalValue (TimeSpan.Zero);
					AddLegalValue (TimeSpan.FromSeconds (1));
					AddLegalValue (TimeSpan.MaxValue);

					AddIllegalValue<ArgumentOutOfRangeException> (TimeSpan.FromSeconds (-1));
					AddIllegalValue<ArgumentOutOfRangeException> (TimeSpan.MinValue);
				}
			}

			[TestFixture]
			[Category("GameTest")]
			public class IsActive : ReadOnlyPropertyFixtureBase<bool> {
				public IsActive ()
					: base (g => g.IsActive)
				{
					DefaultValue = false;
				}
			}

			[TestFixture]
			[Category("GameTest")]
			public class IsFixedTimeStep : ReadWritePropertyFixtureBase<bool> {
				public IsFixedTimeStep ()
					: base (g => g.IsFixedTimeStep)
				{
					DefaultValue = true;

					AddLegalValue (true);
					AddLegalValue (false);
				}

				public override void Cannot_set_illegal_value (Tuple<bool, Type> valueAndException) { }
			}

			[TestFixture]
			[Category("GameTest")]
			public class IsMouseVisible : ReadWritePropertyFixtureBase<bool> {
				public IsMouseVisible ()
					: base (g => g.IsMouseVisible)
				{
					DefaultValue = false;

					AddLegalValue (true);
					AddLegalValue (false);
				}

				public override void Cannot_set_illegal_value (Tuple<bool, Type> valueAndException) { }
			}

			[TestFixture]
			[Category("GameTest")]
			public class LaunchParameters_ : ReadOnlyPropertyFixtureBase<LaunchParameters> {
				public LaunchParameters_ ()
					: base (g => g.LaunchParameters)
				{ }

				public override void Has_correct_default_value () { }

				[Test]
				public void Is_available_before_Run ()
				{
					Assert.That (Game, HasThisProperty.Not.Null);
				}
			}

			[TestFixture]
			[Category("GameTest")]
			public class Services : ReadOnlyPropertyFixtureBase<GameServiceContainer> {
				public Services ()
					: base (g => g.Services)
				{ }

				public override void Has_correct_default_value () { }

				[Test]
				public void Is_available_before_Run ()
				{
					Assert.That (Game, HasThisProperty.Not.Null);
				}
			}

			[TestFixture]
			[Category("GameTest")]
			public class TargetElapsedTime : ReadWritePropertyFixtureBase<TimeSpan> {
				public TargetElapsedTime ()
					: base (g => g.TargetElapsedTime)
				{
					DefaultValue = TimeSpan.FromTicks (166667);

					AddLegalValue (TimeSpan.FromSeconds (1));
					AddLegalValue (TimeSpan.MaxValue);

					AddIllegalValue<ArgumentOutOfRangeException> (TimeSpan.Zero);
					AddIllegalValue<ArgumentOutOfRangeException> (TimeSpan.FromSeconds (-1));
					AddIllegalValue<ArgumentOutOfRangeException> (TimeSpan.MinValue);
				}
			}

			[TestFixture]
			[Category("GameTest")]
			public class Window : ReadOnlyPropertyFixtureBase<GameWindow> {
				public Window ()
					: base (g => g.Window)
				{ }

				public override void Has_correct_default_value () { }

				[Test]
				public void Is_available_before_Run ()
				{
					Game.MakeGraphical ();
					Assert.That (Game, HasThisProperty.Not.Null);
				}

				[Test]
				public void Is_available_in_non_graphical_game ()
				{
					Assert.That (Game, HasThisProperty.Not.Null);
				}
			}

			public abstract class PropertyFixtureBase<PropertyT> : FixtureBase {
				private Func<Game, PropertyT> _getter;
				protected PropertyFixtureBase (Expression<Func<Game, PropertyT> > propertyExpression)
				{
					MemberExpression member = (MemberExpression) propertyExpression.Body;

					_propertyInfo = (PropertyInfo) member.Member;
					_getter = propertyExpression.Compile ();
				}

				protected PropertyT _GetValue ()
				{
					return _getter (Game);
				}

				protected void _SetValue (PropertyT value)
				{
				}

				protected ResolvableConstraintExpression HasThisProperty {
					get { return Has.Property (_propertyInfo.Name); }
				}

				private PropertyInfo _propertyInfo;

				protected PropertyInfo PropertyInfo {
					get { return _propertyInfo; }
				}

				protected virtual PropertyT ThisProperty {
					get { return _getter (Game); }
				}

				private Maybe<PropertyT> _defaultValue;

				protected PropertyT DefaultValue {
					get {
						if (!_defaultValue.HasValue)
							throw new InvalidOperationException (
								"DefaultValue has never been set.");
						return _defaultValue.Value;
					}
					set {
						_defaultValue = value;
					}
				}

				[Test]
				public virtual void Has_correct_default_value ()
				{
					if (!_defaultValue.HasValue)
						throw new IgnoreException ("No DefaultValue available");
					Assert.That (Game, HasThisProperty.EqualTo (_defaultValue.Value));
				}

				private struct Maybe<T> {
					public Maybe (T value)
					{
						_hasValue = true;
						_value = value;
					}

					private readonly bool _hasValue;

					public bool HasValue {
						get { return _hasValue; }
					}

					private readonly T _value;

					public T Value {
						get { return _value; }
					}

					public static implicit operator Maybe<T> (T value)
					{
						return new Maybe<T> (value);
					}
				}
			}

			public abstract class ReadOnlyPropertyFixtureBase<PropertyT> : PropertyFixtureBase<PropertyT> {
				protected ReadOnlyPropertyFixtureBase (
					Expression<Func<Game, PropertyT> > propertyExpression)
					: base (propertyExpression)
				{ }

				[Test]
				public void Is_read_only ()
				{
					if (PropertyInfo.GetGetMethod() == null)
						Assert.Fail ("Property {0} is not readable.", PropertyInfo.Name);
					if (PropertyInfo.GetSetMethod() != null)
						Assert.Fail ("Property {0} is writeable.", PropertyInfo.Name);
				}
			}

			public abstract class ReadWritePropertyFixtureBase<PropertyT> : PropertyFixtureBase<PropertyT> {
				protected ReadWritePropertyFixtureBase (
					Expression<Func<Game, PropertyT>> propertyExpression)
					: base (propertyExpression)
				{ }

				protected new PropertyT ThisProperty {
					get { return base.ThisProperty; }
					set {
						try { PropertyInfo.SetValue (Game, value, null);
						} catch (TargetInvocationException ex) {
							// Unpack the real exception
							throw ex.InnerException;
						}
					}
				}

				[Test]
				public void Is_read_write ()
				{
					if (PropertyInfo.GetGetMethod() == null)
						Assert.Fail ("Property {0} is not readable.", PropertyInfo.Name);
					if (PropertyInfo.GetSetMethod() == null)
						Assert.Fail ("Property {0} is not writeable.", PropertyInfo.Name);
				}

				protected void AddLegalValue (PropertyT value)
				{
					_legalValues.Add (value);
				}

				protected void AddIllegalValue<ExceptionT> (PropertyT value)
				{
					_illegalValues.Add (Tuple.Create (value, typeof (ExceptionT)));
				}

				private List<PropertyT> _legalValues = new List<PropertyT> ();

				protected List<PropertyT> LegalValues {
					get { return _legalValues; }
				}

				private List<Tuple<PropertyT, Type>> _illegalValues =
					new List<Tuple<PropertyT, Type>> ();

				protected List<Tuple<PropertyT, Type>> IllegalValues {
					get { return _illegalValues; }
				}

				[Test, TestCaseSource ("LegalValues"), Ignore("The sourceName specified on a TestCaseSourceAttribute must refer to a static field, property or method.")]
				public virtual void Can_set_legal_value (PropertyT value)
				{
					Assert.DoesNotThrow (() => ThisProperty = value);
				}

				[Test, TestCaseSource ("IllegalValues"), Ignore("The sourceName specified on a TestCaseSourceAttribute must refer to a static field, property or method.")]
				public virtual void Cannot_set_illegal_value (Tuple<PropertyT, Type> valueAndException)
				{
					var value = valueAndException.Item1;
					var exceptionType = valueAndException.Item2;
					Assert.Throws (exceptionType, () => ThisProperty = value);
				}
			}
		}
	}
}
