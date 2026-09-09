using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Builders;

namespace MonoGame.Tests 
{
    /// <summary>
    /// Marshall the test onto the main UI thread.
    /// </summary>
    /// <remarks>
    /// Used to decorate a whole test class.<br/>
    /// It replaces <see cref="TestFixtureAttribute"/> usage and all test methods in that class are marshalled to the UI thread.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    sealed class RunOnUiTestFixtureAttribute : Attribute, IFixtureBuilder2
    {
        public IEnumerable<TestSuite> BuildFrom(ITypeInfo typeInfo, IPreFilter filter)
        {
            var testFixtureBuilder = new NUnitTestFixtureBuilder();
            var testSuite = testFixtureBuilder.BuildFrom(typeInfo, filter);
            
            WrapTestMethods(testSuite);
            
            return [testSuite];
        }

        public IEnumerable<TestSuite> BuildFrom(ITypeInfo typeInfo)
        {
            return BuildFrom(typeInfo, new MatchAllFilter());
        }

        private void WrapTestMethods(TestSuite suite)
        {
            foreach (var test in suite.Tests)
            {
                if (test is TestMethod testMethod)
                {
                    // Skip if the method already has RunOnUiAttribute.
                    if (!testMethod.Method.IsDefined<RunOnUiAttribute>(true))
                    {
                        testMethod.Method = new MethodInfoWithAttribute(testMethod.Method, new RunOnUiAttribute());
                    }
                }
                else if (test is TestSuite childSuite)
                {
                    // Wrap parameterized unit tests.
                    WrapTestMethods(childSuite);
                }
            }
        }

        // Apply to all methods in a class.
        private class MatchAllFilter : IPreFilter
        {
            public bool IsMatch(Type type) => true;
            public bool IsMatch(Type type, MethodInfo method) => true;
        }

        private class MethodInfoWithAttribute : IMethodInfo
        {
            private readonly IMethodInfo _innerMethodInfo;
            private readonly RunOnUiAttribute _attribute;

            public MethodInfoWithAttribute(IMethodInfo innerMethodInfo, RunOnUiAttribute attribute)
            {
                _innerMethodInfo = innerMethodInfo;
                _attribute = attribute;
            }

            public T[] GetCustomAttributes<T>(bool inherit) where T : class
            {
                var customAttributes = _innerMethodInfo.GetCustomAttributes<T>(inherit);
                if (_attribute is T castAttribute)
                {
                    if (Array.IndexOf(customAttributes, castAttribute) < 0)
                    {
                        var list = new List<T>(customAttributes) { castAttribute };
                        
                        return list.ToArray();
                    }
                }
                return customAttributes;
            }

            public bool IsDefined<T>(bool inherit) where T : class
            {
                if (_attribute is T)
                {
                    return true;
                }
                
                return _innerMethodInfo.IsDefined<T>(inherit);
            }

            // Reuse everything else from the inner MethodInfo.
            public ITypeInfo TypeInfo => _innerMethodInfo.TypeInfo;
            public MethodInfo MethodInfo => _innerMethodInfo.MethodInfo;
            public string Name => _innerMethodInfo.Name;
            public bool IsAbstract => _innerMethodInfo.IsAbstract;
            public bool IsPublic => _innerMethodInfo.IsPublic;
            public bool IsStatic => _innerMethodInfo.IsStatic;
            public bool ContainsGenericParameters => _innerMethodInfo.ContainsGenericParameters;
            public bool IsGenericMethod => _innerMethodInfo.IsGenericMethod;
            public bool IsGenericMethodDefinition => _innerMethodInfo.IsGenericMethodDefinition;
            public ITypeInfo ReturnType => _innerMethodInfo.ReturnType;
            public IParameterInfo[] GetParameters() => _innerMethodInfo.GetParameters();
            public Type[] GetGenericArguments() => _innerMethodInfo.GetGenericArguments();
            public IMethodInfo MakeGenericMethod(params Type[] typeArguments)
                => _innerMethodInfo.MakeGenericMethod(typeArguments);
            public object Invoke(object fixture, params object[] args)
                => _innerMethodInfo.Invoke(fixture, args);
        }
    }
}
