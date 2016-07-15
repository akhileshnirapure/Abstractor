﻿using System;
using System.Collections.Generic;
using System.Reflection;

namespace Abstractor.Cqrs.Interfaces.CompositionRoot
{
    /// <summary>
    ///     Abstraction of the inversion of control container that exposes only the methods required by the framework.
    /// </summary>
    public interface IContainer
    {
        object GetInstance(Type type);
        IEnumerable<object> GetAllInstances(Type type);
        object GetCurrentLifetimeScope();
        IDisposable BeginLifetimeScope();
        void RegisterScoped<TService, TImplementation>();
        void RegisterScoped(Type serviceType, Type implementationType);
        void RegisterTransient(Type serviceType, Type implementationType);
        void RegisterTransient(Type openGenericServiceType, IEnumerable<Assembly> assemblies);
        void RegisterCollection(Type openGenericServiceType, IEnumerable<Assembly> assemblies);

        void RegisterSingleton<TService, TImplementation>()
            where TService : class
            where TImplementation : class, TService;

        void RegisterDecoratorTransient(Type serviceType, Type decoratorType);
        void RegisterDecoratorSingleton(Type serviceType, Type decoratorType);

        /// <summary>
        ///     Provides functionality for the deferred resolving of unregistered types.
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        void RegisterLazySingleton<TService, TImplementation>()
            where TService : class
            where TImplementation : class, TService;

        /// <summary>
        ///     Provides functionality for resolving delegates of type <see cref="Func{T}" />.
        /// </summary>
        void AllowResolvingFuncFactories();

        void Register<TService>(Func<TService> instanceCreator)
            where TService : class;
    }
}