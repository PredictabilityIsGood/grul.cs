# grul.cs
A library for chaining generic recursive functions as lambda utilities in C#

## Starting Code Examples
```
dynamic anotherObject = new Dictionary<string,dynamic>(){
	{ "key1" , new { test2depth = 1 } },
	{ "key2" , "value" },
	{ "key3" , new List<int>(){ 1,2,3 } }
};

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
```
