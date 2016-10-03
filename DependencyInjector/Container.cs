﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace DependencyInjector
{
    public class Container
    {
        private IDictionary<Type, Type> dependencies;
        private ContainerOptions options;
        private const ContainerOptions defaultOptions = ContainerOptions.None;

        public Container() : this(defaultOptions)
        {
        }

        public Container(ContainerOptions options)
        {
            this.options = options;
            this.dependencies = new Dictionary<Type, Type>();
        }

        public void RegisterType<TDependencyType, TResolveType>()
            where TDependencyType : class
            where TResolveType : class
        {
            this.dependencies.Add(typeof(TDependencyType), typeof(TResolveType));
        }

        public T Resolve<T>() where T : class
        {
            var constructors = typeof(T)
                .GetConstructors()
                .OrderByDescending(x => x.GetParameters().Count());

            if (!constructors.Any())
            {
                throw new ArgumentException("The class to be resolved does not have any public constructors!");
            }

            foreach (var constructor in constructors)
            {
                var parameters = constructor.GetParameters();

                if (parameters.Length == 0)
                {
                    return Activator.CreateInstance<T>();
                }
                else
                {
                    List<object> parameterObjects = new List<object>();

                    foreach (var parameter in parameters)
                    {
                        var parameterType = parameter.ParameterType;

                        if (this.options.HasFlag(ContainerOptions.UseDefaultValue))
                        {
                            if (parameter.HasDefaultValue)
                            {
                                var res = Convert.ChangeType(parameter.DefaultValue, parameterType);
                                parameterObjects.Add(res);
                                continue;
                            }
                        }

                        if (parameterType.IsAbstract || parameterType.IsInterface)
                        {
                            var concreteObjectType = this.dependencies[parameterType];
                            var method = typeof(Container).GetMethod("Resolve").MakeGenericMethod(concreteObjectType);
                            var obj = method.Invoke(this, null);

                            parameterObjects.Add(obj);
                        }
                        else if (parameterType.IsPrimitive || parameterType.GetConstructors().Any(x => !x.GetParameters().Any()))
                        {
                            var obj = Activator.CreateInstance(parameterType);
                            parameterObjects.Add(obj);
                        }
                    }

                    if (parameterObjects.Count != constructor.GetParameters().Length)
                    {
                        continue;
                    }

                    var createdObject = (T)Activator.CreateInstance(typeof(T), parameterObjects.ToArray());
                    return createdObject;
                }
            }

            throw new Exception("Could not resolve the dependency");
        }
    }
}