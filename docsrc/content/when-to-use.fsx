(*** hide ***)
// This block of code is omitted in the generated HTML documentation. Use 
// it to define helpers that you do not want to show in the documentation.
#r @"../../src/FSharpPlus/bin/Release/net8.0/FSharpPlus.dll"

(**
When to Use What: Decision Guide
=================================

FSharpPlus provides many features and abstractions. This guide helps you choose 
the right tool for your specific situation.

## Quick Decision Tree

```
Do you need to...
├─ Work with familiar F# types more easily?
│  └─ Use Extension Functions
│
├─ Write code that works with multiple container types?
│  └─ Use Generic Functions (map, filter, fold, etc.)
│
├─ Chain operations that might fail?
│  ├─ Stop at first error? → Use Result with bind (>>=)
│  └─ Collect all errors? → Use Validation
│
├─ Ensure a collection is never empty?
│  └─ Use NonEmptyList
│
├─ Build lists by concatenation?
│  └─ Use DList
│
├─ Handle nested monadic values?
│  └─ Use Monad Transformers (OptionT, ResultT, etc.)
│
├─ Update nested records?
│  └─ Use Lenses
│
└─ Learn functional patterns?
   └─ Study Abstractions (Functor, Applicative, Monad)
```

---

## Option vs Result vs Validation

### Use Option when:
*)

open FSharpPlus
open FSharpPlus.Data

// ✅ Absence of value is normal (not an error)
let findUser userId = 
    if userId > 0 then Some { Id = userId; Name = "User" }
    else None

// ✅ You don't need error information
let tryParse text = Option.tryParse<int> text

// ✅ Chaining operations where failure is expected
let getUserEmail userId =
    findUser userId
    >>= (fun user -> findEmail user.Id)
    >>= validateEmail

(**
### Use Result when:
*)

// ✅ You need error messages
let validateAge age =
    if age >= 0 && age <= 150
    then Ok age
    else Error "Age must be between 0 and 150"

// ✅ Railway-oriented programming
let processOrder order =
    order
    |> validateOrder
    >>= checkInventory
    >>= calculatePrice
    >>= applyDiscount

// ✅ You want to stop at the first error
let parseAndValidate text =
    Result.tryParse text
    >>= validatePositive
    >>= validateInRange

(**
### Use Validation when:
*)

// ✅ You want to collect ALL validation errors
type FormData = { Name: string; Age: int; Email: string }

let validateForm name age email =
    let validateName n =
        if String.length n > 0 
        then Validation.ok n
        else Validation.error ["Name required"]
    
    let validateAge a =
        if a >= 18
        then Validation.ok a
        else Validation.error ["Must be 18+"]
    
    let validateEmail e =
        if String.contains "@" e
        then Validation.ok e
        else Validation.error ["Invalid email"]
    
    // Collects all errors, doesn't stop at first
    (fun n a e -> { Name = n; Age = a; Email = e })
    <!> validateName name
    <*> validateAge age
    <*> validateEmail email

(**
**Summary Table:**

| Use Case | Type | Stops at First Error? | Has Error Info? |
|----------|------|----------------------|-----------------|
| Optional value | Option | Yes | No |
| Error with message | Result | Yes | Yes |
| Multiple validations | Validation | No (collects all) | Yes |

---

## List vs Seq vs Array vs NonEmptyList

### Use List when:
*)

// ✅ Small to medium collections
let items = [1; 2; 3; 4; 5]

// ✅ Pattern matching on structure
let rec sum lst =
    match lst with
    | [] -> 0
    | head :: tail -> head + sum tail

// ✅ Recursive operations
let rec quicksort = function
    | [] -> []
    | pivot :: rest ->
        let smaller = List.filter ((>) pivot) rest
        let larger = List.filter ((<=) pivot) rest
        quicksort smaller @ [pivot] @ quicksort larger

(**
### Use Seq when:
*)

// ✅ Lazy evaluation needed
let infiniteSeq = Seq.initInfinite id

// ✅ Large or potentially infinite data
let numbers = seq { 1 .. 1000000 }

// ✅ One-time iteration
let processLargeFile filename =
    System.IO.File.ReadLines(filename)
    |> Seq.filter (String.IsNullOrWhiteSpace >> not)
    |> Seq.take 100

(**
### Use Array when:
*)

// ✅ Random access needed
let lookupByIndex arr idx = arr.[idx]

// ✅ Mutable updates (rare in functional code)
let arr = [|1; 2; 3|]
arr.[1] <- 10

// ✅ Performance-critical code
let fastProcess = Array.map (fun x -> x * 2) [|1..10000|]

(**
### Use NonEmptyList when:
*)

// ✅ Empty list would be a logic error
let calculateAverage (nel: NonEmptyList<float>) =
    let sum = NonEmptyList.reduce (+) nel
    let count = float (NonEmptyList.length nel)
    sum / count  // Safe! Division by zero impossible

// ✅ You always need at least one element
let selectFirst (nel: NonEmptyList<'a>) =
    NonEmptyList.head nel  // No Option needed!

(**
---

## DList vs List

### Use DList when:
*)

// ✅ Building lists by appending
let buildLargeLst items =
    items
    |> List.fold (fun acc item ->
        acc ++ DList.singleton (process item)) DList.empty
    |> DList.toList

// ✅ Frequent concatenation operations
let concatenateMany lists =
    lists
    |> List.map DList.ofList
    |> List.fold (++) DList.empty
    |> DList.toList

(**
### Use List when:
*)

// ✅ Prepending (cons) operations
let addToFront item list = item :: list

// ✅ Pattern matching
let processItems = function
    | [] -> "empty"
    | [single] -> "one"
    | first :: rest -> "many"

(**
---

## map vs bind vs apply

### Use map when:
*)

// ✅ Transforming values inside a context
let doubled = map ((*) 2) (Some 21)              // Some 42
let strings = map string [1; 2; 3]               // ["1"; "2"; "3"]

(**
### Use bind (>>=) when:
*)

// ✅ Chaining operations that return wrapped values
let result = 
    Some "42"
    >>= Option.tryParse
    >>= (fun x -> if x > 0 then Some x else None)

(**
### Use apply (<*>) when:
*)

// ✅ Combining independent computations
let sum3 = Some (+) <*> Some 1 <*> Some 2        // Some 3

// ✅ Parallel-ish operations
let combined = applicative {
    let! x = async { return 1 }
    let! y = async { return 2 }
    return x + y
}

(**
**When to use which?**

| Operation | Signature | Use When |
|-----------|-----------|----------|
| map | `('a -> 'b) -> F<'a> -> F<'b>` | Just transforming values |
| bind | `('a -> F<'b>) -> F<'a> -> F<'b>` | Next step depends on previous |
| apply | `F<'a -> 'b> -> F<'a> -> F<'b>` | Combining independent values |

---

## Computation Expressions: monad vs applicative

### Use monad when:
*)

// ✅ Sequential operations where each depends on the previous
let workflow = monad {
    let! user = getUser userId
    let! profile = getProfile user.ProfileId  // Depends on user
    let! posts = getPosts user.Id             // Depends on user
    return (user, profile, posts)
}

// ✅ Conditional logic
let processWithCondition x = monad {
    let! value = getValue x
    if value > 10 then
        let! extra = getExtra value
        return value + extra
    else
        return value
}

(**
### Use applicative when:
*)

// ✅ Independent operations that can run in parallel
let parallelWorkflow = applicative {
    let! user = getUser userId        // Independent
    let! settings = getSettings ()    // Independent
    let! config = getConfig ()        // Independent
    return (user, settings, config)
}

(**
---

## Lenses vs Record Updates

### Use Lenses when:
*)

open FSharpPlus.Lens

// ✅ Deep nesting
type Address = { Street: string; City: string; Zip: string }
type Person = { Name: string; Address: Address }
type Company = { Name: string; CEO: Person }

let company = { Name = "Acme"; CEO = { Name = "John"; Address = { Street = "Main"; City = "NYC"; Zip = "10001" } } }

// With lenses
let _ceo = (fun c -> c.CEO), (fun ceo c -> { c with CEO = ceo })
let _address = (fun p -> p.Address), (fun addr p -> { p with Address = addr })
let _city = (fun a -> a.City), (fun city a -> { a with City = city })

let updated = setl (_ceo << _address << _city) "LA" company

(**
### Use Record Updates when:
*)

// ✅ Shallow updates
let updateName person newName =
    { person with Name = newName }

// ✅ Single-level nesting
let updateCity person newCity =
    { person with Address = { person.Address with City = newCity } }

(**
---

## Monad Transformers vs Custom Types

### Use Monad Transformers when:
*)

// ✅ Combining effects (Option + Async, Result + State, etc.)
let workflow : OptionT<Async<int option>> = monad {
    let! x = OptionT (async { return Some 10 })
    let! y = OptionT (async { return Some 32 })
    return x + y
}

(**
### Use Custom Types when:
*)

// ✅ Domain-specific needs
type OrderResult =
    | Success of Order
    | OutOfStock of ProductId
    | PaymentFailed of Reason
    | InvalidAddress

(**
---

## Generic Functions vs Module Functions

### Use Generic Functions when:
*)

// ✅ Writing polymorphic code
let processContainer items =
    items
    |> map ((*) 2)
    |> filter ((>) 100)
    |> fold (+) 0

// Works with List, Array, Seq, and more!

(**
### Use Module Functions when:
*)

// ✅ Type-specific behavior needed
let processList items =
    items
    |> List.map ((*) 2)
    |> List.filter ((>) 100)
    |> List.fold (+) 0

// ✅ Team prefers explicit types
let processArray (items: int[]) =
    Array.map ((*) 2) items

(**
---

## Common Scenarios

### Scenario: Web Form Validation
**Use:** Validation type for collecting all errors
*)

let validateWebForm form =
    (fun name email age -> { Name = name; Email = email; Age = age })
    <!> validateName form.Name
    <*> validateEmail form.Email
    <*> validateAge form.Age

(**
### Scenario: Database Query Chain
**Use:** Result type with bind for short-circuiting on errors
*)

let getUserWithPosts userId =
    getUser userId
    >>= ensureActive
    >>= loadPosts
    >>= enrichWithMetadata

(**
### Scenario: Configuration Parsing
**Use:** Result for errors, NonEmptyList for required lists
*)

let parseConfig text =
    text
    |> Result.tryParse
    >>= validateConfig
    >>= convertToModel

(**
### Scenario: Async Operations
**Use:** Applicative for parallel, Monad for sequential
*)

// Parallel
let loadDashboard = applicative {
    let! user = loadUser()
    let! stats = loadStats()
    let! notifications = loadNotifications()
    return (user, stats, notifications)
}

// Sequential
let processOrder = monad {
    let! order = getOrder()
    let! validated = validateOrder order
    let! payment = processPayment validated
    return payment
}

(**
---

## Decision Checklist

Before choosing a feature, ask yourself:

1. **Do I need this?** - Start with standard F#, add FSharpPlus when needed
2. **Will my team understand it?** - Consider team experience level
3. **Is it worth the complexity?** - Sometimes simpler is better
4. **Am I following consistent patterns?** - Stick to project conventions
5. **Can I explain why I chose this?** - Document unusual choices

---

## Next Steps

- [Getting Started](getting-started.html) - Begin using FSharpPlus
- [Learning Path](learning-path.html) - Structured learning guide
- [Common Patterns](common-patterns.html) - Real-world examples
- [Tutorial](tutorial.html) - Deep dive into features

*)
