JSON parsing and creation
===============

This folder contains the Active Events necessary to parse and create JSON. There are 2 Active Events in this project.

* [json2lambda] - Creates one or more lambda objects from one or more JSON snippets
* [lambda2json] - Creates a JSON snippet from one or more lambda objects.

Both of these Active Events can take expressions, leading to what you'd like to transform. Below you can see an example of 
using **[json2lambda]** to convert a JSON snippet to a lambda object.

```
json2lambda:@"{
  'name':'John Doe',
  'address':{
    'zip':5789,
    'str':'Dunbar Road'
  },
  'list':[
    57,
    77, {
      'foo':'bar'
    }
  ]
}"
```

The above will result in something like the following.

```
json2lambda
  result
    name:John Doe
    address
      zip:long:5789
      str:Dunbar Road
    list
      :long:57
      :long:77
      foo:bar
```

Notice, each JSON snippet you transform like this, will end up in a separate **[result]** node as illustrated above. Below you can see an example of 
using **[lambda2json]** to go the opposite way.

```
lambda2json
  name:John Doe
  address
    zip:long:5789
    str:Dunbar Road
  list
    :long:57
    :long:77
    foo:bar
```

Which of course results in the following.

```
lambda2json:@"{
  ""name"": ""John Doe"",
  ""address"": {
    ""zip"": 5789,
    ""str"": ""Dunbar Road""
  },
  ""list"": [
    57,
    77,
    {
      ""foo"": ""bar""
    }
  ]
}"
```

## Concerns

**Notice**; There's a mismatch between the structure of a JSON object and a lambda object. A JSON object is a simple key/value object, while
a lambda object is a key/value/children object. This mismatch implies that not everything that is possible to describe in lambda, is possible 
to describe with the same structure in JSON. For instance, if a lambda object has both a value and a children collection, the value must be
stored in JSON, together with its children properties. Below is an example of how this might look like if converting from lambda to JSON.

```
lambda2json
  foo:bar-value
    child-1:value-1
    child-2:value-2
```

Which will end up looking like the following.

```
lambda2json:@"{
  ""foo"": {
    ""__value"": ""bar-value"",
    ""child-1"": ""value-1"",
    ""child-2"": ""value-2""
  }
}"
```

Notice how the *"__value"* node above becomes a property of our _"foo"_. When you go the other way, from JSON to lambda, a *"__value"* JSON
property, will be automatically assumed to be the value of your lambda object.

There are also other difficulties, such as JSON not preserving any type information. This implies that if you convert from a lambda object having
an integer value to JSON, and then back again - During the conversion back to lambda, the JSON parser will assume your integer value
is actually a _long_ type. Hence, all integer values when parsed from JSON to lambda will be typed as _long_ types, and all floating point values 
will be typed as _double_.

This is an integral weakness with JSON, which is difficult if not impossible to solve, when going from JSON to lambda, since JSON doesn't in 
any ways preserve type information for its values.

Another weakness with JSON which doesn't apply to lambda, is that you can't have two properties with the same name. For instance, the following
will throw an exception during evaluation for you.

```
lambda2json
  foo:bar
  foo:other-bar
```

When the parser checks to see if it should create an array or a complex object, it will check the name of the first node, and if empty, it will
assume the caller wants to create an array. Hence, the above non-working code, could be changed into the following, and would then work perfectly.

```
lambda2json
  :bar
  :other-bar
```

Which will result in the following.

```
lambda2json:@"[
  ""bar"",
  ""other-bar""
]"
```

Alternatively something like the following.

```
lambda2json
  foo
    :bar
    :other-bar
```

Which would result in the following result.

```
lambda2json:@"{
  ""foo"": [
    ""bar"",
    ""other-bar""
  ]
}"
```

These weaknesses ignored, the JSON support in Phosphorus Five is in general terms quite strong, using Newtonsoft's JSON.Net library beneath its 
hoods - And should be able to create any JSON object you would want to create, and/or parse any JSON object you'd encounter out there.

Notice, we could of course have serialized the lambda structure directly, re-creating the entire hierarchy, to defeat the above problems.
However, since the **[lambda2json]** event will probably mostly be used to create JSON which is meant to interact with other services, I feel at the
moment, that being able to create a JSON structure, resembling the JSON you'd eventually would want to end up with, is more
important than being able to create a perfect serialization method for lambda objects as such. This makes it easy for you to create JSON structures,
which you for instance use as return values from web services and such, interacting with JSON clients - While at the same time, it makes 
it impossible to by the very definition of these mismatches to be able to serialize *all* lambda objects due to these structural deifferences, 
and you'll have to be careful when structuring your lambda objects, if you want them to be serialized to JSON.

Basically what this means, is that any **JSON object can be de-serialized to a lambda object, but the opposite is not necessarily true**.
In future versions, there might be created an override, which perfectly preserves the lambda object's structure, which would be possible by 
serializing the lambda object directly as is. However, since this would create a much more _"noisy"_ JSON structure, more difficult to understand for
clients consuming it - I have postponed this for future versions of Phosphorus Five, to focus on what I feel is of most importance at the moment.

**Rule of thumbs**

* You can create any JSON structure you wish by carefully structuring your lambda
* You can parse any JSON structure into a lambda
* You can _not necessarily_ transform all lambda objects into JSON
