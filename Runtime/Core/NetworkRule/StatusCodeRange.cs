using System;
using System.Linq;

namespace PhEngine.Network
{
    [Serializable]
    public class StatusCodeRange
    {
        public int start;
        public int end;

        public bool IsBetweenRange(int value)
        {
            return start <= value && value <= end;
        }
        
        public static bool IsBetweenRange(int value , params StatusCodeRange[] ranges)
        {
            return ranges.Any(range => range.IsBetweenRange(value));
        }
    }
}