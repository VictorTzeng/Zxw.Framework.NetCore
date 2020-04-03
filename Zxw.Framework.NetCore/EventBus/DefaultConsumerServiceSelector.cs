using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using DotNetCore.CAP;
using DotNetCore.CAP.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Zxw.Framework.NetCore.Extensions;
using Zxw.Framework.NetCore.IoC;

namespace Zxw.Framework.NetCore.EventBus
{
    internal class DefaultConsumerServiceSelector : IConsumerServiceSelector,ISingletonDependency
    {
        private readonly CapOptions _capOptions;
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// since this class be designed as a Singleton service,the following two list must be thread safe!!!
        /// </summary>
        private readonly ConcurrentDictionary<string, List<RegexExecuteDescriptor<ConsumerExecutorDescriptor>>> _asteriskList;
        private readonly ConcurrentDictionary<string, List<RegexExecuteDescriptor<ConsumerExecutorDescriptor>>> _poundList;

        /// <summary>
        /// Creates a new <see cref="DefaultConsumerServiceSelector" />.
        /// </summary>
        public DefaultConsumerServiceSelector(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _capOptions = serviceProvider.GetService<IOptions<CapOptions>>().Value;

            _asteriskList = new ConcurrentDictionary<string, List<RegexExecuteDescriptor<ConsumerExecutorDescriptor>>>();
            _poundList = new ConcurrentDictionary<string, List<RegexExecuteDescriptor<ConsumerExecutorDescriptor>>>();
        }

        public IReadOnlyList<ConsumerExecutorDescriptor> SelectCandidates()
        {
            var executorDescriptorList = new List<ConsumerExecutorDescriptor>();

            executorDescriptorList.AddRange(FindConsumersFromInterfaceTypes(_serviceProvider));
            return executorDescriptorList;
        }

        public ConsumerExecutorDescriptor SelectBestCandidate(string key, IReadOnlyList<ConsumerExecutorDescriptor> executeDescriptor)
        {
            var result = MatchUsingName(key, executeDescriptor);
            if (result != null)
            {
                return result;
            }

            //[*] match with regex, i.e.  foo.*.abc
            result = MatchAsteriskUsingRegex(key, executeDescriptor);
            if (result != null)
            {
                return result;
            }

            //[#] match regex, i.e. foo.#
            result = MatchPoundUsingRegex(key, executeDescriptor);
            return result;
        }

        private IEnumerable<ConsumerExecutorDescriptor> FindConsumersFromInterfaceTypes(
            IServiceProvider provider)
        {
            var executorDescriptorList = new List<ConsumerExecutorDescriptor>();

            using (var scoped = provider.CreateScope())
            {
                var scopedProvider = scoped.ServiceProvider;
                var consumerServices = scopedProvider.GetServices<ICapSubscribe>();
                foreach (var service in consumerServices)
                {
                    var typeInfo = service.GetType().GetTypeInfo();

                    // 必须是非抽象类
                    if (!typeInfo.IsClass || typeInfo.IsAbstract)
                        continue;

                    // 继承自IEventHandler<>
                    if (!typeInfo.IsChildTypeOfGenericType(typeof(IEventHandler<>)))
                        continue;

                    executorDescriptorList.AddRange(GetTopicAttributesDescription(typeInfo));
                }

                return executorDescriptorList;
            }
        }

        private List<string> GetEventNamesFromTypeInfo(TypeInfo typeInfo)
        {
            List<string> names = new List<string>();
            foreach (var item in typeInfo.ImplementedInterfaces)
            {
                var @interface = item.GetTypeInfo();
                if (!@interface.IsGenericType)
                    continue;
                if (@interface.GenericTypeArguments.Length != 1)
                    continue;

                var eventType = @interface.GenericTypeArguments[0].GetTypeInfo();
                if (!eventType.IsChildTypeOf<IEvent>())
                    continue;

                names.Add(eventType.FullName);
            }
            return names;
        }

        private IEnumerable<ConsumerExecutorDescriptor> GetTopicAttributesDescription(TypeInfo typeInfo)
        {
            var names = GetEventNamesFromTypeInfo(typeInfo);
            if (names.IsNullOrEmpty())
                return new ConsumerExecutorDescriptor[] { };

            List<ConsumerExecutorDescriptor> results = new List<ConsumerExecutorDescriptor>();
            var methods = typeInfo.GetMethods();
            foreach (var eventName in names)
            {
                var method = methods.FirstOrDefault(r => r.Name == "Execute"
                                                        && r.GetParameters().Length == 1
                                                        && r.GetParameters()[0].ParameterType.FullName == eventName
                                                        && r.GetParameters()[0].ParameterType.IsChildTypeOf<IEvent>());
                if (method == null)
                    continue;

                results.Add(new ConsumerExecutorDescriptor
                {
                    Attribute = new CapSubscribeAttribute(eventName) { Group = typeInfo.FullName + "." + _capOptions.Version },
                    ImplTypeInfo = typeInfo,
                    MethodInfo = method
                });
            }

            return results;
        }

        private ConsumerExecutorDescriptor MatchUsingName(string key, IReadOnlyList<ConsumerExecutorDescriptor> executeDescriptor)
        {
            return executeDescriptor.FirstOrDefault(x => x.Attribute.Name == key);
        }

        private ConsumerExecutorDescriptor MatchAsteriskUsingRegex(string key, IReadOnlyList<ConsumerExecutorDescriptor> executeDescriptor)
        {
            var group = executeDescriptor.First().Attribute.Group;
            if (!_asteriskList.TryGetValue(group, out var tmpList))
            {
                tmpList = executeDescriptor.Where(x => x.Attribute.Name.IndexOf('*') >= 0)
                    .Select(x => new RegexExecuteDescriptor<ConsumerExecutorDescriptor>
                    {
                        Name = ("^" + x.Attribute.Name + "$").Replace("*", "[0-9_a-zA-Z]+").Replace(".", "\\."),
                        Descriptor = x
                    }).ToList();
                _asteriskList.TryAdd(group, tmpList);
            }

            foreach (var red in tmpList)
            {
                if (Regex.IsMatch(key, red.Name, RegexOptions.Singleline))
                {
                    return red.Descriptor;
                }
            }

            return null;
        }

        private ConsumerExecutorDescriptor MatchPoundUsingRegex(string key, IReadOnlyList<ConsumerExecutorDescriptor> executeDescriptor)
        {
            var group = executeDescriptor.First().Attribute.Group;
            if (!_poundList.TryGetValue(group, out var tmpList))
            {
                tmpList = executeDescriptor
                    .Where(x => x.Attribute.Name.IndexOf('#') >= 0)
                    .Select(x => new RegexExecuteDescriptor<ConsumerExecutorDescriptor>
                    {
                        Name = ("^" + x.Attribute.Name + "$").Replace("#", "[0-9_a-zA-Z\\.]+"),
                        Descriptor = x
                    }).ToList();
                _poundList.TryAdd(group, tmpList);
            }

            foreach (var red in tmpList)
            {
                if (Regex.IsMatch(key, red.Name, RegexOptions.Singleline))
                {
                    return red.Descriptor;
                }
            }

            return null;
        }


        private class RegexExecuteDescriptor<T>
        {
            public string Name { get; set; }

            public T Descriptor { get; set; }
        }
    }
}
