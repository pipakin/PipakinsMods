using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Pipakin.GatheringMod.Config
{
    class GenericConfigVariable<T>
    {
        object backingStore;
        
        public T Value
        {
            get
            {
                return Traverse.Create(backingStore).Property("Value").GetValue<T>();
            }
        }

        public GenericConfigVariable(Assembly assembly, ConfigFile config, string id, string varName, T defaultValue, string configSection, string configDescription, bool localOnly)
        {
            if(assembly != null)
            {
                var method = assembly.GetType("ModConfigEnforcer.ConfigManager").GetMethods().First(x => x.Name =="RegisterModConfigVariable" && x.IsGenericMethod).MakeGenericMethod(typeof(T));
                backingStore = method.Invoke(null, new object[] { id, varName, defaultValue, configSection, configDescription, localOnly });
            } else {
                backingStore = config.Bind<T>(configSection, varName, defaultValue, configDescription);
            }
        }
    }
}
