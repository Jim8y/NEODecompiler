using System.Collections.Generic;
namespace NEODisassembler
{
    public enum Type
    {
        array,
        map,
        integer,
        boolen,
        struc,
        bytearray
    }
    
    public class StackItem 
    {
        public string name;
        public Type type;
        public List<StackItem> arr;
        public bool flag;
        public byte[] byteArray;
        //public Map map;
        public int integer;
    }

}
