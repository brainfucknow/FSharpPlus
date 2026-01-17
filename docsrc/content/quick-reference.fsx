(*** hide ***)
// This block of code is omitted in the generated HTML documentation. Use 
// it to define helpers that you do not want to show in the documentation.
#r @"../../src/FSharpPlus/bin/Release/net8.0/FSharpPlus.dll"

(**
Quick Reference / Cheat Sheet
==============================

A quick lookup guide for common FSharpPlus operations.

*)

(**
```f#
#r "nuget: FSharpPlus"
```
*)

open FSharpPlus
open FSharpPlus.Data

(**

## String Operations

| Operation | Example | Result |
|-----------|---------|--------|
| Trim | `String.trim "  hello  "` | `"hello"` |
| To Lower | `String.toLower "HELLO"` | `"hello"` |
| To Upper | `String.toUpper "hello"` | `"HELLO"` |
| Replace | `String.replace "old" "new" "old text"` | `"new text"` |
| Split | `String.split [|' '|] "hello world"` | `[|"hello"; "world"|]` |
| Contains | `String.contains "el" "hello"` | `true` |
| Starts With | `String.startsWith "he" "hello"` | `true` |
| Ends With | `String.endsWith "lo" "hello"` | `true` |
| Is Null or Whitespace | `String.IsNullOrWhiteSpace "  "` | `true` |

---

## Option Operations

*)

// Create
let some = Some 42
let none = None

// Transform
let doubled = map ((*) 2) some                    // Some 84

// Chain (bind)
let chained = some >>= (fun x -> Some (x + 10))   // Some 52

// Get with default
let value = Option.defaultValue 0 none            // 0

// Try operations
let parsed = Option.tryParse<int> "42"            // Some 42
let head = List.tryHead [1; 2; 3]                 // Some 1

// Convert
let result = some <?> "Error message"             // Ok 42

(**

## Result Operations

*)

// Create
let success = Ok 42
let failure = Error "Something went wrong"

// Transform
let resultDoubled = map ((*) 2) success           // Ok 84

// Chain (bind)
let resultChained = 
    success >>= (fun x -> Ok (x + 10))            // Ok 52

// Handle errors
let handled = Result.either id (fun _ -> 0) failure  // 0

// Try operations
let resultParsed = Result.tryParse<int> "42"      // Ok 42
let protected = Result.protect int "42"           // Ok 42

// Get value
let value2 = Result.get success                   // 42 (throws on Error)
let defaulted = Result.defaultValue 0 failure     // 0

(**

## Validation Operations

*)

// Create
let valid = Validation.ok 42
let invalid = Validation.error ["Error message"]

// Combine validations (applicative)
let combined = 
    (fun x y -> x + y)
    <!> Validation.ok 10
    <*> Validation.ok 32                          // Success 42

// Collect all errors
let allErrors =
    (fun x y z -> (x, y, z))
    <!> Validation.error ["Error 1"]
    <*> Validation.error ["Error 2"]  
    <*> Validation.error ["Error 3"]
    // Failure ["Error 1"; "Error 2"; "Error 3"]

(**

## List/Collection Operations

*)

// Split at index
let split = List.splitAt 2 [1; 2; 3; 4; 5]        // ([1; 2], [3; 4; 5])

// Intersperse
let interspersed = List.intersperse 0 [1; 2; 3]   // [1; 0; 2; 0; 3]

// Chunk by function
let chunked = List.chunkBy (fun x -> x % 2) [1; 2; 3; 4]

// Try head/tail
let maybeHead = List.tryHead [1; 2; 3]            // Some 1
let maybeTail = List.tryTail [1; 2; 3]            // Some [2; 3]

(**

## Generic Functions

### Map
*)

map ((*) 2) [1; 2; 3]                             // [2; 4; 6]
map ((*) 2) (Some 21)                             // Some 42
map ((*) 2) (Ok 21)                               // Ok 42
map ((*) 2) [|1; 2; 3|]                           // [|2; 4; 6|]

(**
### Filter
*)

filter ((>) 5) [1; 2; 3; 4; 5; 6]                 // [1; 2; 3; 4]

(**
### Fold
*)

fold (+) 0 [1; 2; 3; 4; 5]                        // 15

(**

## Operators

### Bind (>>=)
*)

Some 10 >>= (fun x -> Some (x * 2))               // Some 20
Ok 10 >>= (fun x -> Ok (x * 2))                   // Ok 20

(**
### Kleisli Composition (>=>)
*)

let f x = Some (x + 1)
let g x = Some (x * 2)
let h = f >=> g
h 5                                               // Some 12

(**
### Applicative (<*>)
*)

Some (+) <*> Some 2 <*> Some 3                    // Some 5

(**
### Map operator (<!>)
*)

((*) 2) <!> Some 21                               // Some 42

(**
### Alternative (<|>)
*)

None <|> Some 42 <|> Some 100                     // Some 42

(**

## NonEmptyList

*)

// Create
let nel = NonEmptyList.create 1 [2; 3; 4]

// Head (always safe!)
let head2 = NonEmptyList.head nel                 // 1

// Tail
let tail = NonEmptyList.tail nel                  // [2; 3; 4]

// Map
let mapped = NonEmptyList.map ((*) 2) nel

// Concatenate
let combined2 = nel ++ NonEmptyList.singleton 5

(**

## Computation Expressions

### monad
*)

let monadicOption = monad {
    let! x = Some 10
    let! y = Some 32
    return x + y
}  // Some 42

let monadicResult = monad {
    let! x = Ok 10
    let! y = Ok 32
    return x + y
}  // Ok 42

(**
### monad.plus (for multiple results)
*)

let combinations = monad.plus {
    let! x = [1; 2; 3]
    let! y = [10; 20]
    return x + y
}  // [11; 21; 12; 22; 13; 23]

(**
### applicative (for parallel operations)
*)

let parallel = applicative {
    let! x = async { return 10 }
    let! y = async { return 32 }
    return x + y
}

(**

## Lenses

*)

open FSharpPlus.Lens

// Read
let value3 = ("hello", 42) ^. _2                  // 42

// Write
let updated = setl _2 100 ("hello", 42)           // ("hello", 100)

// Compose
let nested = ("a", ("b", 42))
let value4 = nested ^. (_2 << _2)                 // 42
let updated2 = setl (_2 << _2) 100 nested         // ("a", ("b", 100))

(**

## Type Conversions

*)

// Option to Result
let optToRes = Some 42 <?> "Error"                // Ok 42

// Result to Option  
let resToOpt = Result.toOption (Ok 42)            // Some 42

// Validation to Result
let valToRes = Validation.toResult valid          // Ok value

// List to Array
let arr = List.toArray [1; 2; 3]

// Seq conversions
let fromSeq = List.ofSeq (seq [1; 2; 3])
let toSeq = List.toSeq [1; 2; 3]

(**

## Common Patterns Quick Reference

### Chain optional operations
*)

let chainExample =
    Some "42"
    >>= Option.tryParse
    >>= (fun x -> if x > 0 then Some x else None)

(**
### Validate with multiple errors
*)

let validateExample name email =
    (fun n e -> (n, e))
    <!> validateName name
    <*> validateEmail email

(**
### Safe operations
*)

let safeExample =
    List.tryHead [1; 2; 3]
    >>= (fun x -> List.tryItem x [10; 20; 30])

(**
### Railway-oriented programming
*)

let railwayExample input =
    input
    |> validate
    >>= transform
    >>= save
    >>= notify

(**

## Frequently Used Functions

| Function | Works On | Purpose |
|----------|----------|---------|
| `map` | Functor | Transform wrapped value |
| `bind` / `>>=` | Monad | Chain operations |
| `apply` / `<*>` | Applicative | Combine independent values |
| `fold` | Foldable | Reduce to single value |
| `filter` | Filterable | Keep matching elements |
| `traverse` | Traversable | Map with effects |
| `sequence` | Traversable | Turn inside-out |
| `join` | Monad | Flatten nested structure |
| `lift2` | Applicative | Apply binary function |

---

## Common Type Signatures

*)

// map:      ('a -> 'b) -> F<'a> -> F<'b>
// bind:     ('a -> F<'b>) -> F<'a> -> F<'b>
// apply:    F<'a -> 'b> -> F<'a> -> F<'b>
// fold:     ('b -> 'a -> 'b) -> 'b -> F<'a> -> 'b
// filter:   ('a -> bool) -> F<'a> -> F<'a>
// traverse: ('a -> F<'b>) -> T<'a> -> F<T<'b>>
// sequence: T<F<'a>> -> F<T<'a>>

(**

## Keyboard Shortcuts (for operators)

In many editors, you can create snippets for common operators:

- `>>` → `>>=` (bind)
- `>-` → `>=>` (Kleisli)
- `<!` → `<!>` (map)
- `<*` → `<*>` (apply)
- `<|` → `<|>` (alternative)

---

## When in Doubt

1. **Start with extensions**: `String.trim`, `List.splitAt`, etc.
2. **Use generic map**: Works almost everywhere
3. **Chain with >>=**: For operations that might fail
4. **Remember the types**: Option (no error), Result (with error), Validation (all errors)
5. **Check the docs**: [Full documentation](index.html)

---

## Next Steps

- [Getting Started](getting-started.html) - Learn the basics
- [Learning Path](learning-path.html) - Structured guide
- [Common Patterns](common-patterns.html) - Real-world examples
- [API Reference](reference/index.html) - Complete documentation

---

## Print This Page

This cheat sheet is designed to be printed and kept handy while coding!

*)
