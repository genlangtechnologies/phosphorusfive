Dynamically creating Active Events in p5.lambda
===============

In Phosphorus Five, you can create static Active Events in C#, by adding the *ActiveEvent* attribute to your C# methods. See
an example of this [here](/samples/p5.active-event-sample-plugin/). You can also use any p5.lambda object as an "anonymous function". 
In addition, you can also easily execute Hyperlambda files, the same way you'd invoke a function or method, passing in arguments, 
and returning values, just like you would with a normal function. The latter could be accomplished using e.g. *[sys42.utilities.execute-lambda-file]*,
if you have [System42](https://github.com/polterguy/system42) installed.

However, there is also the possibility of dynamically creating globally accessible Active Events, through the *[create-event]*
Active Event. This Active Event, allows you to modify the underlaying Active Event dictionary directly, by either modifying existing, 
adding new, or deleting existing entries within it. Almost like you would do with any other dictionary/hash-table object.
Imagine the following code.

```
create-event:foo
  eval-x:x:/+
  return:Hello there {0}
    :x:/../*/_arg?value
foo:Thomas
```

When executed, the *[foo]* invocation above, will look like this.

```
/* ... other code ... */
foo:Hello there Thomas
```

Notice the "Hello there Thomas" value of the *[foo]* node above. This was the return value, from the Active Event *[foo]* we created.

When you create a dynamic Active Event, this event will be accessible for any p5.lambda object in your application pool, for the life 
span of your application pool. This means that if something resets your server, somehow, forcing the web-server application pool process
to restart, for some reasons (reboot of server or recycling your app pool for instance), you need to re-create your dynamic Active Events,
if you wish for them to still be acccessible.

In System42, this is easily done, by creating a "startup" file, which is executed during startup somehow. Either by putting your *[create-event]*
invocations inside a Hyperlambda file, inside of your "/system42/startup/" folder, or by putting your event creational logic, inside your _app's_
"startup.hl" file, inside of your "/system42/your-app/" folder. The latter is the preferred way, since it creates better encapsulation and cohesion of
your app.

This dynamic trait of p5.lambda, allows you to dynamically create Active Events, during runtime of your application, without requiring a 
recompilation, or anything similar for your code. It also allows you to "install" new apps in your system, without requiring a reboot of
the web-server process, etc.

Besides from that they're globally accessible, there is no real difference between a "normal lambda object", and a dynamically created
Active Event - They behave exactly the same way in all regards. When you invoke a dynamic Active Event, you are given a "shallow copy" of your
event's lambda object, which is evaluated, as if it was a normal lambda object. Making it possible to pass in arguments, and return nodes and 
values, as if it was a normal lambda object, evaluated using the *[eval]* Active Event.

To understand dynamically created Active Events, means first understanding the [eval Active Event](/plugins/p5.lambda#eval-the-heart-of-p5lambda/)
Active Event, since they obey by the same rules.

## Deleting events

The *[delete-event]* Active Event, allows you to completely delete a dynamic Active Event. You can either provide an expression, leading to multiple
names, or a constant, deleting only one event.

However, if you invoke *[create-event]* with no lambda object as its children, you will also delete any existing events with the given name.
The *[delete-event]* allows you to delete multiple events at the same time though, which the *[create-event]*, without a lambda object does _not_.
Besides, it is probably better to be more "clear" in your usage of vocabulary, and explicitly use the Active Event *[delete-event]*, to communicate
your intent more precisely.

## Stateful Active Events

Sometimes, it might be necessary to create a "stateful" Active Event. Meaning, an Active Event, that somehow is able to remember state, across
multiple invocations. The way you'd do this in most other OOP programming languages, is by creating an instance data field, or something similar,
and keep the object reference alive. However, in Phosphorus Five, no such thing exists. In fact, P5 doesn't even have OO any mechanism at all!

However, if you have a node, who's value is of type node, then its value will actually be remembered across multiple invocations of your event. 
That's why we said "shallow copy" above. When you invoke an Active Event, then you get a copy of its lambda to evaluate. However, since all _values_ 
are copied by reference, and not cloned, this means that each invocation will act upon the same value object. For the .Net types, this has no 
consequence, since they're all immutable. However, for the Node class, which is the underlaying structure of a p5.lambda object, this has side effects, 
which you can benefit from, to create "stateful events".

Simply because the copy of the Active Event you are evaluating, will have a "shallow copy" of its values, allowing you to _share_ node references,
inside of values of Active Events, across multiple invocations to the same event.

Consider this Active Event.

```
create-event:foo
  _static:node:"count:int:0"
  set:x:/../*/_static/#?value
    +:x:/../*/_static/#?value
      _:int:1
  eval-x:x:/+
  return:Lambda invocation count was; {0}
    :x:/../*/_static/#?value
foo
foo
foo
```

If you now evaluate the following Hyperlambda, you will clearly see your *[foo]* Active Event is able to remember its "invocation count", 
across multiple invocations. This is because the `_static:node:"count:int:0"` node's value will be shared for all invocations to your event.

Try to run this code.

```
foo
foo
foo
```

Notice, this feature, might require you to synchronize access to the p5.lambda that is accessing your nodes, to avoid race-conditions and similar. This is because
the value of our *[_static]* node above, is actually a shared resource. Synchronizing acccess to the *[_static]* node above, can easily be done by wrapping your 
p5.lambda in a *[lock]* block, which is documented in the "p5.threading" project. An example is given below.

```
create-event:foo
  _static:node:"count:int:0"
  lock:lock.foo
    set:x:/../*/_static/#?value
      +:x:/../*/_static/#?value
        _:int:1
    eval-x:x:/+
    return:Lambda invocation count was; {0}
      :x:/../*/_static/#?value
```

For the record, the above "Ninja Trick", is something you should be _careful_ with, since stateful Active Events, in most cases, is _not_ 
considered "good architecture". Besides, synchronizing access to shared resources, is often quite expensive, in terms of scalability. However, 
for those rare occassions, where it makes sense, it _really_ make sense. A good example is for instance caching.

Hint, if you re-create the above *[foo]* Active Event, you will reset the "hit count". Do you understand why ...?

## Listing all Active Events in your system

If you wish to see a list of all Active events in your system, this can easily be done using the *[vocabulary]* Active Event. Try it out in your
"system42/executor", and see how it returns a list of all Active Events in your system. Both those dynamically created, and those whom are statically
declared in C#. Example given below.

```
vocabulary
```

After evaluating the above code, you will get a list of nodes, where the name of the node says either "static" or "dynamic", where "dynamic" are your
dynamically created Active Events, vice versa. If you wish to exclude all "static" Active Events, this can easily be done with an additional *[set]*
invocation, like the following code illustrates.

```
vocabulary
set:x:/-/*/static
```

The *[vocabulary]* event has a protected alias, called *[.vocabulary]* (of course), which will also return all protected events, but (of course) must be
raised from C#.

Also the *[create-event]* and *[delete-event]* have similar protected alias versions.

