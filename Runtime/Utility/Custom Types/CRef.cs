
namespace Konfus.Utility.Custom_Types
{
    /// <summary>
    /// <para> Is a direct reference to data like in C
    /// , whatever happens to this ref happens to the original. </para>
    /// <param name="T"> Type of data being stored. </param>
    /// </summary>
    public class Ref<T> where T : struct
    {
        public T Value {get; set;}
    }
}
