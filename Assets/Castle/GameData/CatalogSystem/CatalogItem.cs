using System.Collections.Generic;
using System;
using System.Reflection;
using UnityEngine;

public class CatalogItem{
	
	public readonly string id;
//	static Dictionary<Type,Dictionary<int,CatalogItem>> data = new Dictionary<Type, Dictionary<int, CatalogItem>>();

	protected CatalogItem (string id){
		this.id = id;
	}
	
	/*public static Dictionary<int,CatalogItem> GetCatalog(Type type){
		if( !data.ContainsKey(type) )
			data.Add( type, (Dictionary<int,CatalogItem>)Activator.CreateInstance( typeof(Dictionary<,>).MakeGenericType(new Type[]{typeof(int),type} ) ) );
		return data [type];
	}
	public static Dictionary<int,T> GetCatalog<T>() where T:CatalogItem{
		if( !data.ContainsKey(typeof(T)) )
			data.Add( typeof(T), (Dictionary<int,CatalogItem>)new Dictionary<int,T>() );
	}*/

	public override bool Equals (object obj)
	{
		if (obj == null)
			return false;
		if (ReferenceEquals (this, obj))
			return true;
		if (obj.GetType () != GetType() )
			return false;
		CatalogItem other = (CatalogItem)obj;
		return id == other.id;
	}

	public override int GetHashCode ()
	{
		unchecked {
			return GetType().GetHashCode() ^ (id != null ? id.GetHashCode () : 0);
		}
	}
	
	public override string ToString ()
	{
		return string.Format ("[CatalogItem: id={0}]", id);
	}

    /// <summary>
    /// Called after all catalogs are loaded
    /// </summary>
    public virtual void Initialize() { }
	
}
