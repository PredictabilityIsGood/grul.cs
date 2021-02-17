using System;
using System.Reflection;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Linq;

namespace grul
{
    public static class grul
    {
        public static string TrimDelimeter( string input, string delimeter ) {
            int delimeterPosition = input.IndexOf(delimeter);
            return input.Substring(0,delimeterPosition>=0?delimeterPosition:input.Length);
        }
        public static T CloneDeep<T>( this T data ){
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, data);
                ms.Position = 0;
                return (T) formatter.Deserialize(ms);
            }
        }
        public static Func<dynamic,List<Type>,List<dynamic>,List<Hop>,dynamic,dynamic> Logic( Func<dynamic,List<Type>,List<dynamic>,List<Hop>,dynamic,dynamic> aFunc ){
            return aFunc;	
        }
        public static bool CompareT( Type DefinedType , dynamic Data ){
            return DefinedType.ToString() == Data.GetType().ToString() ? true : false;
        }
        public static dynamic Pluck( dynamic data, List<dynamic> historicalLiteralPath, dynamic aValue=null){
            if(historicalLiteralPath.Count>1){
                return Pluck( GetEntry(data,historicalLiteralPath[0]).value, historicalLiteralPath.GetRange(1,historicalLiteralPath.Count-1), aValue );	
            }
            else if(historicalLiteralPath.Count == 0){
                if(aValue == null){
                    return data;
                }
                else{
                    data = aValue;
                    return data;
                }
            }
            else if(aValue == null){
                return GetEntry(data,historicalLiteralPath[0]).value;
            }
            else{
                GetEntry(data,historicalLiteralPath[0]).value = aValue;
                return GetEntry(data,historicalLiteralPath[0]).value;
            }
        }
        public static bool ListEquals( List<dynamic> listA, List<dynamic> listB ){
            if(listA.Count != listB.Count){
                return false;	
            }
            for(int i = listA.Count; i>0; i--){
                if(listA[i] == listB[i]){
                    return false;	
                }
            }
            return true;
        }
        public static dynamic PathExists( dynamic data, List<dynamic> bindPath, List<dynamic> historicalLiteralPath){
            bool isEqual = ListEquals( bindPath, historicalLiteralPath ) && bindPath.Count == historicalLiteralPath.Count;
            if(!isEqual){
                historicalLiteralPath.Add(bindPath[historicalLiteralPath.Count]);
            }
            try {
                if(Pluck(data, historicalLiteralPath)){
                    return isEqual ? isEqual : PathExists(data, bindPath, historicalLiteralPath);	
                }
                else{
                    return bindPath.GetRange(historicalLiteralPath.Count-1,bindPath.Count-(historicalLiteralPath.Count-1));
                }
            }
            catch(Exception e) {
                return bindPath.GetRange(historicalLiteralPath.Count-1,bindPath.Count-(historicalLiteralPath.Count-1));	
            }
        }
        public static bool IsPrimitive( dynamic data ){
            Type t = data.GetType();
            return
                t.IsValueType ||
                t.IsPrimitive ||
                (new Type[] { 
                    typeof(String),
                    typeof(Decimal),
                    typeof(DateTime),
                    typeof(DateTimeOffset),
                    typeof(TimeSpan),
                    typeof(Guid)
                }).Contains(t) ||
                Convert.GetTypeCode(t) != TypeCode.Object;
        }
        public static dynamic ExecuteLogic( dynamic logic, string location, int patternIndex, dynamic data, List<Type> historicalTypePath, List<dynamic> historicalLiteralPath, List<Hop> historicalObjectPath, dynamic initial){
            dynamic continueTraversal;
            if(logic.GetType()==typeof(List<dynamic>)){
                if(logic[patternIndex].GetType().BaseType.Name.ToString() == "Object"){
                    try{
                        continueTraversal = GetEntry(logic[patternIndex],location).value(data,historicalTypePath,historicalLiteralPath,historicalObjectPath,initial);
                    }
                    catch(Exception e){
                        continueTraversal = true;
                    }
                }
                else{
                    continueTraversal = logic[patternIndex](data,historicalTypePath,historicalLiteralPath,historicalObjectPath,initial);
                }
            }
            else if(logic.GetType().BaseType.Name.ToString() == "Object"){
                try{
                    continueTraversal = GetEntry(logic,location).value(data,historicalTypePath,historicalLiteralPath,historicalObjectPath,initial);
                }
                catch(Exception e){
                    continueTraversal = true;
                }
            }
            else{
                continueTraversal = logic(data,historicalTypePath,historicalLiteralPath,historicalObjectPath,initial);
            }
            return continueTraversal;
        }
        public static dynamic AtMeta( dynamic data, List<dynamic> metaPath, dynamic logic, int relativity = 0, List<Type> historicalTypePath = null, List<dynamic> historicalLiteralPath = null, List<MetaTemplate> curPathStates = null){
            return AtPattern(data,metaPath,logic,relativity, historicalTypePath, historicalLiteralPath, curPathStates, data, true);
        }
        public static dynamic AtPattern( dynamic data, List<dynamic> metaPath, dynamic logic, int relativity = 0, List<Type> historicalTypePath = null, List<dynamic> historicalLiteralPath = null,  List<MetaTemplate> curPathStates = null,  dynamic root = null, bool direct=false){
            historicalTypePath = historicalTypePath ?? new List<Type>();
            historicalLiteralPath = historicalLiteralPath ?? new List<dynamic>();
            root = root ?? data;
            
            Dictionary<dynamic,dynamic> matched = new Dictionary<dynamic,dynamic>();
            dynamic relativeData = null;
            MetaTemplate nextPathStateTemplate = new MetaTemplate(){
                matchCount = 0,
                hop = new List<Hop>()
            };
            List<MetaTemplate> nextPathStates = new List<MetaTemplate>();
            if(metaPath[0].GetType() == typeof(List<dynamic>)){
                if(curPathStates!=null){
                    nextPathStates = new List<MetaTemplate>();
                    for(var i=0; i < metaPath.Count; i++){
                        nextPathStates.Add(nextPathStateTemplate);
                    }
                }
            }
            else {
                metaPath = new List<dynamic>(){ metaPath };
                nextPathStates.Add(nextPathStateTemplate);
            }
            
            if(historicalLiteralPath.Count > 0) {
                bool matchExists = false;
                for(var i=0; i < metaPath.Count; i++){
                    nextPathStates[i] = new MetaTemplate(){
                        matchCount = CloneDeep(curPathStates[i].matchCount),
                        hop = curPathStates[i].hop
                    };

                    bool metaPathPrimitive = IsPrimitive(metaPath[i][nextPathStates[i].matchCount]);

                    bool typeCheck = false;
                    if(!metaPathPrimitive){
                        string metaPathStr = TrimDelimeter(((Type)metaPath[i][nextPathStates[i].matchCount]).Name,"`");
                        string historicalTypeStr = TrimDelimeter(((Type)historicalTypePath[historicalTypePath.Count-1]).Name,"`");
                        typeCheck = historicalTypeStr.IndexOf(metaPathStr) >= 0 ? true : false;
                    }
                    if( ( nextPathStates[i].matchCount < metaPath[i].Count && !metaPathPrimitive && typeCheck )  ||
                        ( nextPathStates[i].matchCount < metaPath[i].Count &&  metaPathPrimitive && historicalLiteralPath[historicalLiteralPath.Count-1] == metaPath[i][nextPathStates[i].matchCount] ) ){
                        nextPathStates[i].matchCount++;
                        matchExists = true;
                    }
                    else{
                        nextPathStates[i].matchCount = 0;
                    }
                    if( nextPathStates[i].matchCount == metaPath[i].Count ){
                        dynamic continueTraversal;
                        relativeData = relativity == 0 ? data : Pluck(root,historicalLiteralPath.GetRange(0,historicalLiteralPath.Count+relativity));
                        nextPathStates[i].hop = nextPathStates[i].hop.Concat(new List<Hop>(){ new Hop(){ data = relativeData } }).ToList();
                        
                        int frozenPathStateIndex = CloneDeep(i);
                        int frozenHopIndex =  nextPathStates[i].hop.Count - 1;
                        if(frozenHopIndex > 0){
                            nextPathStates[i].hop[frozenHopIndex].previous = () => {
                                return nextPathStates[frozenPathStateIndex].hop[frozenHopIndex-1];
                            };
                            nextPathStates[i].hop[frozenHopIndex].previous().next = () => {
                                return nextPathStates[frozenPathStateIndex].hop[frozenHopIndex];
                            };
                        }
                        continueTraversal = ExecuteLogic(logic,"head",i,relativeData,historicalTypePath,historicalLiteralPath, nextPathStates[i].hop,root);
                        matched[i] = relativeData;
                        if(continueTraversal == false){
                            return relativeData;	
                        }
                        nextPathStates[i].matchCount = 0;
                    }
                }
                if(direct && !matchExists){
                    return false;	
                }
            }
            
            if(data == null){
                return false;	
            }
            
            List<Type> nhtpath = historicalTypePath.GetRange(0,historicalTypePath.Count);
            nhtpath.Add(data.GetType());
            if(!IsPrimitive(data)){
                ((List<Entry>)GetEntries(data)).ForEach((Entry anEntry)=>{
                    List<dynamic> nhlpath = historicalLiteralPath.GetRange(0,historicalLiteralPath.Count);
                    nhlpath.Add(anEntry.key);
                    AtPattern( anEntry.value, metaPath, logic, relativity, nhtpath, nhlpath, nextPathStates, root, direct );
                });	
            }
            
            for(var i=0; i<metaPath.Count;i++){
                if( matched.ContainsKey(i) ){
                    dynamic continueTraversal = ExecuteLogic(logic,"tail",i,relativeData,historicalTypePath,historicalLiteralPath,nextPathStates[i].hop,root);
                    if(continueTraversal == false){
                        return relativeData;
                    }
                }
            }
            return data;
        }
        public static dynamic AtEvery( dynamic data, dynamic logic, List<Type> historicalTypePath = null, List<dynamic> historicalLiteralPath = null, List<Hop> historicalObjectPath = null,  dynamic root = null){
            historicalTypePath = historicalTypePath ?? new List<Type>();
            historicalLiteralPath = historicalLiteralPath ?? new List<dynamic>();
            root = root ?? data;

            dynamic continueTraversal = ExecuteLogic(new List<dynamic>(){ logic },"head",0,data,historicalTypePath,historicalLiteralPath,historicalObjectPath,root);
            if(continueTraversal == false){
                return data;
            }
            if(!IsPrimitive(data)){
                List<Type> nhtpath = historicalTypePath.GetRange(0,historicalTypePath.Count);
                nhtpath.Add(data.GetType());
                ((List<Entry>)GetEntries(data)).ForEach((Entry anEntry)=>{
                    List<dynamic> nhlpath = historicalLiteralPath.GetRange(0,historicalLiteralPath.Count);
                    nhlpath.Add(anEntry.key);
                    AtEvery( anEntry.value, logic, nhtpath, nhlpath, historicalObjectPath, root );
                });
            }
            continueTraversal = ExecuteLogic(new List<dynamic>(){ logic },"tail",0,data,historicalTypePath,historicalLiteralPath,historicalObjectPath,root);
            if(continueTraversal == false){
                return data;
            }
            return data;
        }
        public static List<Entry> GetEntries(dynamic data){
            
            List<Entry> entries = new List<Entry>();

            string TypeName = data.GetType().Name;
            if(TypeName.IndexOf("Dictionary") >= 0){
                foreach(KeyValuePair<string,dynamic> keyValue in data){
                    entries.Add(new Entry { key = keyValue.Key, value = keyValue.Value});
                }
            }
            else if(TypeName.IndexOf("List") == 0){
                for(var i=0;i<data.Count;i++){
                    entries.Add(new Entry { key = i , value = data[i]});
                }
            }
            else if(TypeName.IndexOf("Array") == 0){
                for(var i=0;i<data.Length;i++){
                    entries.Add(new Entry { key = i , value = data[i]});
                }
            }
            else{
                dynamic propertyNames = data.GetType().GetProperties();
                foreach(PropertyInfo prop in propertyNames){
                    if(prop.CanRead && prop.GetIndexParameters().Length==0){
                        entries.Add(new Entry { key = prop.Name, value = prop.GetValue(data) });
                    }
                }
            }
            
            return entries;
        }
        public static Entry GetEntry(dynamic data,dynamic key){
            dynamic property = data.GetType().GetProperty(key);
            return new Entry { key = key , value = property.GetValue(data) };
        }
    }
    public class Entry{
        public dynamic key {get;set;}
        public dynamic value {get;set;}
    }
    public class MetaTemplate{
        public int matchCount {get;set;}
        public List<Hop> hop {get;set;}
    }
    public class Hop{
        public dynamic data {get;set;}
        public Func<Hop> previous {get;set;}
        public Func<Hop> next {get;set;}
    }
}