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
