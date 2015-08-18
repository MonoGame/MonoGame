#region License
/*
Microsoft Public License (Ms-PL)
MonoGame - Copyright © 2009-2012 The MonoGame Team

All rights reserved.

This license governs use of the accompanying software. If you use the software,
you accept this license. If you do not accept the license, do not use the
software.

1. Definitions

The terms "reproduce," "reproduction," "derivative works," and "distribution"
have the same meaning here as under U.S. copyright law.

A "contribution" is the original software, or any additions or changes to the
software.

A "contributor" is any person that distributes its contribution under this
license.

"Licensed patents" are a contributor's patent claims that read directly on its
contribution.

2. Grant of Rights

(A) Copyright Grant- Subject to the terms of this license, including the
license conditions and limitations in section 3, each contributor grants you a
non-exclusive, worldwide, royalty-free copyright license to reproduce its
contribution, prepare derivative works of its contribution, and distribute its
contribution or any derivative works that you create.

(B) Patent Grant- Subject to the terms of this license, including the license
conditions and limitations in section 3, each contributor grants you a
non-exclusive, worldwide, royalty-free license under its licensed patents to
make, have made, use, sell, offer for sale, import, and/or otherwise dispose of
its contribution in the software or derivative works of the contribution in the
software.

3. Conditions and Limitations

(A) No Trademark License- This license does not grant you rights to use any
contributors' name, logo, or trademarks.

(B) If you bring a patent claim against any contributor over patents that you
claim are infringed by the software, your patent license from such contributor
to the software ends automatically.

(C) If you distribute any portion of the software, you must retain all
copyright, patent, trademark, and attribution notices that are present in the
software.

(D) If you distribute any portion of the software in source code form, you may
do so only under this license by including a complete copy of this license with
your distribution. If you distribute any portion of the software in compiled or
object code form, you may only do so under a license that complies with this
license.

(E) The software is licensed "as-is." You bear the risk of using it. The
contributors give no express warranties, guarantees or conditions. You may have
additional consumer rights under your local laws which this license cannot
change. To the extent permitted under your local laws, the contributors exclude
the implied warranties of merchantability, fitness for a particular purpose and
non-infringement.
*/
#endregion License

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
			public class Components : ReadOnlyPropertyFixtureBase<GameComponentCollection> 
            {
				public Components ()
					: base (g => g.Components)
				{ }

				[Ignore]
				public override void Has_correct_default_value () { }

				[Test]
				public void Is_available_before_Run ()
				{
					Assert.That (Game, HasThisProperty.Not.Null);
				}
			}

			[TestFixture]
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

				[Ignore]
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
			public class GraphicsDevice_ : ReadOnlyPropertyFixtureBase<GraphicsDevice> {
				public GraphicsDevice_ ()
					: base (g => g.GraphicsDevice)
				{ }

				[Ignore]
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
			public class IsActive : ReadOnlyPropertyFixtureBase<bool> {
				public IsActive ()
					: base (g => g.IsActive)
				{
					DefaultValue = false;
				}
			}

			[TestFixture]
			public class IsFixedTimeStep : ReadWritePropertyFixtureBase<bool> {
				public IsFixedTimeStep ()
					: base (g => g.IsFixedTimeStep)
				{
					DefaultValue = true;

					AddLegalValue (true);
					AddLegalValue (false);
				}

				[Ignore]
				public override void Cannot_set_illegal_value (Tuple<bool, Type> valueAndException) { }
			}

			[TestFixture]
			public class IsMouseVisible : ReadWritePropertyFixtureBase<bool> {
				public IsMouseVisible ()
					: base (g => g.IsMouseVisible)
				{
					DefaultValue = false;

					AddLegalValue (true);
					AddLegalValue (false);
				}

				[Ignore]
				public override void Cannot_set_illegal_value (Tuple<bool, Type> valueAndException) { }
			}

			[TestFixture]
			public class LaunchParameters_ : ReadOnlyPropertyFixtureBase<LaunchParameters> {
				public LaunchParameters_ ()
					: base (g => g.LaunchParameters)
				{ }

				[Ignore]
				public override void Has_correct_default_value () { }

				[Test]
				public void Is_available_before_Run ()
				{
					Assert.That (Game, HasThisProperty.Not.Null);
				}
			}

			[TestFixture]
			public class Services : ReadOnlyPropertyFixtureBase<GameServiceContainer> {
				public Services ()
					: base (g => g.Services)
				{ }

				[Ignore]
				public override void Has_correct_default_value () { }

				[Test]
				public void Is_available_before_Run ()
				{
					Assert.That (Game, HasThisProperty.Not.Null);
				}
			}

			[TestFixture]
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
			public class Window : ReadOnlyPropertyFixtureBase<GameWindow> {
				public Window ()
					: base (g => g.Window)
				{ }

				[Ignore]
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

				[Test, TestCaseSource ("LegalValues")]
				public virtual void Can_set_legal_value (PropertyT value)
				{
					Assert.DoesNotThrow (() => ThisProperty = value);
				}

				[Test, TestCaseSource ("IllegalValues")]
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
