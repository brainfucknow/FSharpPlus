(*** hide ***)
// This block of code is omitted in the generated HTML documentation. Use 
// it to define helpers that you do not want to show in the documentation.
#r @"../../src/FSharpPlus/bin/Release/net8.0/FSharpPlus.dll"

(**
Getting Started with FSharpPlus
================================

Welcome to FSharpPlus! This guide will help you get up and running quickly, 
taking you from zero to productive in no time.

## Installation

FSharpPlus is available as a NuGet package. Add it to your project:

### Using .NET CLI
```bash
dotnet add package FSharpPlus
```

### Using Package Manager Console
```
Install-Package FSharpPlus
```

### In F# Scripts
*)

(**
```f#
#r "nuget: FSharpPlus"
```
*)

open FSharpPlus

(**

## Your First Steps

FSharpPlus enhances F# in several ways. Let's explore them progressively, 
starting with the simplest and most immediately useful features.

### Level 1: Extension Functions (Immediate Value)

These are the easiest to adopt - they're just helpful functions on familiar types.
You can start using them right away without learning anything new.

#### String Extensions
*)

// Convenient string operations
let text = "  Hello World  "
let trimmed = String.trim text                    // "Hello World"
let lower = String.toLower "HELLO"                // "hello"
let replaced = String.replace "old" "new" "Good old days"  // "Good new days"

(**
#### Collection Extensions
*)

// Works on lists, arrays, and sequences
let numbers = [1; 2; 3; 4; 5]
let splitAt = List.splitAt 2 numbers              // ([1; 2], [3; 4; 5])
let intercalated = List.intersperse 0 [1; 2; 3]  // [1; 0; 2; 0; 3]
let chunked = List.chunkBy (fun x -> x % 2) [1; 2; 3; 4] // [(1, [1]); (0, [2]); (1, [3]); (0, [4])]

(**
#### Option and Result Extensions
*)

// Safer error handling
let tryParseInt x = Option.protect int x

// Result helpers
let okValue = Result.get (Ok 42)                  // 42
let withDefault = Result.defaultValue 0 (Error "oops")  // 0

(**

### Level 2: Generic Functions (More Power)

Once comfortable with extensions, try generic functions. These work across 
different types, reducing code duplication.

#### The `map` function
*)

// Works on lists
let stringList = map string [1; 2; 3]             // ["1"; "2"; "3"]

// Works on arrays  
let stringArray = map string [|1; 2; 3|]          // [|"1"; "2"; "3"|]

// Works on options
let mappedOption = map ((*) 2) (Some 21)          // Some 42

// Works on Results
let mappedResult = map ((*) 2) (Ok 21)            // Ok 42

(**
#### Generic operators for composition
*)

open FSharpPlus.Data

// The bind operator >>= (read as "bind")
let tryDivide x y = if y = 0 then None else Some (x / y)

let result1 = Some 100 >>= tryDivide 50 >>= tryDivide 2  // Some 1

// The Kleisli composition operator >=> (read as "fish")  
let safeDivision = tryDivide 100 >=> tryDivide 2
let result2 = safeDivision 5                      // Some 10

(**

### Level 3: Useful Types (Common Scenarios)

FSharpPlus provides types that solve common problems elegantly.

#### NonEmptyList - A list that's never empty
*)

// Create a non-empty list
let nel = NonEmptyList.create 1 [2; 3; 4]
// Guaranteed to have at least one element - no need to check!
let firstElement = NonEmptyList.head nel          // Always safe: 1

(**
#### Validation - Accumulate all errors
*)

// Unlike Result which stops at first error, Validation collects them all
type PersonRequest = { Name: string; Age: int; Email: string }

let validateName name =
    if String.IsNullOrWhiteSpace name 
    then Validation.error ["Name is required"]
    else Validation.ok name

let validateAge age =
    if age < 0 || age > 150
    then Validation.error ["Age must be between 0 and 150"]
    else Validation.ok age

let validateEmail email =
    if String.contains "@" email
    then Validation.ok email
    else Validation.error ["Email must contain @"]

// Using applicative style to combine validations
let validatePerson name age email =
    (fun n a e -> { Name = n; Age = a; Email = e })
    <!> validateName name
    <*> validateAge age
    <*> validateEmail email

(**
#### DList - Efficient list concatenation
*)

// DList is perfect when you need to build lists by concatenation
let dlist = DList.ofSeq [1; 2] ++ DList.ofSeq [3; 4] ++ DList.ofSeq [5; 6]
let resultList = DList.toList dlist               // [1; 2; 3; 4; 5; 6]

(**

### Level 4: Computation Expressions (Elegant Code)

Computation expressions make working with effects natural and readable.

#### The monad computation expression
*)

// Works with Option
let monadicOption = monad {
    let! x = Some 10
    let! y = Some 32
    return x + y
}  // Some 42

// Works with Result  
let monadicResult = monad {
    let! x = Ok 10
    let! y = Ok 32
    return x + y
}  // Ok 42

// Works with List (for combinations)
let combinations = monad.plus {
    let! x = [1; 2; 3]
    let! y = [10; 20]
    return x + y
}  // [11; 21; 12; 22; 13; 23]

(**

## Quick Reference: Common Tasks

### Working with Options
*)

// Create
let someValue = Some 42
let noValue = None

// Transform
let doubled = map ((*) 2) someValue               // Some 84

// Chain operations that might fail
let safeCalculation x = 
    Some x 
    >>= (fun n -> if n > 0 then Some n else None)
    >>= (fun n -> Some (n * 2))

(**
### Working with Results  
*)

// Create
let successResult = Ok 42
let errorResult = Error "Something went wrong"

// Transform
let resultDoubled = map ((*) 2) successResult     // Ok 84

// Handle errors
let getResultMessage result =
    Result.either 
        (fun x -> sprintf "Success: %d" x)
        (fun e -> sprintf "Error: %s" e)
        result
        
let handled = getResultMessage successResult      // "Success: 42"

(**
### Working with Collections
*)

// Generic operations work across List, Array, Seq
let squared = map (fun x -> x * x) [1; 2; 3; 4]

// Filter
let evens = filter (fun x -> x % 2 = 0) [1; 2; 3; 4; 5; 6]

// Fold
let sum = fold (+) 0 [1; 2; 3; 4; 5]

(**

## Next Steps

Now that you've seen the basics, here's how to continue your journey:

1. **[Learning Path](learning-path.html)** - A structured roadmap from beginner to advanced
2. **[Tutorial](tutorial.html)** - Deeper dive into FSharpPlus features
3. **[Abstractions](abstractions.html)** - Understanding the underlying concepts
4. **[API Reference](reference/index.html)** - Complete API documentation

## Getting Help

If you get stuck or have questions:

- Join the [Gitter chat](https://gitter.im/fsprojects/FSharpPlus)
- Ask on [Stack Overflow](https://stackoverflow.com/questions/tagged/f%23%2b) with the `f#+` tag
- Check the [API Reference](reference/index.html) for detailed documentation

Remember: Start small! You don't need to learn everything at once. Begin with 
the extension functions, then gradually adopt generic functions and types as you 
become comfortable.

Happy coding with FSharpPlus! 🚀
*)
