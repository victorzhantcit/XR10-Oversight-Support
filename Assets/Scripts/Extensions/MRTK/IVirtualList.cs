using System.Collections.Generic;
using Unity.Extensions;

namespace MRTK.Extensions
{
    public interface IVirtualList
    {
        public List<KeyValue> ToVirtualList();
    }
}
