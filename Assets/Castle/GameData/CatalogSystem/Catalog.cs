using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections;

public class Catalog<T> : IEnumerable<T> where T:CatalogItem{
	
	Dictionary<string,T> items;

	public Catalog (){
		items = new Dictionary<string, T> ();
	}

	public T this[string key]{
		get{
			if(items.ContainsKey(key))
				return items[key];
			else
				return null;
		}
		set{
			items[key] = value;
		}
	}

    public Dictionary<string,T>.ValueCollection.Enumerator GetEnumerator() {
        return items.Values.GetEnumerator();
    }

    public override string ToString (){
		return string.Format ("Catalog {0}: \n{1}", typeof(T).Name, string.Concat( (from item in items.Values select (item.ToString()+"\n") ).ToArray() )  );
	}

    IEnumerator IEnumerable.GetEnumerator() {
        return items.Values.GetEnumerator();
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator() {
        return items.Values.GetEnumerator();
    }
}


[System.AttributeUsage(System.AttributeTargets.Field)]
public class CatalogLoaded : System.Attribute {
	//whether this field should be loaded or not. If CatalogLoaded is not present, defaults to CatalogEntryInfo.loadAll
	public bool loaded = true;
	//uses the field name if NULL
	public String name = null;
	//whether there should be an error raised if the element is not present
	public bool mandatory = false;
	//for pointer-type attributes. If true, the value is treated as catalog entry an is loaded to according catalog. If false, it is treated as ID and is just searched in the catalog
	//public bool subCatalog = false;
}

[System.AttributeUsage(System.AttributeTargets.Class)]
public class CatalogEntryInfo : System.Attribute {
	public String name;
	public bool loadAll = false;
}
