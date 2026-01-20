(*** hide ***)
// This block of code is omitted in the generated HTML documentation. Use 
// it to define helpers that you do not want to show in the documentation.
#r @"../../src/FSharpPlus/bin/Release/net8.0/FSharpPlus.dll"

(**
Learning Path: From Zero to Hero
=================================

This guide provides a structured learning path through FSharpPlus, organized by 
experience level. Each level builds on the previous one, so you can progress at 
your own pace.

## Level 0: Prerequisites

Before starting with FSharpPlus, you should be comfortable with:

- Basic F# syntax (functions, types, pattern matching)
- Working with standard F# collections (List, Array, Seq)
- Understanding Option and Result types
- Basic concept of higher-order functions (map, filter, fold)

If you need a refresher on F#, check out [F# for Fun and Profit](https://fsharpforfunandprofit.com/).

---

## Level 1: Extension Functions (Week 1)

**Goal**: Use FSharpPlus to write cleaner, more concise code with familiar types.

### What to Learn
- String extensions
- Collection extensions (List, Array, Seq)
- Option and Result helpers

### Learning Resources
- [Getting Started Guide](getting-started.html) - Start here
- [Extensions Documentation](extensions.html) - Complete reference

### Practice Projects
1. **Text Processor**: Build a simple text processing tool using String extensions
2. **Data Cleaner**: Process CSV data using collection extensions
3. **Safe Calculator**: Use Option extensions for division that might fail

### Key Concepts
*)

open FSharpPlus

// String operations are more discoverable
let processText text =
    text
    |> String.trim
    |> String.toLower
    |> String.split [|' '|]
    |> Array.filter (String.IsNullOrWhiteSpace >> not)

// Collection operations with consistent naming
let processList items =
    items
    |> List.splitAt 5              // Split at index
    |> fun (first, rest) -> List.intersperse 0 first  // Add separator

(**
### Success Criteria
- [ ] Can use String extensions instead of String module functions
- [ ] Comfortable with collection extensions (splitAt, intersperse, etc.)
- [ ] Using Option.protect for exception handling
- [ ] Using Result.either for error handling

---

## Level 2: Generic Functions (Weeks 2-3)

**Goal**: Write polymorphic code that works across different types.

### What to Learn
- Generic map, filter, fold functions
- Understanding why generic functions are useful
- Working with generic operators (>>=, >=>)

### Learning Resources
- [Generic Functions Documentation](generic-doc.html)
- [Operators Reference](operators-common.html)
- [Applicative Functors Guide](applicative-functors.html)

### Practice Projects
1. **Generic Data Pipeline**: Build a data transformation pipeline that works with different containers
2. **Railway Calculator**: Implement railway-oriented programming with generic operators
3. **Universal Validator**: Create validators that work with Option, Result, and custom types

### Key Concepts
*)

// Generic map works with any "functor"
let example1() =
    let doubled1 = map ((*) 2) [1; 2; 3]        // list
    let doubled2 = map ((*) 2) [|1; 2; 3|]      // array
    let doubled3 = map ((*) 2) (Some 21)        // option
    let doubled4 = map ((*) 2) (Ok 21)          // result
    ()

// Generic bind operator >>= for chaining
let example2() =
    let tryParse x = Option.tryParse<int> x
    let tryDivide100 y = if y = 0 then None else Some (100 / y)
    
    let result = Some "20" >>= tryParse >>= tryDivide100  // Some 5
    ()

// Kleisli composition >=> for building pipelines
let example3() =
    let tryParse x = Option.tryParse<int> x
    let tryDivide100 y = if y = 0 then None else Some (100 / y)
    
    let pipeline = tryParse >=> tryDivide100
    let result = pipeline "20"                             // Some 5
    ()

(**
### Success Criteria
- [ ] Can use map instead of List.map, Option.map, etc.
- [ ] Understand when to use >>= (bind) vs >=> (Kleisli composition)
- [ ] Can read and write code with generic operators
- [ ] Understand what "functor" and "monad" mean in practical terms

---

## Level 3: FSharpPlus Types (Weeks 4-5)

**Goal**: Use specialized types that solve common problems elegantly.

### What to Learn
- NonEmptyList for non-empty collections
- Validation for accumulating errors
- DList for efficient list building
- ZipList for zippy behavior

### Learning Resources
- [Types Overview](types.html)
- [NonEmptyList](type-nonempty.html)
- [Validation](type-validation.html)
- [DList](type-dlist.html)

### Practice Projects
1. **Form Validator**: Build a web form validator that accumulates all errors
2. **Config Parser**: Parse configuration with guaranteed non-empty values
3. **Log Aggregator**: Use DList to efficiently build logs

### Key Concepts
*)

open FSharpPlus.Data

// NonEmptyList guarantees at least one element
let safeHead items =
    let nel = NonEmptyList.create (List.head items) (List.tail items)
    NonEmptyList.head nel  // No need to check for empty!

// Validation accumulates ALL errors, not just the first
type User = { Name: string; Age: int; Email: string }

let validateName name =
    if String.length name > 0 
    then Validation.ok name
    else Validation.error ["Name required"]

let validateAge age =
    if age >= 0 && age <= 150
    then Validation.ok age
    else Validation.error ["Age must be 0-150"]

let validateEmail email =
    if String.contains "@" email
    then Validation.ok email  
    else Validation.error ["Invalid email"]

// Combine validations using applicative style
let createUser name age email =
    (fun n a e -> { Name = n; Age = a; Email = e })
    <!> validateName name
    <*> validateAge age
    <*> validateEmail email
    
(**
### Success Criteria
- [ ] Use NonEmptyList when a collection shouldn't be empty
- [ ] Use Validation to collect multiple errors
- [ ] Use DList for efficient concatenation
- [ ] Understand the difference between Result and Validation

---

## Level 4: Computation Expressions (Week 6)

**Goal**: Write clean, readable code for complex workflows.

### What to Learn
- The monad computation expression
- When to use monad vs monad.plus
- Applicative computation expressions
- Understanding lazy vs strict evaluation

### Learning Resources
- [Computation Expressions Guide](computation-expressions.html)
- [Monad Abstraction](abstraction-monad.html)

### Practice Projects
1. **Async Workflow Builder**: Chain async operations with monad
2. **Query Builder**: Build queries using monad.plus
3. **Parser Combinator**: Create simple parsers with computation expressions

### Key Concepts
*)

// monad works with any monad
let workflow1 = monad {
    let! x = Some 10
    let! y = Some 32
    return x + y
}  // Some 42

// monad.plus for multiple results (like list comprehensions)
let combinations = monad.plus {
    let! x = [1; 2; 3]
    let! y = [10; 20]  
    return x + y
}  // [11; 21; 12; 22; 13; 23]

// applicative for independent computations
let parallel = applicative {
    let! x = async { return 10 }
    let! y = async { return 32 }
    return x + y
}

(**
### Success Criteria
- [ ] Can use monad { } for sequential workflows
- [ ] Can use monad.plus { } for multiple results
- [ ] Understand when to use applicative vs monad
- [ ] Know the difference between lazy and strict evaluation

---

## Level 5: Abstractions (Weeks 7-9)

**Goal**: Understand the underlying abstractions and design your own instances.

### What to Learn
- Functor, Applicative, Monad abstractions
- Foldable and Traversable
- How to make your types work with FSharpPlus
- Type classes in F#

### Learning Resources
- [Abstractions Overview](abstractions.html)
- [Functor](abstraction-functor.html)
- [Applicative](abstraction-applicative.html)
- [Monad](abstraction-monad.html)
- [Foldable](abstraction-foldable.html)

### Practice Projects
1. **Custom Container**: Create a tree type that works with map, fold, etc.
2. **Domain Monad**: Build a domain-specific monad for your business logic
3. **Traversable Type**: Implement a type that can be traversed

### Key Concepts
*)

// Make your own type work with generic functions
type Tree<'T> =
    | Leaf of 'T
    | Node of Tree<'T> * Tree<'T>
    static member Map(tree, f) =
        let rec loop = function
            | Leaf x -> Leaf (f x)
            | Node (l, r) -> Node (loop l, loop r)
        loop tree

// Now it works with generic map!
let tree = Node(Leaf 1, Node(Leaf 2, Leaf 3))
let doubled = map ((*) 2) tree

(**
### Success Criteria
- [ ] Understand Functor laws (identity, composition)
- [ ] Understand Applicative and Monad relationships
- [ ] Can implement Map, Return, Bind for custom types
- [ ] Know when to use Foldable vs Traversable

---

## Level 6: Advanced Features (Weeks 10-12)

**Goal**: Master advanced features like lenses, transformers, and arrows.

### What to Learn
- Lenses for elegant data access and updates
- Monad transformers for combining effects
- Arrows and Categories
- Type-level programming

### Learning Resources
- [Lens Tutorial](lens.html)
- Monad transformer type documentation (ReaderT, StateT, etc.)
- [Arrow Abstraction](abstraction-arrow.html)

### Practice Projects
1. **Lens-Based Editor**: Use lenses to update nested records
2. **Combined Effects**: Use StateT with ReaderT for complex workflows
3. **Parser Library**: Build parsers using arrows

### Key Concepts
*)

open FSharpPlus.Lens

// Lenses for reading and writing nested data
let person = ("John", (30, "john@example.com"))
let age = person ^. (_2 << _1)                    // 30
let updated = setl (_2 << _1) 31 person           // ("John", (31, "john@example.com"))

// Monad transformers combine effects
// ReaderT adds environment to any monad
// StateT adds state to any monad
// WriterT adds logging to any monad

(**
### Success Criteria
- [ ] Can use lenses to read and update nested data
- [ ] Understand when to use monad transformers
- [ ] Can combine multiple effects (Reader + State, etc.)
- [ ] Comfortable with advanced type signatures

---

## Level 7: Expert (Ongoing)

**Goal**: Contribute to the library and help others learn.

### What to Do
- Read the [Developer Guide](https://github.com/fsprojects/FSharpPlus/blob/master/DEVELOPER_GUIDE.md)
- Explore the source code
- Answer questions on Gitter and Stack Overflow
- Contribute examples and documentation
- Submit bug fixes and features

### Advanced Topics
- Performance optimization techniques
- Type-level computation
- Category theory foundations
- Designing APIs with FSharpPlus

---

## Learning Tips

### Do's
✅ **Start small** - Don't try to learn everything at once  
✅ **Practice daily** - Write code using what you learned  
✅ **Read others' code** - See how experienced developers use FSharpPlus  
✅ **Ask questions** - The community is friendly and helpful  
✅ **Take breaks** - Let concepts sink in between levels  

### Don'ts
❌ **Don't skip levels** - Each builds on the previous  
❌ **Don't memorize** - Understand the "why" behind each concept  
❌ **Don't compare yourself** - Everyone learns at their own pace  
❌ **Don't use features you don't understand** - Keep it simple  

---

## Recommended Weekly Schedule

- **Week 1**: Extensions only, build 2-3 small projects
- **Week 2**: Add generic map, filter, fold to your toolkit
- **Week 3**: Master generic operators (>>=, >=>)
- **Week 4**: Learn NonEmptyList and DList
- **Week 5**: Master Validation for error handling
- **Week 6**: Use computation expressions everywhere
- **Week 7-9**: Study abstractions, implement custom types
- **Week 10-12**: Explore advanced features selectively

---

## Next Steps

Based on your current level:

- **Beginner**: Start with [Getting Started](getting-started.html)
- **Intermediate**: Explore [Tutorial](tutorial.html) for deeper understanding
- **Advanced**: Study [Abstractions](abstractions.html) and type details

Remember: The goal isn't to use every feature of FSharpPlus. The goal is to 
write better, more maintainable F# code. Use what helps you, skip what doesn't.

Good luck on your journey! 🎯
*)
