using System.Collections.Generic;
using System.Linq;
using System;
using System.Collections;

public class CatalogDB {

    Dictionary<Type, object> catalogs = new Dictionary<Type, object>();

    public CatalogDB() { }

    public Catalog<T> GetCatalog<T>() where T : CatalogItem {
        if (!catalogs.ContainsKey(typeof(T)))
            catalogs.Add(typeof(T), new Catalog<T>());
        return catalogs[typeof(T)] as Catalog<T>;
    }

    public CatalogItem GetItemOfType(string id, Type type) {
        return (CatalogItem)typeof(CatalogDB).GetMethods()[2].MakeGenericMethod(type).Invoke(this, new object[] { id });
    }

    public CatalogItem GetItem<T>(string id) where T : CatalogItem {
        return GetCatalog<T>()[id];
    }

    public override string ToString() {
        return string.Format("CatalogDB:\n{0}", string.Concat((from catalog in catalogs.Values select (catalog.ToString() + "\n")).ToArray()));
    }

    /// <summary>
    /// Calls initialize on all items in all catalogs
    /// </summary>
    public void InitializeAll() {
        foreach (Type type in catalogs.Keys) {
            foreach (object item in (catalogs[type] as IEnumerable))
                (item as CatalogItem).Initialize();
        }
    }

}

