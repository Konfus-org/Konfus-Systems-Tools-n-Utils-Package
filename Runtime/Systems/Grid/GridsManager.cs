using Konfus.Utility.Serialization;
using UnityEngine;

namespace Konfus.Systems.Grid
{
    [ExecuteInEditMode] public class GridsManager : MonoBehaviour
    {
        [SerializeField]
        protected SerializableDict<string, GridBase> grids;

        public T GetGrid<T>(string key) where T : GridBase => grids[key] as T;
        public void AddGrid(string key, GridBase grid) => grids.Add(key, grid);
        public void RemoveGrid(string key) => grids.Remove(key);
    }
    
}
