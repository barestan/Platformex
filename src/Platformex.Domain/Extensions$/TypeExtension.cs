using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

[assembly:InternalsVisibleTo("Platformex.Application")]

namespace Platformex.Domain
{
    internal static class TypeExtension
    {
        internal static IReadOnlyList<Tuple<Type, Type, bool>> GetSubscribersTypes(this Type type, bool isSync)
        {
            var interfaceTypes = new [] {
                (isSync ? typeof(ISubscribeSyncTo<,>) : typeof(ISubscribeTo<,>)),
                (isSync ? typeof(IStartedBySync<,>) : typeof(IStartedBy<,>))
            };

            var interfaces = type
                .GetTypeInfo()
                .GetInterfaces()
                .Select(i => i.GetTypeInfo())
                .ToList();
            var types = interfaces
                .Where(i => i.IsGenericType && interfaceTypes.Contains(i.GetGenericTypeDefinition()))
                .Select(i => new Tuple<Type, Type, bool>(i.GetGenericArguments()[0], 
                    i.GetGenericArguments()[1], i.Name.Contains("Sync")))
                .ToList();

            return types;
        }
        internal static IReadOnlyDictionary<Type, Action<T, IAggregateEvent>> GetAggregateEventApplyMethods<TIdentity, T>(this Type type)
            where TIdentity : Identity<TIdentity>
        {
            var aggregateEventType = typeof(IAggregateEvent<TIdentity>);

            return type
                .GetTypeInfo()
                .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(mi =>
                {
                    if (!string.Equals(mi.Name, "Apply", StringComparison.Ordinal) &&
                        !mi.Name.EndsWith(".Apply", StringComparison.Ordinal))
                    {
                        return false;
                    }

                    var parameters = mi.GetParameters();
                    return
                        parameters.Length == 1 &&
                        aggregateEventType.GetTypeInfo().IsAssignableFrom(parameters[0].ParameterType);
                })
                .ToDictionary(
                    mi => mi.GetParameters()[0].ParameterType,
                    mi => ReflectionHelper.CompileMethodInvocation<Action<T, IAggregateEvent>>(type, mi.Name, mi.GetParameters()[0].ParameterType));
        }
        internal static IReadOnlyDictionary<Type, Func<TReadModel, IDomainEvent, Task>> GetReadModelEventApplyMethods<TReadModel>(this Type type)
        {
            var aggregateEventType = typeof(IDomainEvent);

            return type
                .GetTypeInfo()
                .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(mi =>
                {
                    if (mi.Name != "HandleAsync") return false;
                    var parameters = mi.GetParameters();
                    return
                        parameters.Length == 1 &&
                        aggregateEventType.GetTypeInfo().IsAssignableFrom(parameters[0].ParameterType);
                })
                .ToDictionary(
                    mi => mi.GetParameters()[0].ParameterType,
                    mi => ReflectionHelper.CompileMethodInvocation<Func<TReadModel, IDomainEvent, Task>>(type, "HandleAsync", mi.GetParameters()[0].ParameterType));
        }
    }
}