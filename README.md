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
<li><a href="#state-interpreter">Displaying the State of the Interpreter</a></li>
<li><a href="#conditional-statements">Conditional Statements</a></li>
<li><a href="#loops">Loops</a></li>
<li><a href="#arrays">Arrays</a></li>
<li><a href="#functions">Functions</a></li>
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

### Variable Types

#### Int and String

`Int` and `String` are self-explanatory value types. A `String` cannot change its value once created; it is necessary to create a new `String` to modify the content.

#### Array

`Array` is a value type and cannot change its value directly. To modify an element in an array, you need to create a new array with the desired changes.

#### RefBox

`RefBox` is a reference type that can store variables and functions. In other languages, this is often referred to as a class. Variables and functions within a `RefBox` can have access modifiers.

#### Void

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

| Operator | first argument      | second operator | snd operator type | result                | Description                                                                                                                               |
| -------- | ------------------- | --------------- | ----------------- | --------------------- | ----------------------------------------------------------------------------------------------------------------------------------------- |
| +        | [1,2,3,4]           | 5               | any type          | [1,2,3,4,5]           | Adds an element to the end of the array, or to the beginning if the array is the second argument.                                         |
| -        | [1,2,3,4]           | 2               | int               | [1,2]                 | Shortens the array by the number of elements specified.                                                                                   |
| \*       | [1,2]               | 3               | int               | [1, 2, 1, 2, 1, 2]    | Repeats the array a specified number of times                                                                                             |
| /        | [1,2,3,4,5]         | 2               | int               | [[1, 2], [3, 4], [5]] | Splits the array into chunks                                                                                                              |
| /        | [1,2,3,1,2,3,1,2,3] | [1,2,3]         | array             | 3                     | Returns the number of times the second array fits into the first array. If the first array is not a multiple of the second, it returns 0. |

<h3 id="functions">Functions</h3>

To declare a function, use the keyword `function` followed by its name and parentheses. Parameters can be included within these parentheses. Every function must end with the keyword return followed by a value or the keyword void (or wuwei; alias for void). A function can return different types or not return anything, so you don't need to worry about the return type of data, as it will be determined at the time the function is called.

```
function myFunction(a, b)
  if a < b
    print b + " is bigger than " + a
    return void
  else
    return b
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
