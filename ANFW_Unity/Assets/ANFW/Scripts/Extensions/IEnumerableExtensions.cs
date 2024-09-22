using System.Collections.Generic;
using System.Linq;

namespace ANFW {
    namespace Extensions {
        public static class IEnumerableExtensions
        {
            public static bool IsNullOrEmpty<T>(this IEnumerable<T> self) {
                return self == null || self.Count() == 0;
            }
        }
    }
}
