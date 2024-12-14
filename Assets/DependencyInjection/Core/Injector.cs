using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace DependencyInjection
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method)]
    public sealed class InjectAttribute : Attribute
    {

    }
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class ProvideAttribute : Attribute
    {

    }
    public interface IDependencyProvider { }
    [DefaultExecutionOrder(-1000)]
    public class Injector : Singleton<Injector>
    {
        const BindingFlags BINDING_FLAGS = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        readonly Dictionary<Type, object> registry = new Dictionary<Type, object>();
        protected override void Awake()
        {
            base.Awake();
            var providers = FindMonoBehaviours().OfType<IDependencyProvider>();
            foreach (var provider in providers)
            {
                RegisterProvider(provider);
            }

            var injectables = FindMonoBehaviours().Where(IsInjectable);
            foreach (var injectable in injectables)
            {
                Inject(injectable);
            }
        }

        private void Inject(object injectable)
        {
            var type = injectable.GetType();

            var injectableFields = type.GetFields(BINDING_FLAGS).Where(field => Attribute.IsDefined(field, typeof(InjectAttribute)));
            foreach (var field in injectableFields)
            {
                var fieldType = field.FieldType;
                var resolvedInstance = Resolve(fieldType);
                if (resolvedInstance == null)
                {
                    throw new Exception($"Failed to inject {fieldType.Name} into {type.Name}");
                }
                field.SetValue(injectable, resolvedInstance);
            }

            var injectableMethods = type.GetMethods(BINDING_FLAGS).Where(method => Attribute.IsDefined(method, typeof(InjectAttribute)));
            foreach (var method in injectableMethods)
            {
                var parameterTypes = method.GetParameters().Select(parameter => parameter.ParameterType).ToArray();
                var resolvedInstances = parameterTypes.Select(Resolve).ToArray();
                if (resolvedInstances.Any(resolvedInstance => resolvedInstance == null))
                {
                    throw new Exception($"Failed to inject {type.Name} into {method.Name}");
                }
                method.Invoke(injectable, resolvedInstances);

            }
        }
        private object Resolve(Type type)
        {
            registry.TryGetValue(type, out var resolvedInstance);
            return resolvedInstance;
        }

        private bool IsInjectable(MonoBehaviour mb)
        {
            var members = mb.GetType().GetMembers(BINDING_FLAGS);
            return members.Any(member => Attribute.IsDefined(member, typeof(InjectAttribute)));
        }

        private void RegisterProvider(IDependencyProvider provider)
        {
            var methods = provider.GetType().GetMethods(BINDING_FLAGS);
            foreach (var method in methods)
            {
                if (!Attribute.IsDefined(method, typeof(ProvideAttribute)))
                {
                    continue;
                }
                var returnType = method.ReturnType;
                var providedInstance = method.Invoke(provider, null);
                if (providedInstance != null)
                {
                    registry.Add(returnType, providedInstance);
                }
                else
                {
                    throw new Exception($"Provider {provider.GetType().Name} Returned null");
                }
            }

        }
        static MonoBehaviour[] FindMonoBehaviours()
        {
            return FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.InstanceID);
        }
    }
}