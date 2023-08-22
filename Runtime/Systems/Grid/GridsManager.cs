using Konfus.Utility.Custom_Types;
using UnityEngine;

namespace Konfus.Systems.ThreeDGrid
{
    [ExecuteInEditMode] 
    public class GridsManager : MonoBehaviour
    {
        [SerializeField]
        protected SerializableDict<string, Grid> grids;

        public T GetGrid<T>(string key) where T : Grid => grids[key] as T;
        public void AddGrid(string key, Grid grid) => grids.Add(key, grid);
        public void RemoveGrid(string key) => grids.Remove(key);
    }
    
}
