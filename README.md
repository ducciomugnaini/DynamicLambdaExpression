# DynamicLambdaExpression

## A Simple Lambda Expression Query

Just about every application requires some form of data access and query capability. Using the data classes and data set illustrated in Listing 1 and Listing 2 respectively, you can employ the following query to find all people with the first name of John:

``` c#
var result = _people.Where(person => person.FirstName
 == "John").ToList();
```
Lambda expressions, like is the one illustrated above, are very powerful. There is also some inflexibility, although you can easily vary the value you use to compare to the FirstName property. What if you want to vary the property? That is, as it turns out, a bit more complicated.

In the next section, I’ll convert the query listed above to be dynamic so that it can be built and compiled at run time.
Creating Your First Dynamic Lambda Expression

The key to understanding how to create dynamic lambda expressions rests with understanding the Expressions Class, which is part of the System.Linq.Expressions Namespace. The first step is to create a parameter expression that allows you to validate and compile against a known type. In this case, the type is in the Person and Address Classes:

``` c#
var parameterExpression =
 Expression.Parameter(Type.GetType("ExpressionTreeTests.
Person"), "person");
```

Once you have you parameter expression, you can begin to build the additional components. The following code creates an expression constant:

``` c#
var constant = Expression.Constant("John");
```

The expression constant has to be compared to some property. The code in the next snippet creates a property expression.

``` c#
var property = Expression.Property(parameterExpression,
 "FirstName");
```

Note that the parameter expression created earlier is used to validate the property. If the property name passed as a string isn’t found for the specified type, an error is thrown.

Next, you need to bring the expression constant and property together. In this case, you wish to test whether or not a property is equal to a string constant. The following code brings those two expressions together:

``` c#
var expression = Expression.Equal(property, constant);
```

The expression variable’s string representation is:

``` c#
{(person.FirstName == "John")}
```

Once an expression has been created, you can create a lambda expression. The next code snippet accomplishes that task:

``` c#
var lambda = Expression.Lambda<Func<Person,
 bool>>(expression, parameterExpression);
```

In this case, the lambda represents a delegate function that accepts a Person argument and returns a Boolean value to indicate whether the specified Person object meets the query criteria. In this case, the query criterion is whether the FirstName == "John". The following illustrates the string representation of the dynamic lambda expression:

``` c#
{person => (person.FirstName == "John")}
```

This is identical to the lambda expression in the simple lambda expression query listed above. The only difference is that instead of embedding the definition in the code, you’ve built it dynamically. The last step before using the dynamic lambda expression is to compile it:

``` c#
var compiledLambda = lambda.Compile();
```

Now that the lambda is compiled, you can use it in a query:

``` c#
var result = _people.Where(compiledLambda).ToList();
```

That’s it. That’s all there is to building a simple dynamic lambda expression. What if you have compound criteria? Let’s tackle that next. 

## From string to lambda with Roslyn

One of the little known gems of Roslyn is that as part of its Scripting API, it ships with an excellent expression evaluator. And this is what we will be using – through the Roslyn scripting packages from Nuget – to solve our problem in almost literally a single line of code.

Of course if you follow this blog, or my Twitter, or my Github, you probably know that I’m deeply involved in the C# scripting community, so if you have any questions about that area I’m always happy to help.

The Nuget package to reference is called Microsoft.CodeAnalysis.CSharp.Scripting.

Once you have it referenced in the project, you can use the C# scripting engine to evaluate our delegate, represented as a raw string, into a real instance of Func lambda that we require to filter our albums. The scripting APIs of Roslyn exposes a static method CSharpScript.EvaluateAsync which is perfect for our use case. We can pass in a string expression and have it compiled and evaluated on the fly, with the full power of the C# compiler. No manual parsing, no worrying about edge cases.

The usage is shown below.

``` c#
var discountFilter = "album => album.Quantity > 0";
var options = ScriptOptions.Default.AddReferences(typeof(Album).Assembly);

Func<Album, bool> discountFilterExpression = await CSharpScript.EvaluateAsync<Func<Album, bool>>(discountFilter, options);

var discountedAlbums = albums.Where(discountFilterExpression);
//hooray now we have discountedAlbums!
```

# References

[MS Documentation](https://docs.microsoft.com/it-it/dotnet/csharp/programming-guide/concepts/expression-trees/how-to-use-expression-trees-to-build-dynamic-queries)

[Simple Dyamic Expression](https://www.codemag.com/article/1607041/Simplest-Thing-Possible-Dynamic-Lambda-Expressions)

[Dynamic Expression From String](https://www.strathweb.com/2018/01/easy-way-to-create-a-c-lambda-expression-from-a-string-with-roslyn/)

