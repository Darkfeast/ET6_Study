using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ET
{
    using OneTypeSystems = UnOrderMultiMap<Type, object>;

    public sealed class EventSystem: IDisposable
    {
        private class TypeSystems
        {
            private readonly Dictionary<Type, OneTypeSystems> typeSystemsMap = new Dictionary<Type, OneTypeSystems>();

            public OneTypeSystems GetOrCreateOneTypeSystems(Type type)
            {
                OneTypeSystems systems = null;
                this.typeSystemsMap.TryGetValue(type, out systems);
                if (systems != null)
                {
                    return systems;
                }

                systems = new OneTypeSystems();
                this.typeSystemsMap.Add(type, systems);
                return systems;
            }

            public OneTypeSystems GetOneTypeSystems(Type type)
            {
                OneTypeSystems systems = null;
                this.typeSystemsMap.TryGetValue(type, out systems);
                return systems;
            }

            public List<object> GetSystems(Type type, Type systemType)
            {
                OneTypeSystems oneTypeSystems = null;
                if (!this.typeSystemsMap.TryGetValue(type, out oneTypeSystems))
                {
                    return null;
                }

                if (!oneTypeSystems.TryGetValue(systemType, out List<object> systems))
                {
                    return null;
                }

                return systems;
            }
        }

        private static EventSystem instance;

        public static EventSystem Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new EventSystem();
                }

                return instance;
            }
        }

        private readonly Dictionary<long, Entity> allEntities = new Dictionary<long, Entity>();

        private readonly Dictionary<string, Assembly> assemblies = new Dictionary<string, Assembly>();

        
        //所有加入的程序集里面的所有类，tmd 打印出来有7w个  不过很多是无用的类 
        private readonly Dictionary<string, Type> allTypes = new Dictionary<string, Type>();

        //从allTypes中过滤出  继承自 BaseAttribute 的所有子类  对应的 被该子类特性标记的类型列表 (以下为继承Base特性的所有子类特性
        //UIEvent, Event, ObjectSystem, Timer, AIHandler, ActorMessageHandler, ConsoleHandler, Message, MessageHandler, ResponseType, SessionStreamDispatcher, NumericWatcher, RobotCase, HttpHandler
        private readonly UnOrderMultiMap<Type, Type> types = new UnOrderMultiMap<Type, Type>();

        //被Event标记的, 且最上层接口为IEvent的类型
        private readonly Dictionary<Type, List<object>> allEvents = new Dictionary<Type, List<object>>();

        
        //实现ISystemType的 子接口如下:
        //
        //过滤被 ObjectSystem 标记的类 且最上层接口为 ISystemType 的类型, key为 事件类传入参数 , value为 所有事件类的参数跟key一样的事件类实例
        //key包含了entity组件类    因为组件也被作为了类参数T 
        private TypeSystems typeSystems = new TypeSystems();

        private Queue<long> updates = new Queue<long>();
        private Queue<long> updates2 = new Queue<long>();

        private Queue<long> loaders = new Queue<long>();
        private Queue<long> loaders2 = new Queue<long>();

        private Queue<long> lateUpdates = new Queue<long>();
        private Queue<long> lateUpdates2 = new Queue<long>();

        private EventSystem()
        {
        }

        private List<Type> GetBaseAttributes()
        {
            List<Type> attributeTypes = new List<Type>();
            foreach (var kv in this.allTypes)
            {
                Type type = kv.Value;
                if (type.IsAbstract)
                {
                    continue;
                }

                if (type.IsSubclassOf(typeof (BaseAttribute)))
                {
                    attributeTypes.Add(type);
                }
            }

            return attributeTypes;
        }

        public void Add(Dictionary<string, Type> addTypes)
        {
            this.allTypes.Clear();
            foreach (var kv in addTypes)
            {
                this.allTypes[kv.Key] = kv.Value;
            }

            this.types.Clear();
            List<Type> baseAttributeTypes = GetBaseAttributes();
            foreach (Type baseAttributeType in baseAttributeTypes)
            {
                foreach (var kv in this.allTypes)
                {
                    Type type = kv.Value;
                    if (type.IsAbstract)
                    {
                        continue;
                    }

                    object[] objects = type.GetCustomAttributes(baseAttributeType, true);
                    if (objects.Length == 0)
                    {
                        continue;
                    }

                    this.types.Add(baseAttributeType, type);
                }
            }

            
            //
            this.typeSystems = new TypeSystems();

            //找到types中  继承自BaseAttribute的  特定 ObjectSystemAttribute特性  所标记的类型列表
            foreach (Type type in this.GetTypes(typeof (ObjectSystemAttribute)))
            {
                object obj = Activator.CreateInstance(type);

                //进一步过滤出 实现 ISystemType接口的类型
                //其实就是模拟的生命周期类  因为实现ISystemType接口的类型有:
                //AwakeSystem, UpdateSystem, LateUpdateSystem, LoadSystem, AddComponentSystem, GetComponentSystem, DeserializeSystem, DestroySystem,
                //实现ISystemType接口的接口有:
                //IAwakeSystem, IUpdateSystem, ILateUpdateSystem, ILoadSystem, IAddComponentSystem, IGetComponentSystem, IDeserializeSystem, IDestroySystem,
                if (obj is ISystemType iSystemType)
                {
                    //将生命周期类型加入 typeSystems 并返回创建初始化的空字典列表
                    OneTypeSystems oneTypeSystems = this.typeSystems.GetOrCreateOneTypeSystems(iSystemType.Type());
                    //<实现IAwak之类接口的生命周期类型, <实现IAwak之类接口的生命周期类型的父类所实现的接口类型 ,<对应的事件实例>>  >
                    oneTypeSystems.Add(iSystemType.SystemType(), obj);
                    
                    //例如,
                    //public abstract class AwakeSystem<T> : IAwakeSystem where T: IAwake
                    
                    // public class ConfigComponent_SetConfigLoader_Awake: AwakeSystem<ConfigComponent>
                    // public class ConfigAwakeSystem : AwakeSystem<ConfigComponent>
                    //可以看到上面两个类传入的参数都是 ConfigComponent 
                    //那么 他们的key都是 typeof(ConfigComponent)  而且二级key都是AwakeSystem<ConfigComponent>  二级key对应的列表存了两个值
                    //即：ConfigComponent_SetConfigLoader_Awake,   ConfigAwakeSystem
                    //继承结构  实际生命周期类 : 生命周期类<参数类型> : 生命周期接口<参数类型> : ISystemType
                    
                    //typeSystems 字典结构
                    //< typeof(T), < typeof(IAwakeSystem),< 所有继承AwakeSystem<T>的实际类型> > >
                    //< 参数类型, < 生命周期接口类型, < 所有继承生命周期类<T>的实际类型 > > >
                }
            }

            this.allEvents.Clear();
            foreach (Type type in types[typeof (EventAttribute)])
            {
                IEvent iEvent = Activator.CreateInstance(type) as IEvent;
                if (iEvent != null)
                {
                    //获取的是事件类传入的参数的类型
                    Type eventType = iEvent.GetEventType();
                    if (!this.allEvents.ContainsKey(eventType))
                    {
                        this.allEvents.Add(eventType, new List<object>());
                    }

                    this.allEvents[eventType].Add(iEvent);
                }
            }
        }

        public void Add(Assembly assembly)
        {
            this.assemblies[$"{assembly.GetName().Name}.dll"] = assembly;

            Dictionary<string, Type> dictionary = new Dictionary<string, Type>();

            foreach (Assembly ass in this.assemblies.Values)
            {
                foreach (Type type in ass.GetTypes())
                {
                    dictionary[type.FullName] = type;
                }
            }
            
            this.Add(dictionary);
        }

        public List<Type> GetTypes(Type systemAttributeType)
        {
            return this.types[systemAttributeType];
        }

        public Dictionary<string, Type> GetTypes()
        {
            return allTypes;
        }

        public Type GetType(string typeName)
        {
            return this.allTypes[typeName];
        }

        public void RegisterSystem(Entity component, bool isRegister = true)
        {
            if (!isRegister)
            {
                this.Remove(component.InstanceId);
                return;
            }

            this.allEntities.Add(component.InstanceId, component);

            Type type = component.GetType();

            //找到之前添加过的组件类 所对应的生命周期类
            OneTypeSystems oneTypeSystems = this.typeSystems.GetOneTypeSystems(type);;
            if (component is ILoad)
            {
                if (oneTypeSystems.ContainsKey(typeof (ILoadSystem)))
                {
                    this.loaders.Enqueue(component.InstanceId);
                }
            }

            if (component is IUpdate)
            {
                if (oneTypeSystems.ContainsKey(typeof (IUpdateSystem)))
                {
                    this.updates.Enqueue(component.InstanceId);
                }
            }

            if (component is ILateUpdate)
            {
                if (oneTypeSystems.ContainsKey(typeof (ILateUpdateSystem)))
                {
                    this.lateUpdates.Enqueue(component.InstanceId);
                }
            }
        }

        public void Remove(long instanceId)
        {
            this.allEntities.Remove(instanceId);
        }

        public Entity Get(long instanceId)
        {
            Entity component = null;
            this.allEntities.TryGetValue(instanceId, out component);
            return component;
        }

        public bool IsRegister(long instanceId)
        {
            return this.allEntities.ContainsKey(instanceId);
        }

        public void Deserialize(Entity component)
        {
            List<object> iDeserializeSystems = this.typeSystems.GetSystems(component.GetType(), typeof (IDeserializeSystem));
            if (iDeserializeSystems == null)
            {
                return;
            }

            for (int i = 0; i < iDeserializeSystems.Count; ++i)
            {
                IDeserializeSystem deserializeSystem = iDeserializeSystems[i] as IDeserializeSystem;
                if (deserializeSystem == null)
                {
                    continue;
                }

                try
                {
                    deserializeSystem.Run(component);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }
        
        // GetComponentSystem
        public void GetComponent(Entity entity, Entity component)
        {
            List<object> iGetSystem = this.typeSystems.GetSystems(entity.GetType(), typeof (IGetComponentSystem));
            if (iGetSystem == null)
            {
                return;
            }

            for (int i = 0; i < iGetSystem.Count; ++i)
            {
                IGetComponentSystem getSystem = iGetSystem[i] as IGetComponentSystem;
                if (getSystem == null)
                {
                    continue;
                }

                try
                {
                    getSystem.Run(entity, component);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }
        
        // AddComponentSystem
        public void AddComponent(Entity entity, Entity component)
        {
            List<object> iAddSystem = this.typeSystems.GetSystems(entity.GetType(), typeof (IAddComponentSystem));
            if (iAddSystem == null)
            {
                return;
            }

            for (int i = 0; i < iAddSystem.Count; ++i)
            {
                IAddComponentSystem addComponentSystem = iAddSystem[i] as IAddComponentSystem;
                if (addComponentSystem == null)
                {
                    continue;
                }

                try
                {
                    addComponentSystem.Run(entity, component);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        public void Awake(Entity component)
        {
            List<object> iAwakeSystems = this.typeSystems.GetSystems(component.GetType(), typeof (IAwakeSystem));
            if (iAwakeSystems == null)
            {
                return;
            }

            for (int i = 0; i < iAwakeSystems.Count; ++i)
            {
                IAwakeSystem aAwakeSystem = iAwakeSystems[i] as IAwakeSystem;
                if (aAwakeSystem == null)
                {
                    continue;
                }

                try
                {
                    aAwakeSystem.Run(component);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        public void Awake<P1>(Entity component, P1 p1)
        {
            List<object> iAwakeSystems = this.typeSystems.GetSystems(component.GetType(), typeof (IAwakeSystem<P1>));
            if (iAwakeSystems == null)
            {
                return;
            }

            for (int i = 0; i < iAwakeSystems.Count; ++i)
            {
                IAwakeSystem<P1> aAwakeSystem = iAwakeSystems[i] as IAwakeSystem<P1>;
                if (aAwakeSystem == null)
                {
                    continue;
                }

                try
                {
                    aAwakeSystem.Run(component, p1);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        public void Awake<P1, P2>(Entity component, P1 p1, P2 p2)
        {
            List<object> iAwakeSystems = this.typeSystems.GetSystems(component.GetType(), typeof (IAwakeSystem<P1, P2>));
            if (iAwakeSystems == null)
            {
                return;
            }

            for (int i = 0; i < iAwakeSystems.Count; ++i)
            {
                IAwakeSystem<P1, P2> aAwakeSystem = iAwakeSystems[i] as IAwakeSystem<P1, P2>;
                if (aAwakeSystem == null)
                {
                    continue;
                }

                try
                {
                    aAwakeSystem.Run(component, p1, p2);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        public void Awake<P1, P2, P3>(Entity component, P1 p1, P2 p2, P3 p3)
        {
            List<object> iAwakeSystems = this.typeSystems.GetSystems(component.GetType(), typeof (IAwakeSystem<P1, P2, P3>));
            if (iAwakeSystems == null)
            {
                return;
            }

            for (int i = 0; i < iAwakeSystems.Count; ++i)
            {
                IAwakeSystem<P1, P2, P3> aAwakeSystem = iAwakeSystems[i] as IAwakeSystem<P1, P2, P3>;
                if (aAwakeSystem == null)
                {
                    continue;
                }

                try
                {
                    aAwakeSystem.Run(component, p1, p2, p3);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        public void Awake<P1, P2, P3, P4>(Entity component, P1 p1, P2 p2, P3 p3, P4 p4)
        {
            List<object> iAwakeSystems = this.typeSystems.GetSystems(component.GetType(), typeof (IAwakeSystem<P1, P2, P3, P4>));
            if (iAwakeSystems == null)
            {
                return;
            }

            for (int i = 0; i < iAwakeSystems.Count; ++i)
            {
                IAwakeSystem<P1, P2, P3, P4> aAwakeSystem = iAwakeSystems[i] as IAwakeSystem<P1, P2, P3, P4>;
                if (aAwakeSystem == null)
                {
                    continue;
                }

                try
                {
                    aAwakeSystem.Run(component, p1, p2, p3, p4);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        public void Load()
        {
            while (this.loaders.Count > 0)
            {
                long instanceId = this.loaders.Dequeue();
                Entity component;
                if (!this.allEntities.TryGetValue(instanceId, out component))
                {
                    continue;
                }

                if (component.IsDisposed)
                {
                    continue;
                }

                List<object> iLoadSystems = this.typeSystems.GetSystems(component.GetType(), typeof (ILoadSystem));
                if (iLoadSystems == null)
                {
                    continue;
                }

                this.loaders2.Enqueue(instanceId);

                for (int i = 0; i < iLoadSystems.Count; ++i)
                {
                    ILoadSystem iLoadSystem = iLoadSystems[i] as ILoadSystem;
                    try
                    {
                        iLoadSystem.Run(component);
                    }
                    catch (Exception e)
                    {
                        Log.Error(e);
                    }
                }
            }

            ObjectHelper.Swap(ref this.loaders, ref this.loaders2);
        }

        public void Destroy(Entity component)
        {
            List<object> iDestroySystems = this.typeSystems.GetSystems(component.GetType(), typeof (IDestroySystem));
            if (iDestroySystems == null)
            {
                return;
            }

            for (int i = 0; i < iDestroySystems.Count; ++i)
            {
                IDestroySystem iDestroySystem = iDestroySystems[i] as IDestroySystem;
                if (iDestroySystem == null)
                {
                    continue;
                }

                try
                {
                    iDestroySystem.Run(component);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        public void Update()
        {
            while (this.updates.Count > 0)
            {
                long instanceId = this.updates.Dequeue();
                Entity component;
                if (!this.allEntities.TryGetValue(instanceId, out component))
                {
                    continue;
                }

                if (component.IsDisposed)
                {
                    continue;
                }

                List<object> iUpdateSystems = this.typeSystems.GetSystems(component.GetType(), typeof (IUpdateSystem));
                if (iUpdateSystems == null)
                {
                    continue;
                }

                this.updates2.Enqueue(instanceId);

                for (int i = 0; i < iUpdateSystems.Count; ++i)
                {
                    IUpdateSystem iUpdateSystem = iUpdateSystems[i] as IUpdateSystem;
                    try
                    {
                        iUpdateSystem.Run(component);
                    }
                    catch (Exception e)
                    {
                        Log.Error(e);
                    }
                }
            }

            ObjectHelper.Swap(ref this.updates, ref this.updates2);
        }

        public void LateUpdate()
        {
            while (this.lateUpdates.Count > 0)
            {
                long instanceId = this.lateUpdates.Dequeue();
                Entity component;
                if (!this.allEntities.TryGetValue(instanceId, out component))
                {
                    continue;
                }

                if (component.IsDisposed)
                {
                    continue;
                }

                List<object> iLateUpdateSystems = this.typeSystems.GetSystems(component.GetType(), typeof (ILateUpdateSystem));
                if (iLateUpdateSystems == null)
                {
                    continue;
                }

                this.lateUpdates2.Enqueue(instanceId);

                for (int i = 0; i < iLateUpdateSystems.Count; ++i)
                {
                    ILateUpdateSystem iLateUpdateSystem = iLateUpdateSystems[i] as ILateUpdateSystem;
                    try
                    {
                        iLateUpdateSystem.Run(component);
                    }
                    catch (Exception e)
                    {
                        Log.Error(e);
                    }
                }
            }

            ObjectHelper.Swap(ref this.lateUpdates, ref this.lateUpdates2);
        }

        public async ETTask PublishAsync<T>(T a) where T : struct
        {
            List<object> iEvents;
            if (!this.allEvents.TryGetValue(typeof(T), out iEvents))
            {
                return;
            }

            using (ListComponent<ETTask> list = ListComponent<ETTask>.Create())
            {
                for (int i = 0; i < iEvents.Count; ++i)
                {
                    object obj = iEvents[i];
                    if (!(obj is AEventAsync<T> aEvent))
                    {
                        Log.Error($"event error: {obj.GetType().Name}");
                        continue;
                    }

                    list.Add(aEvent.Handle(a));
                }

                try
                {
                    await ETTaskHelper.WaitAll(list);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }
        
        public void Publish<T>(T a) where T : struct
        {
            List<object> iEvents;
            if (!this.allEvents.TryGetValue(a.GetType(), out iEvents))
            {
                return;
            }
            
            for (int i = 0; i < iEvents.Count; ++i)
            {
                object obj = iEvents[i];
                if (!(obj is AEvent<T> aEvent))
                {
                    Log.Error($"event error: {obj.GetType().Name}");
                    continue;
                }
                aEvent.Handle(a);
            }
        }

        // ILRuntime消除GC使用，服务端不需要用这个
        public void PublishClass<T>(T a) where T : DisposeObject
        {
            List<object> iEvents;
            if (!this.allEvents.TryGetValue(a.GetType(), out iEvents))
            {
                return;
            }
            
            for (int i = 0; i < iEvents.Count; ++i)
            {
                object obj = iEvents[i];
                IEventClass aEvent = (IEventClass) obj;
                aEvent.Handle(a);
            }
            a.Dispose();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            HashSet<Type> noParent = new HashSet<Type>();
            Dictionary<Type, int> typeCount = new Dictionary<Type, int>();

            HashSet<Type> noDomain = new HashSet<Type>();

            foreach (var kv in this.allEntities)
            {
                Type type = kv.Value.GetType();
                if (kv.Value.Parent == null)
                {
                    noParent.Add(type);
                }

                if (kv.Value.Domain == null)
                {
                    noDomain.Add(type);
                }

                if (typeCount.ContainsKey(type))
                {
                    typeCount[type]++;
                }
                else
                {
                    typeCount[type] = 1;
                }
            }

            sb.AppendLine("not set parent type: ");
            foreach (Type type in noParent)
            {
                sb.AppendLine($"\t{type.Name}");
            }

            sb.AppendLine("not set domain type: ");
            foreach (Type type in noDomain)
            {
                sb.AppendLine($"\t{type.Name}");
            }

            IOrderedEnumerable<KeyValuePair<Type, int>> orderByDescending = typeCount.OrderByDescending(s => s.Value);

            sb.AppendLine("Entity Count: ");
            foreach (var kv in orderByDescending)
            {
                if (kv.Value == 1)
                {
                    continue;
                }

                sb.AppendLine($"\t{kv.Key.Name}: {kv.Value}");
            }

            return sb.ToString();
        }

        public void Dispose()
        {
            instance = null;
        }
    }
}
