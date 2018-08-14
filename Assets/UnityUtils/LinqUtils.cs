using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

public static class LinqUtils
{

    public static IEnumerable<T> Flatten<T>(IEnumerable<IEnumerable<T>> source)
    {
        foreach(IEnumerable<T> singleSource in source)
            foreach(T item in singleSource)
                yield return item;
    }

}

