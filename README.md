![img](./imgs/gn_script_logo.png)

# Introduction

Ever since I started coding, I've dreamt of creating my own programming language. Now that I'm more experienced, I decided to make that dream a reality. The result is GN Script: a dynamically-typed, object-oriented language designed to be as intuitive as possible. When developing GN Script, I relied heavily on my gut instincts about how language elements should behave. For example, dividing a string by an integer breaks it into chunks, and multiplying an array by an integer repeats its elements.

In GN Script, you get:

- <b>Dynamic Typing:</b> Variables can change types freely, e.g., x = 12 can later become x = "test".
- <b>Duck Typing:</b> If it looks like a duck and quacks like a duck, it’s a duck.
- <b>Object-Oriented:</b> Supports classes (refboxes) with inheritance, abstract classes/methods, and two access modifiers (exposed and guarded).
- <b>Immutable Arrays and Strings:</b> Both arrays and strings are immutable; creating a new instance is necessary for any modification.
- <b>Reference Types:</b> Refboxes are reference types.
- <b>Looping Constructs:</b> Standard iteration capabilities.
- <b>Properties:</b> Built-in types have properties like length for strings and arrays, and methods like toArray for converting strings to arrays. Some properties take parameters, like :hasField("x") to check for a field in a refbox.
- <b>File I/O:</b> Read and write files, and execute scripts from files within the current interpreter instance.

GN Script is not just a language, it's an extension of my thought process, a tool that behaves just as I intuitively expect it to. I wrote it just for fun, to see how far my creativity and intuition could take me in building something unique.

<table>
<tr>
<th>Code (check amicable numbers)</th>
<th>rangeLib.txt</th>
</tr>
<tr>
<td>
  
```
import "rangeLib.txt"

function sumOfProperDivisors(n)
  total = 0
  range = rangeArray(1, n / 2 + 1)
  for i = 0; i < range:length; i = i + 1
    number = range[i]
    if n % number == 0
      total = total + number
    end
  end
return total

function areAmicableNumbers(a, b)
  return (sumOfProperDivisors(a) == b) && (sumOfProperDivisors(b) == a)

number1 = 220
number2 = 284

if areAmicableNumbers(number1, number2)
  print "The numbers " + number1 + " and " + number2 + " are amicable."
else
  print "The numbers " + number1 + " and " + number2 + " are not amicable."
end
```
</td>
<td>

```
function rangeArray(startNum, endNum)
  result = []
  for i = startNum; i < endNum; i = i + 1
    result = result + i
  end
return result
```

</td>
</tr>
</table>

## Examples

As a first step, I recommend running the examples located in the `examples` folder. They will help you take your first step in learning GN script.

| File                    | Description                                                                                             |
| ----------------------- | ------------------------------------------------------------------------------------------------------- |
| GCD.txt                 | Calculates the greatest common divisor (GCD) of two numbers.                                            |
| bubble_sort.txt         | Implements the bubble sort algorithm to sort a list of numbers.                                         |
| count_occurrences.txt   | Counts the occurrences of elements in a list and displays how many times each element appears.          |
| fibonacci_iterative.txt | Generates the Fibonacci sequence iteratively.                                                           |
| fibonacci_recursive.txt | Generates the Fibonacci sequence recursively.                                                           |
| pascals_triangle.txt    | Prints Pascal's Triangle up to a given number of rows.                                                  |
| perfect_number.txt      | Checks if a number is a perfect number, i.e., a natural number that is equal to the sum of its divisors |
| prime_numbers.txt       | Prints all prime numbers below a number specified by the user.                                          |

## Documentation

<ol>
<li><a href="#variables">Variables and Scopes</a></li>
<li><a href="#variable-types">Variable Types</a></li>
<ul>
<li><a href="#variable-types-int-string">Int and String</a></li>
<li><a href="#variable-types-Array">Array</a></li>
<li><a href="#variable-types-RefBox">RefBox</a></li>
<li><a href="#variable-types-Void">Void</a></li>
</ul>
<li><a href="#state-interpreter">Displaying the State of the Interpreter</a></li>
<li><a href="#conditional-statements">Conditional Statements</a></li>
<li><a href="#loops">Loops</a></li>
<li><a href="#arrays">Arrays</a></li>
<li><a href="#functions">Functions</a></li>
<li><a href="#refboxes">Refboxes</a></li>
<ul>
<li><a href="#refboxes-Inheritance">Inheritance</a></li>
<li><a href="#refboxes-Abstract-RefBoxes">Abstract RefBoxes</a></li>
<li><a href="#refboxes-Const-RefBoxes">Const RefBoxes</a></li>
<li><a href="#refboxes-Passing-Data">Passing Data</a></li>
</ul>
<li><a href="#extensions">Extensions</a></li>
<ul>
<li><a href="#extensions-common">Common</a></li>
<li><a href="#extensions-array">Array</a></li>
<li><a href="#extensions-string">String</a></li>
<li><a href="#extensions-refbox">Refbox</a></li>
<li><a href="#extensions-int">Int</a></li>
</ul>
<li><a href="#operators">Operators</a></li>
<ul>
<li><a href="#operators-int-and-int">Int And Int</a></li>
<li><a href="#operators-int-and-string">Int And String</a></li>
<li><a href="#operators-string-and-int">String And Int</a></li>
<li><a href="#operators-array-and-int">Array And Int</a></li>
<li><a href="#operators-int-and-array">Int And Array</a></li>
<li><a href="#operators-string-and-array">String And Array</a></li>
<li><a href="#operators-array-and-string">Array And String</a></li>
<li><a href="#operators-array-and-array">Array And Array</a></li>
<li><a href="#operators-string-and-string">String And String</a></li>
</ul>
<li><a href="#interpreter-instructions">Interpreter Instructions</a></li>
<li><a href="#Importing-Scripts">Importing Scripts</a></li>
</ol>

<h3 id="variables">Variables and Scopes</h3>

Variables are created by specifying their name, followed by an equals sign =, and then their value. When creating variables, it is important to consider that they respect scopes. This means that if you create a variable inside a function or loop, it will not be accessible outside of them. To make a variable accessible in other scopes, you must first declare it in a higher scope, such as the body of a function. Then, if you create a variable with the same name inside a loop (i.e., in a nested scope), you will change the value of the variable in the function body.

```
x = 2
x = "test"
```

Thus, it can be said that if a variable with a given name exists in a parent scope, its value can be changed in nested scopes. The same principle applies to accessing the variable's value. It is first searched for in the current scope, then in the parent scope, and so on until it is found, or an exception is thrown if it is not found.

The behavior of parameters and variables declared within a for loop is different. In these cases, references will terminate at those variables, and no operation on them will affect the higher scope. For example, if a variable `i` exists in the parent scope and `i` is declared as an iterator in a for loop, adding a number to 'i' within the loop will only modify the iterator variable, leaving the variable in the parent scope unchanged.

```
function scope_test()
  i = 20
  for i = 0; i < 5 ; i = i + 1
    print "inside loop: " + i
    i = i + 1
  end
  print "i: " + i
return void
```

Output:

```
inside loop: 0
inside loop: 2
inside loop: 4
i: 20
```

<h3 id="variable-types">Variable Types</h3>

<h4 id="variable-types-int-string">Int and String</h4>

`Int` and `String` are self-explanatory value types. A `String` cannot change its value once created; it is necessary to create a new `String` to modify the content.

<h4 id="variable-types-Array">Array</h4>

`Array` is a value type and cannot change its value directly. To modify an element in an array, you need to create a new array with the desired changes.

<h4 id="variable-types-RefBox">RefBox</h4>

`RefBox` is a reference type that can store variables and functions. In other languages, this is often referred to as a class. Variables and functions within a `RefBox` can have access modifiers.

<h4 id="variable-types-Void">Void</h4>

If we assign the result of a function returning `void` to a variable, the variable will be of type `void`. This type is mostly useless but was necessary for the consistency of the language.

Variable names can contains letters, digits and underscore, but they cannot start with digit.

More information about arrays and `RefBox` can be found in their respective sections.

<h3 id="state-interpreter">Displaying the State of the Interpreter</h3>

To display the contents of a variable, you can use the `print` or `printInline` instructions. The former adds a newline character, while the latter does not.

```
print "This will be followed by a new line"
printInline "This will not be followed by a new line"
```

Another way is to use the interpreter instruction `dump`, which will display all variables, functions, and RefBoxes.

```
> dump
[Variables]
  Scope level: 0
  {a: 12} [Int]
  {b: [1, 2, 3]} [Array]

[Functions]
  bubbleSort <- {arr}

[RefBoxes]
  my_refbox : {[Guarded] x, [Guarded] my_func <- (a)}
```

<h3 id="conditional-statements">Conditional Statements</h3>

Conditional statements are declared using the keyword `if` followed by a condition. Similar to the C language, 0 means `false` and numbers greater than 0 mean `true`. The statement must end with the keyword `end`.

```
if a < 5
  print "less than " + 5
end
```

It is possible to add an `else` block, which will be executed if the condition is not met.

```
if a < 5
  print "less than " + 5
else
  print "greater than or equal to " + 5
end
```

There is no else if statement. To achieve this effect, you need to place another `if` inside the `else` block.

```
if a < 5
  print "less than " + 5
else
  if a == 5
    print "equals to " + 5
  else
    print "greater than " + 5
  end
end
```

<h3 id="loops">Loops</h3>

There are two types of loops available: `for` and `while`.

To declare a for loop, write the keyword `for`, declare an iterator, specify the condition, and the iterator increment. The body of the loop must end with the keyword `end`.

```
for i = 0; i < 10; i = i + 1
  print i
end
```

To declare a while loop, write the keyword `while` followed by a condition. The loop declaration must end with the keyword `end`.

```
a = 0
while a < 5
  print a
  a = a + 1
end
```

<h3 id="arrays">Arrays</h3>

As mentioned earlier, arrays are a value type, they are also immutable. Consequently, every change to the array's value requires creating a new array, which is done using extensions. Extensions are a very important concept in this language and are covered in a separate section. But returning to arrays, they are declared as follows:

```
a = [1, 2, 3, "my array"]
```

Arrays can store any type.

Access to array elements is done using square brackets [].

```
> a = [1, 2, 3, "my array", [6, 7, [33]]]

> print a[0]

1

> print a[a:length - 1][2][0]

33

> print [1, 2, 3][2]

3
```

<h3 id="functions">Functions</h3>

To declare a function, use the keyword `function` followed by its name and parentheses. Parameters can be included within these parentheses. Every function must end with the keyword return followed by a value or the keyword void (or wuwei; alias for void). A function can return different types or not return anything, so you don't need to worry about the return type of data, as it will be determined at the time the function is called.

```
function myFunction(a, b)
  if a < b
    print b + " is bigger than " + a
    return void
  else
    if a > b
      return a
    end
  end
return "they are equal"
```

Output:

```
> myFunction(1, 2)

"2 is bigger than 1"

> print myFunction(2, 1)

1

> print myFunction(2, 2)

2
```

Even though the language is dynamically typed, sometimes we need to ensure that a variable is of the correct type. For this purpose, we can use the extension `:type`, which returns the type as a string, allowing us to compare it. Below is an example function that calculates the sum of an array. If the input is not an array, an exception is thrown.

```
function sumArray(arr)
  if arr:type:tolower <> "array"
    throw "Expected array"
  end
  sum = 0
  for i = 0; i < arr:length; i = i + 1
    sum = sum + arr[i]
  end
return sum
```

<h3 id="refboxes">Refboxes</h3>

RefBoxes are reference data types that can contain fields and functions with one of two access modifiers: `exposed` and `guarded`. The first one, `exposed`, is the default (i.e., if you don't specify anything, it will be exposed) and allows access to the RefBox instance from outside (equivalent to public in other languages). The second one, `guarded`, means that access is restricted to within the RefBox itself and its inheriting RefBoxes.

The declaration starts with the keyword `refbox`, followed by the name and the body of the RefBox, which ends with the keyword end. We can create refbox instance using `create` keyword.

```
refbox myRefbox
  guarded x = 1
  exposed y = 2
  function printX()
    print x
  return void
end
```

Instance:

```
> myInstance = create myRefbox

> myInstance.printX()

1

> print myInstance.x

Error: Cannot access guarded field

> myInstance.y = 100
```

<h4 id="refboxes-Inheritance">Inheritance</h4>

Every RefBox can be inherited. If the child RefBox already has a function or field with the same name as the base RefBox, the elements of the RefBox will be overridden.

```
refbox base
  x = 10
  function fun()
    print "I'm from base refbox"
    return void
  end

refbox child : base
  y = "dddd"
  function fun()
    print "I'm from child refbox"
  return void
end
```

Dump:

```
> dump
[Variables]
  No variables to display.

[Functions]
  No functions to display.

[RefBoxes]
  base : {[Exposed] x, [Exposed] fun <- ()}
  child : {[Exposed] y, [Exposed] x, [Exposed] fun <- ()} [base: base]
```

Overwritten function:

```
> child = create child

> child.fun()

I'm from child refbox
```

<h4 id="refboxes-Abstract-RefBoxes">Abstract RefBoxes</h4>

A RefBox can be `abstract`, which can be achieved by adding the keyword `abstract` after the refbox keyword during declaration. Such RefBoxes can define `abstract` functions that must be overridden in the inheriting refbox. Note that the definition of an `abstract` function does not end with the keyword `return` or `end` and does not contain a function body.

```
refbox abstract abstr
  f = 122
  abstract function test(x)
end
```

Inheritance:

```
> refbox noOverwritten : abstr
  end

Error: Refbox cannot have not overrided functions: test

> refbox overwritten : abstr
  function test(x)
  return 123
end
```

<h4 id="refboxes-Const-RefBoxes">Const RefBoxes</h4>

RefBoxes can be `const`, meaning that any subsequent attempt to declare a RefBox with the same name will fail. By default, like other data types, re-declaring a RefBox would overwrite the previous one. Therefore, the `const` keyword can be useful when writing scripts intended to be imported into other programs, to prevent the programmer from accidentally overwriting the definition in their program.

```
refbox const myConstRef
  f = 1234
  function test()
    print "You can't redefine me"
  return void
end
```

Redefining:

```
> refbox myConstRef
  end

Error: Ref box 'myConstRef' is const, cannot create ref box definition with the same name
```

<h4 id="refboxes-Passing-Data">Passing Data</h4>

Since RefBoxes are reference types, they can be used to pass data not only via the `return` keyword but also as parameters.

```
refbox keyValuePair
  key = 0
  value = 0
end

function findValue(kv)
  arr = [["a", 97], ["b", 98], ["c", 99], ["d", 100], ["e", 101], ["f", 102]]
  for i = 0; i < arr:length; i = i + 1
    if arr[i][0] == kv.key
      kv.value = arr[i][1]
      return void
    end
  end
return void
```

Usage:

```
> kv = create keyValuePair

> kv.key = "e"

> findValue(kv)

> print kv.value

101
```

<h3 id="extensions">Extensions</h3>

Extensions are special methods implemented directly in the language that can be called on variables or directly on values. Extensions are case-sensitive, so they must be written in all lowercase. Below is a list of all extensions along with the types they can be called on.

<h4 id="extensions-common">Common</h4>

| Extension | Input          | Result   | Description            |
| --------- | -------------- | -------- | ---------------------- |
| Type      | [1,2,3,4]:type | "Array"  | Returns type as string |
|           | 100:type       | "Int"    |                        |
|           | myRef:type     | "RefBox" |                        |
|           | "sss"          | "String" |                        |

<h4 id="extensions-array">Array</h4>

| Extension | Input                                                              | Result                             | Description                                                                                                                                                                                                                |
| --------- | ------------------------------------------------------------------ | ---------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Length    | [1,2,3,4]:length                                                   | 4                                  | Length of the array                                                                                                                                                                                                        |
| Reverse   | [1,2,3,4]:reverse                                                  | [4,3,2,1]                          | Create reversed array                                                                                                                                                                                                      |
| ToString  | [1,2,3]:tostring [1,2,3]:tostring("-")                             | "123" "1-2-3"                      | It concatenates array elements, using an empty string by default, but a custom delimiter can be specified                                                                                                                  |
| RemoveAt  | [1,2,3]:removeat(2)                                                | [1,2]                              | It removes the element at the specified index, but only in a single dimension                                                                                                                                              |
| AddAt     | [1,2,3]:addat(1,"f")                                               | [1,"f",2,3]                        | It inserts an element at the specified index, but only in a single dimension                                                                                                                                               |
| Append    | [1,2,3]:append("f")                                                | [1,2,3,"f"]                        | It adds an element to the end of the array                                                                                                                                                                                 |
| Prepend   | [1,2,3]:prepend("f")                                               | ["f",1,2,3]                        | It adds an element to the beginning of an array                                                                                                                                                                            |
| ReplaceAt | [1,2,3]:replaceat(1, "g") [1,2,[5,6,["h",2]]]:replaceat(2,2,0,"g") | [1,"g",3] [1, 2, [5, 6, ["g", 2]]] | It replaces the element at the specified index. The last argument is the value to replace the element with, and the preceding arguments are the indices for each dimension. This function works on multidimensional arrays |
| Has       | [1,2,3]:has("f") ["e","f","g"]:has("f")                            | 0 1                                | Returns 1 if the array contains the specified element, otherwise returns 0                                                                                                                                                 |

<h4 id="extensions-string">String</h4>

| Extension       | Input                              | Result               | Description                                                      |
| --------------- | ---------------------------------- | -------------------- | ---------------------------------------------------------------- |
| ToLower         | "ABC":tolower                      | "abc"                | Converts to lowercase                                            |
| ToUpper         | "abc":toupper                      | "ABC"                | Converts to uppercase                                            |
| Reverse         | "abc":reverse                      | "cba"                | Reverses the string                                              |
| ToArray         | "abc":toarray                      | ["a","b","c"]        | Converts string to array                                         |
| Length          | "abc":length                       | 3                    | Returns length                                                   |
| Split           | "abcbdbe":split("b") "a b c":split | ["a", "c", "d", "e"] | Splits a string, the default separator is a space                |
| ReplaceAt       | "abcd":replaceat(1, "a")           | "aacd"               | Replaces character at a specified index with value               |
| ToInt           | "123":toint                        | 123                  | Parse string to int                                              |
| CanConvertToInt | "123":canconverttoint              | 1                    | Returns 1 when the string can be parsed into int or 0 otherwise. |

<h4 id="extensions-refbox">Refbox</h4>

```
refbox test1
  x = 1
  function test(x)
    print x
  return void
end

refbox test2
  x = 2
  function test(x)
    print x + " 2"
  return void
end

t1 = create test1
```

| Extension          | Input                          | Result | Description                                                                       |
| ------------------ | ------------------------------ | ------ | --------------------------------------------------------------------------------- |
| IsInstanceOf       | t1:isinstanceof("test2")       | 1      | Checks if it is an instance of the specified refbox, using duck typing            |
| HasField           | t1:hasfield("x")               | 1      | Checks if the instance has a field with the given name                            |
| HasFunction        | t1:hasfunction("test", 1)      | 1      | Checks if the instance has a function with the given name and number of arguments |
| ReflectionSetField | t1:reflectionsetfield("x", 44) |        | Allows setting the value of a field with a 'guarded' modifier using reflection    |

<h4 id="extensions-int">Int</h4>

| Extension | Input       | Result | Description                   |
| --------- | ----------- | ------ | ----------------------------- |
| ToString  | 12:tostring | "12"   | Converts number to the string |

<h3 id="operators">Operators</h3>

<h4 id="operators-int-and-int">Int And Int</h4>

| Operator | first argument | second argument | result | Description                                                                                 |
| -------- | -------------- | --------------- | ------ | ------------------------------------------------------------------------------------------- |
| +        | 3              | 5               | 8      | Adds two integers.                                                                          |
| -        | 8              | 5               | 3      | Subtracts the second integer from the first.                                                |
| \*       | 4              | 3               | 12     | Multiplies two integers.                                                                    |
| /        | 10             | 2               | 5      | Divides the first integer by the second.                                                    |
| %        | 10             | 3               | 1      | Returns the remainder of the division of the first integer by the second.                   |
| &&       | 1              | 0               | 0      | Logical AND: returns 1 if both integers are non-zero, otherwise returns 0.                  |
| \|\|     | 0              | 2               | 1      | Logical OR: returns 1 if at least one of the integers is non-zero, otherwise returns 0.     |
| ^        | 2              | 3               | 8      | Raises the first integer to the power of the second.                                        |
| >        | 5              | 3               | 1      | Returns 1 if the first integer is greater than the second, otherwise returns 0.             |
| >=       | 5              | 5               | 1      | Returns 1 if the first integer is greater than or equal to the second, otherwise returns 0. |
| <        | 3              | 5               | 1      | Returns 1 if the first integer is less than the second, otherwise returns 0.                |
| <=       | 3              | 5               | 1      | Returns 1 if the first integer is less than or equal to the second, otherwise returns 0.    |
| ==       | 5              | 5               | 1      | Returns 1 if the two integers are equal, otherwise returns 0.                               |
| <>       | 5              | 3               | 1      | Returns 1 if the two integers are not equal, otherwise returns 0.                           |

<h4 id="operators-int-and-string">Int And String</h4>

| Operator | first argument (int) | second argument (string) | result      | Description                                                                             |
| -------- | -------------------- | ------------------------ | ----------- | --------------------------------------------------------------------------------------- |
| +        | 3                    | "test"                   | "3test"     | Concatenates the integer and the string.                                                |
| \*       | 3                    | "abc"                    | "abcabcabc" | Repeats the string a specified number of times.                                         |
| >        | 5                    | "hello"                  | 0           | Returns 1 if the integer is greater than the length of the string, otherwise returns 0. |
| ==       | 5                    | "hello"                  | 1           | Returns 1 if the integer is equal to the length of the string, otherwise returns 0.     |
| !=       | 3                    | "hello"                  | 1           | Returns 1 if the integer is not equal to the length of the string, otherwise returns 0. |

<h4 id="operators-string-and-int">String And Int</h4>

| Operator | first argument (string) | second argument (int) | result      | Description                                                                                      |
| -------- | ----------------------- | --------------------- | ----------- | ------------------------------------------------------------------------------------------------ |
| +        | "test"                  | 3                     | "test3"     | Concatenates the string and the integer.                                                         |
| -        | "teststring"            | 6                     | "test"      | Removes the last n characters from the string.                                                   |
| \*       | "abc"                   | 3                     | "abcabcabc" | Repeats the string a specified number of times.                                                  |
| /        | "abcdefgh"              | 2                     | "abcd"      | Truncates the string to the length obtained by dividing its length by the integer.               |
| <        | "hello"                 | 10                    | 1           | Returns 1 if the length of the string is less than the integer, otherwise returns 0.             |
| <=       | "hello"                 | 5                     | 1           | Returns 1 if the length of the string is less than or equal to the integer, otherwise returns 0. |
| ==       | "hello"                 | 5                     | 1           | Returns 1 if the length of the string is equal to the integer, otherwise returns 0.              |
| !=       | "hello"                 | 3                     | 1           | Returns 1 if the length of the string is not equal to the integer, otherwise returns 0.          |

<h4 id="operators-array-and-int">Array And Int</h4>

| Operator | first argument (array) | second argument (int) | result                | Description                                                                                     |
| -------- | ---------------------- | --------------------- | --------------------- | ----------------------------------------------------------------------------------------------- |
| +        | [1, 2, 3]              | 4                     | [1, 2, 3, 4]          | Adds an integer to the end of the array.                                                        |
| -        | [1, 2, 3, 4, 5]        | 2                     | [1, 2, 3]             | Removes the last n elements from the array.                                                     |
| /        | [1, 2, 3, 4, 5]        | 2                     | [[1, 2], [3, 4], [5]] | Splits the array into chunks of the specified size.                                             |
| \*       | [1, 2]                 | 3                     | [1, 2, 1, 2, 1, 2]    | Repeats the array a specified number of times.                                                  |
| <        | [1, 2, 3]              | 5                     | 1                     | Returns 1 if the length of the array is less than the integer, otherwise returns 0.             |
| <=       | [1, 2, 3]              | 3                     | 1                     | Returns 1 if the length of the array is less than or equal to the integer, otherwise returns 0. |
| ==       | [1, 2, 3]              | 3                     | 1                     | Returns 1 if the length of the array is equal to the integer, otherwise returns 0.              |
| !=       | [1, 2, 3]              | 4                     | 1                     | Returns 1 if the length of the array is not equal to the integer, otherwise returns 0.          |

<h4 id="operators-int-and-array">Int And Array</h4>

| Operator | first argument (int) | second argument (array) | result       | Description                                     |
| -------- | -------------------- | ----------------------- | ------------ | ----------------------------------------------- |
| +        | 5                    | [1, 2, 3]               | [5, 1, 2, 3] | Adds the integer to the beginning of the array. |

<h4 id="operators-string-and-array">String And Array</h4>

| Operator | first argument (string) | second argument (array) | result             | Description                                    |
| -------- | ----------------------- | ----------------------- | ------------------ | ---------------------------------------------- |
| +        | "hello"                 | [1, 2, 3]               | ["hello", 1, 2, 3] | Adds the string to the beginning of the array. |

<h4 id="operators-array-and-string">Array And String</h4>

| Operator | first argument (array) | second argument (string) | result            | Description                              |
| -------- | ---------------------- | ------------------------ | ----------------- | ---------------------------------------- |
| +        | [1, 2, 3]              | "test"                   | [1, 2, 3, "test"] | Adds the string to the end of the array. |

<h4 id="operators-array-and-array">Array And Array</h4>

| Operator | first argument (array) | second argument (array) | result                    | Description                                                                                                     |
| -------- | ---------------------- | ----------------------- | ------------------------- | --------------------------------------------------------------------------------------------------------------- |
| +        | [1, 2, 3]              | [4, 5]                  | [1, 2, 3, 4, 5]           | Concatenates the second array to the end of the first array.                                                    |
| -        | [1, 2, 3, 4, 5]        | [4, 5]                  | [1, 2, 3]                 | Removes the elements in the second array from the end of the first array, if they match.                        |
| /        | [1, 2, 1, 2, 1, 2]     | [1, 2]                  | 3                         | Returns the count of how many the second array fits within the first array.                                     |
| \*       | [1, 2]                 | [3, 4]                  | [[1, 1, 1], [2, 2, 2, 2]] | Multiplies the first array by the integers specified in the second array, arrays have to have same length       |
| <        | [1, 2, 3]              | [1, 2, 3, 4]            | 1                         | Returns 1 if the length of the first array is less than the length of the second array, otherwise returns 0.    |
| >        | [1, 2, 3, 4]           | [1, 2, 3]               | 1                         | Returns 1 if the length of the first array is greater than the length of the second array, otherwise returns 0. |
| ==       | [1, 2, 3]              | [1, 2, 3]               | 1                         | Returns 1 if the first array is equal to the second array, otherwise returns 0.                                 |
| !=       | [1, 2, 3]              | [4, 5, 6]               | 1                         | Returns 1 if the first array is not equal to the second array, otherwise returns 0.                             |

<h4 id="operators-string-and-string">String And String</h4>

| Operator | first argument (string) | second argument (string) | result       | Description                                                                                                         |
| -------- | ----------------------- | ------------------------ | ------------ | ------------------------------------------------------------------------------------------------------------------- |
| +        | "hello"                 | "world"                  | "helloworld" | Concatenates the second string to the end of the first string.                                                      |
| -        | "hello world"           | " world"                 | "hello"      | Removes the trailing occurrences of the second string from the end of the first string.                             |
| /        | "hellohellohello"       | "hello"                  | 3            | Counts the number of occurrences of the second string within the first string.                                      |
| >        | "b"                     | "a"                      | 1            | Returns 1 if the first string is lexicographically greater than the second string, otherwise returns 0.             |
| >=       | "a"                     | "a"                      | 1            | Returns 1 if the first string is lexicographically greater than or equal to the second string, otherwise returns 0. |
| <        | "a"                     | "b"                      | 1            | Returns 1 if the first string is lexicographically less than the second string, otherwise returns 0.                |
| <=       | "a"                     | "b"                      | 1            | Returns 1 if the first string is lexicographically less than or equal to the second string, otherwise returns 0.    |
| ==       | "hello"                 | "hello"                  | 1            | Returns 1 if the first string is equal to the second string, otherwise returns 0.                                   |
| !=       | "hello"                 | "world"                  | 1            | Returns 1 if the first string is not equal to the second string, otherwise returns 0.                               |

<h3 id="interpreter-instructions">Interpreter Instructions</h3>

- **READ**: Reads a script from a file and executes it.

- **READCLS**: Resets the current state of the interpreter, then reads and executes a script from a file. This is useful when you want to start with a clean state before executing a new script.

- **CLS**: Resets the interpreter state, clearing variables, functions, refboxes, and other elements. Use this to completely clear the current context.

- **DUMP**: Outputs the current state of the interpreter, including all variables (with their types), declared functions, and refboxes. This is useful for debugging and inspecting the current interpreter state.

- **EXIT**: Closes the interpreter, terminating the session. This should be used when you are done with all operations and want to end the interpreter's execution.

<h3 id="Importing-Scripts">Importing Scripts</h3>

To load and execute another script while maintaining the current state of the interpreter, you should use the `import` instruction followed by the file path. The file path provided to `import` does not have to be a direct string; it can also be a variable or a function that returns the file path. This flexibility allows for dynamic script loading based on varying conditions or inputs.

It is important to note that `import` is an instruction within the language itself, not an interpreter command like `READ`. This means that `import` is handled as part of the script execution and can be used to modularize and manage code effectively.

<h3 id="Code-Formatting">Code Formatting</h3>

The language does not rely on indentation for code structure, unlike Python. This means that all code can be written on a single line without affecting its interpretation. For example, the following code:

```
arr = [1, 2, 3, 4, 5, 6, 7] for i = 0; i < arr:length; i = i + 1 print arr[i] end
```

is interpreted the same way as:

```
arr = [1, 2, 3, 4, 5, 6, 7]
for i = 0; i < arr:length; i = i + 1
  print arr[i]
end
```

However, the interpreter requires that there are no blank lines between instructions. A double line break is treated as a command to execute the written code fragment. This rule does not apply to scripts imported using the `import` instruction.

<h3 id="Future-of-the-Project">Future of the Project</h3>

Looking ahead, this language has promising potential for integration into other programs. Much like VBA (Visual Basic for Applications) is used within Excel to automate tasks and extend functionality, this language could serve as an embedded scripting tool in various applications.
