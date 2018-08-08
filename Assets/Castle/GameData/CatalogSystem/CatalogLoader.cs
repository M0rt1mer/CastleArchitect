using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Collections;
using System.Globalization;
using UnityEngine;
using System.Linq;

public class CatalogLoader
{

	Dictionary<string,EntryInfo> loadTags = new Dictionary<string, EntryInfo>();
	Dictionary<Type,string> tagNames = new Dictionary<Type, string> ();
	HashSet<CatalogItem> referencedOnly = new HashSet<CatalogItem> ();
	CatalogDB db;

	public CatalogLoader( CatalogDB db, IEnumerable<Type> possibleTypes ){
		this.db = db;
		foreach( Type t in possibleTypes ){
			string name = t.Name;
			object[] attrs = t.GetCustomAttributes(typeof(CatalogEntryInfo),false);
			if( attrs.Length > 0 )
				name = ((CatalogEntryInfo)attrs[0]).name;
			EntryInfo info = new EntryInfo(t);
			List<EntryFieldInfo> efi = new List<EntryFieldInfo>();
			//Debug.Log( name );
			foreach( FieldInfo fld in t.GetFields() ){
				foreach( CatalogLoaded attr in fld.GetCustomAttributes( typeof(CatalogLoaded), false ).Cast<CatalogLoaded>() ){
					efi.Add( new EntryFieldInfo( fld, attr.name!=null?attr.name:fld.Name ) );
					//Debug.Log( "\t"+attr.name!=null?attr.name:fld.Name );
				}
			}
			info.fields = efi.ToArray();
			loadTags.Add( name, info );
			tagNames.Add( t, name );
		}

	}

	public void Load( params Stream[] streams  ){

		foreach (Stream str in streams) {
		
			XmlDocument catXml = new XmlDocument();
			catXml.Load( str );
			str.Close();
		
			LoadXml( catXml );
		}

	}

    public void Load(params TextReader[] streams) {

        foreach (TextReader str in streams) {

            XmlDocument catXml = new XmlDocument();
            catXml.Load(str);
            str.Close();

            LoadXml(catXml);
        }

    }

    public void Load(params XmlReader[] streams) {

        foreach (XmlReader str in streams) {

            XmlDocument catXml = new XmlDocument();
            catXml.Load(str);
            str.Close();

            LoadXml(catXml);
        }

    }

    public void Load( IEnumerable<string> datas ){
	
		foreach (string str in datas) {
			
			XmlDocument catXml = new XmlDocument();
			catXml.LoadXml(str);
			
			LoadXml( catXml );
		}



	}

	private void LoadXml( XmlDocument xml ){

		foreach (XmlNode node in xml.SelectSingleNode("Catalog").ChildNodes ) {
            if(!node.Name.Equals( "#comment" )) {
                if(loadTags.ContainsKey( node.Name ))
                    typeof( CatalogLoader ).GetMethod( "LoadEntry", BindingFlags.NonPublic | BindingFlags.Instance ).MakeGenericMethod( loadTags[node.Name].type ).Invoke( this, new object[] { node } );
                else
                    Debug.Log( "Unknown tag: " + node.Name );
            }
		}
		
	}

	private void LoadEntry<T>( XmlNode node ) where T:CatalogItem{

		string id = node.Attributes ["id"].Value.Trim ();
		//add to catalog
		T item = GetEntry<T> (id,false);
		//fill in data
		EntryInfo ei = loadTags [ tagNames[typeof(T)] ];

		foreach (EntryFieldInfo efi in ei.fields) {
			XmlNode nd = node.SelectSingleNode(efi.name);
			if( nd!=null ){
				efi.field.SetValue( item, LoadValue( efi.field.FieldType, nd ) );
			}
		}

	}

	private object LoadValue( Type type, XmlNode source ){
		//Debug.Log (type + ": " + source);
		if (tagNames.ContainsKey (type)) { //reference
			return GetType ().GetMethod ("GetEntry", BindingFlags.NonPublic | BindingFlags.Instance ).MakeGenericMethod (type).Invoke (this, new object[]{ source, true } );
		} else { //nonreference

			if( type == typeof(float)){
				return float.Parse( source.InnerText.Trim(), CultureInfo.InvariantCulture);
			}
			if( type == typeof(int)){
				return int.Parse( source.InnerText.Trim() );
			}
			if( type == typeof(double)){
				return double.Parse( source.InnerText.Trim(), CultureInfo.InvariantCulture);
			}
			if( type == typeof(bool)){
				return bool.Parse( source.InnerText.Trim() );
			}
			if (type == typeof(string) || type == typeof(String) ){
				return source.InnerText.Trim();
			}
			if( type.IsArray ){
				return GetType().GetMethod("LoadArray", BindingFlags.NonPublic | BindingFlags.Instance).MakeGenericMethod( type.GetElementType() ).Invoke( this, new object[]{source } );
			}
			if( type.IsValueType ){
				return GetType().GetMethod( "LoadStruct", BindingFlags.NonPublic | BindingFlags.Instance ).MakeGenericMethod( type.GetElementType() ).Invoke( this, new object[] { source } );
            }
            if(type.IsGenericType && type.GetGenericTypeDefinition() == typeof( Dictionary<,> ) && type.GetGenericArguments()[0] == typeof( string )) {
                return GetType().GetMethod( "LoadDictionary", BindingFlags.NonPublic | BindingFlags.Instance ).MakeGenericMethod( type.GetGenericArguments()[1] ).Invoke( this, new object[] { source } );
            }
		}
		return null;
	}

	private T[] LoadArray<T>( XmlNode source ){
        /*String[] split = source.Split (new char[]{';'},StringSplitOptions.RemoveEmptyEntries);
		T[] t = new T[split.Length];
		for (int i = 0; i<split.Length; i++)
			t[i] = (T)LoadValue (typeof(T), split [i]);
		return t;*/
        T[] t = new T[source.ChildNodes.Count];
        for(int i = 0; i < t.Length; i++)
            t[i] = (T)LoadValue( typeof( T ), source.ChildNodes[i] );
        return t;
	}

	private T LoadStruct<T>( XmlNode source ) where T:new(){
        /*String[] split = source.Trim('(',')').Split (',');
		object[] atrs = new object[split.Length];
		FieldInfo[] fld = type.GetFields ();
		if (split.Length != fld.Length) {
			Debug.Log ("Wrong struct format: " + source);
			return Activator.CreateInstance(type);
		}
		for (int i = 0; i<split.Length; i++) {
			atrs [i] = LoadValue (fld [i].FieldType, split [i]);
			Debug.Log ( fld[i].Name +" "+ split[i] );
		}
		return type.GetConstructor ( (from field in fld select field.FieldType).ToArray() ).Invoke (atrs);*/
        T str = new T();
        foreach(XmlNode node in source.ChildNodes) {
            FieldInfo fld = typeof( T ).GetField( node.Name );
            if(fld != null)
                fld.SetValue( str, LoadValue( fld.GetType(), node ) );
        }
        return str;
    }

    private Dictionary<string,T> LoadDictionary<T>( XmlNode node ) {
        //K is string
        Dictionary<string, T> dict = new Dictionary<string, T>();
        foreach(XmlNode nd in node.ChildNodes) {
            dict[nd.Name] = (T)LoadValue( typeof( T ), nd );
        }
        return null;
    }

	private T GetEntry<T>( string id, bool markAsReference ) where T:CatalogItem{
		T t = db.GetCatalog<T> () [id];
		if (t == null) {
			t = (T)typeof(T).GetConstructor (new Type[]{typeof(string)}).Invoke (new object[]{id});
			db.GetCatalog<T>()[id] = t;
			if (markAsReference)
				referencedOnly.Add (t);
		} else if(!markAsReference)
			referencedOnly.Remove(t);
		return t;
	}

	private struct EntryInfo{
		public Type type;
		public EntryFieldInfo[] fields;
		public EntryInfo (System.Type type){
			this.type = type;
			fields = null;
		}
	}

	private struct EntryFieldInfo{
		public FieldInfo field;
		public string name;
		public EntryFieldInfo (System.Reflection.FieldInfo field, string name){
			this.field = field;
			this.name = name;
		}
	}

}