using System;
using System.Collections.Generic;
using System.Reflection;

namespace Konfus.Systems.Node_Graph
{
    public static class AppDomainExtension
    {
        public static IEnumerable<Type> GetAllTypes(this AppDomain domain)
        {
            foreach (Assembly assembly in domain.GetAssemblies())
            {
                Type[] types = { };

                try
                {
                    types = assembly.GetTypes();
                }
                catch
                {
                    //just ignore it ...
                }

                foreach (Type type in types)
                    yield return type;
            }
        }
    }
}