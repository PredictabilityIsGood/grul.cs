# grul.cs
A library for chaining generic recursive functions as lambda utilities in C# (.net core)

This project doesn't have a parked space within nuget at the moment.

#### C# Start
1) Add the compiled dll to your project references
2) Add using statement within project
```csharp
using grul
```

#### Prerequisites
Compiled dll within project uses .net 5. Compile within your desired versioning.

#### Traverse, mutate, flatten multidimensional data
```csharp
List<int> flattened = new List<int>();
List<List<List<int>>> matrix = new List<List<List<int>>>() {
	new List<List<int>>(){
		new List<int>(){ 5,6 },
		new List<int>(){ 7,8 }
	}
};
//using explicit direct access
AtMeta(matrix,new List<dynamic>(){ typeof(List<List<List<int>>>), typeof(List<List<int>>) , typeof(List<int>) },
	new { 
		head = Logic((dynamic data, List<Type> htp, List<dynamic> hlp, List<Hop> hop, dynamic root)=>{
			flattened.Add(data);
			return true; // to continue traversal
		})
	}
);
Console.WriteLine(flattened);
flattened = new List<int>();
//using implicit direct access
AtMeta(matrix,new List<dynamic>(){ typeof(List<dynamic>), typeof(List<dynamic>) , typeof(List<int>) },
	new { 
		head = Logic((dynamic data, List<Type> htp, List<dynamic> hlp, List<Hop> hop, dynamic root)=>{
			flattened.Add(data);
			return true; // to continue traversal
		})
	}
);
Console.WriteLine(flattened);
flattened = new List<int>();
//using pattern based access
AtPattern(matrix,new List<dynamic>(){ typeof(int) },
	new { 
		head = Logic((dynamic data, List<Type> htp, List<dynamic> hlp, List<Hop> hop, dynamic root)=>{
			flattened.Add(data);
			return true; // to continue traversal
		})
	}
);
Console.WriteLine(flattened);
```

#### Mixin type and literal path values at any time
```csharp
dynamic data = new List<dynamic>(){
	new { name="Ryan", age=28 },
	new { name="Sarah", age=29 }
};
AtMeta(
	data,
	new List<dynamic>(){typeof(List<dynamic>), "age"},
	new { 
		head = Logic((dynamic data, List<Type> htp, List<dynamic> hlp, List<Hop> hop, dynamic root)=>{
			Console.WriteLine(data);
			return true; // to continue traversal
		})
	}
);
```

#### Locate patterns within data sets
```csharp
dynamic anObject = new { test = "value", anotherOne = new { secondPath = "value2" } };
//Get anonymous class instance properties at any position within the tree
AtPattern(
	anObject,
	new List<dynamic> { (new {}).GetType() }, //anonymous objects do not allow typeof(dynamic). Therefore we 
	new { 
		head = Logic((dynamic data, List<Type> htp, List<dynamic> hlp, List<Hop> hop, dynamic root)=>{
			Console.WriteLine(data);
			return true; // to continue traversal
		})
	}
);

dynamic anotherObject = new Dictionary<string,dynamic>(){
	{ "key1" , new { test2depth = 1 } },
	{ "key2" , "value" },
	{ "key3" , new List<int>(){ 1,2,3 } }
};
//Get List<int> class instance properties at any position within the tree
AtPattern(
	anotherObject,
	new List<dynamic>(){ typeof(List<int>) }, 
	new {
		head = Logic((dynamic data, List<Type> htp, List<dynamic> hlp, List<Hop> hop, dynamic root)=>{
			Console.WriteLine(data);
			return true; // to continue traversal
		})
	}
);
```

#### Traverse custom or generic class instances
```csharp
dynamic anotherObject = new Dictionary<string,dynamic>(){
	{ "key1" , new { test2depth = 1 } },
	{ "key2" , "value" },
	{ "key3" , new List<int>(){ 1,2,3 } }
};
//Traverse multi-class instance tree for data at every traversal
AtEvery(anotherObject,
	new { 
		head = Logic((dynamic data, List<Type> htp, List<dynamic> hlp, List<Hop> hop, dynamic root)=>{
			Console.WriteLine(data);
			return true; // to continue traversal
		})
	}
);


List<Entry> customClassTraversal = new List<Entry> {
  new Entry{
	  key = "testing",
	  value = "traversal"
  }  
};

//Get property within custom class name "Entry" within the tree
AtPattern(
	customClassTraversal,
	new List<dynamic>(){ typeof(Entry) },
	new {
		head = Logic((dynamic data, List<Type> htp, List<dynamic> hlp, List<Hop> hop, dynamic root)=>{
			Console.WriteLine(data);
			return true; // to continue traversal
		})
	}
);
/* Example Entry Class
	public class Entry{
        public dynamic key {get;set;}
        public dynamic value {get;set;}
    }
*/
```
#### Planned Future Changes
* shallowest pattern searches at arbitrary depth ( atShallowestPattern )
* deepest patterns searches at arbitrary depth ( atDeepestPattern )
* normal recursive tree traversal with halting capabilities ( atEvery ) 
* define patterns with object templates ( atMatching )
* retrieve primitives ( atEnds )
* Hierarchical grouping with declarative templates
* Segment and prune multi-dimensional recursive patterns