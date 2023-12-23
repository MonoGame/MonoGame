// MIT License - Copyright (C) The Mono.Xna Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using MonoGame.Framework.Utilities;

namespace Microsoft.Xna.Framework
{
    /// <summary>
    /// A container for services for a <see cref="Game"/>.
    /// </summary>
    public class GameServiceContainer : IServiceProvider
    {
        private readonly Dictionary<Type, object> _services = new();

        /// <summary>
        /// Create an empty <see cref="GameServiceContainer"/>.
        /// </summary>
        public GameServiceContainer()
        {
        }

        /// <summary>
        /// Add a service provider to this container.
        /// </summary>
        /// <param name="type">The type of the service.</param>
        /// <param name="provider">The provider of the service.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="type"/> or <paramref name="provider"/> is <code>null</code>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="provider"/> cannot be assigned to <paramref name="type"/>.
        /// </exception>
        public void AddService(Type type, object provider)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            if (!ReflectionHelpers.IsAssignableFrom(type, provider))
                throw new ArgumentException("The provider does not match the specified service type!");

            _services.Add(type, provider);
        }

        /// <summary>
        /// Get a service provider for the service of the specified type.
        /// </summary>
        /// <param name="type">The type of the service.</param>
        /// <returns>
        /// A service provider for the service of the specified type or <code>null</code> if
        /// no suitable service provider is registered in this container.
        /// </returns>
        /// <exception cref="ArgumentNullException">If the specified type is <code>null</code>.</exception>
        public object GetService(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return _services.GetValueOrDefault(type);
        }

        /// <summary>
        /// Remove the service with the specified type. Does nothing no service of the specified type is registered.
        /// </summary>
        /// <param name="type">The type of the service to remove.</param>
        /// <exception cref="ArgumentNullException">If the specified type is <code>null</code>.</exception>
        public void RemoveService(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            _services.Remove(type);
        }

        /// <summary>
        /// Add a service provider to this container.
        /// </summary>
        /// <typeparam name="T">The type of the service.</typeparam>
        /// <param name="provider">The provider of the service.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="provider"/> is <code>null</code>.
        /// </exception>
        public void AddService<T>(T provider)
        {
            AddService(typeof(T), provider);
        }

        /// <summary>
        /// Get a service provider of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the service provider.</typeparam>
        /// <returns>
        /// A service provider of the specified type or <code>null</code> if
        /// no suitable service provider is registered in this container.
        /// </returns>
 	    public T GetService<T>() where T : class
        {
            var service = GetService(typeof(T));
            return (T)service;
        }
    }
}
