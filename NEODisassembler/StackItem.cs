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
        public bool directValue = false;
        public string name;
        public Type type;
        public List<StackItem> arr;
        public bool flag;
        public byte[] byteArray;
        //public Map map;
        public int integer;

        public string getName()
        {
            if (directValue)
            {
                switch (type)
                {
                    case Type.array:
                        return arr.ToString();
                        break;
                    case Type.map:
                        
                        break;
                    case Type.integer:
                        return integer+"";
                        break;
                    case Type.boolen:
                        return flag ? "TRUE" : "FALSE";
                        break;
                    case Type.struc:
                        break;
                    case Type.bytearray:
                        return byteArray.ToString();
                        break;
                }
                return "ERROR";
            }
            else
                return name;

        }


    }

}
