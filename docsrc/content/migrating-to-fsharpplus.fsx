(*** hide ***)
// This block of code is omitted in the generated HTML documentation. Use 
// it to define helpers that you do not want to show in the documentation.
#r @"../../src/FSharpPlus/bin/Release/net8.0/FSharpPlus.dll"

(**
Migrating to FSharpPlus
========================

This guide shows how to gradually adopt FSharpPlus in your existing F# codebase,
with before/after examples for common scenarios.

## Migration Strategy

### The Safe Approach: Three Phases

1. **Phase 1: Extension Functions** (Low risk, high value)
2. **Phase 2: Generic Functions** (Medium risk, medium value)
3. **Phase 3: New Types & Abstractions** (Higher learning curve, high value)

You can stop at any phase based on your needs.

---

## Phase 1: Extension Functions

These are drop-in replacements that make your code cleaner without changing structure.

### String Operations

#### Before
*)

open System

let processText (text: string) =
    let trimmed = text.Trim()
    let lower = trimmed.ToLower()
    let replaced = lower.Replace("old", "new")
    replaced

(**
#### After
*)

open FSharpPlus

let processText text =
    text
    |> String.trim
    |> String.toLower
    |> String.replace "old" "new"

(**
### List Operations

#### Before
*)

let splitList n lst =
    let rec splitAt idx lst =
        match idx, lst with
        | 0, xs -> [], xs
        | _, [] -> [], []
        | i, x::xs ->
            let left, right = splitAt (i-1) xs
            x::left, right
    splitAt n lst

(**
#### After
*)

let splitList n lst =
    List.splitAt n lst

(**
### Safe Operations

#### Before
*)

let tryParseInt text =
    try
        Some (int text)
    with
    | _ -> None

let safeHead lst =
    match lst with
    | [] -> None
    | x::_ -> Some x

(**
#### After
*)

let tryParseInt text =
    Option.tryParse<int> text

let safeHead lst =
    List.tryHead lst

(**
---

## Phase 2: Generic Functions

Replace type-specific functions with generic ones for more flexible code.

### Generic Map

#### Before
*)

let processNumbers (numbers: int list) =
    List.map (fun x -> x * 2) numbers

let processOptional (maybeNumber: int option) =
    Option.map (fun x -> x * 2) maybeNumber

let processArray (numbers: int[]) =
    Array.map (fun x -> x * 2) numbers

(**
#### After
*)

// One implementation works for all
let processContainer numbers =
    map (fun x -> x * 2) numbers

// Call it with any container
let results1 = processContainer [1; 2; 3]           // list
let results2 = processContainer (Some 5)            // option
let results3 = processContainer [|1; 2; 3|]         // array

(**
### Chaining with Bind

#### Before
*)

let parseAndValidate text =
    match tryParseInt text with
    | None -> None
    | Some num ->
        match validatePositive num with
        | None -> None
        | Some valid ->
            match processNumber valid with
            | None -> None
            | Some result -> Some result

(**
#### After
*)

let parseAndValidate text =
    tryParseInt text
    >>= validatePositive
    >>= processNumber

// Or with computation expression
let parseAndValidate' text = monad {
    let! num = tryParseInt text
    let! valid = validatePositive num
    let! result = processNumber valid
    return result
}

(**
### Generic Fold

#### Before
*)

let sumList numbers = List.fold (+) 0 numbers
let sumArray numbers = Array.fold (+) 0 numbers
let sumSeq numbers = Seq.fold (+) 0 numbers

(**
#### After
*)

let sumAny numbers = fold (+) 0 numbers
// Works with list, array, seq, and more

(**
---

## Phase 3: New Types

Introduce FSharpPlus types to solve specific problems.

### From List to NonEmptyList

#### Before
*)

type Config = {
    Servers: string list
}

let getFirstServer config =
    match config.Servers with
    | [] -> failwith "No servers configured"
    | first::_ -> first

let loadBalance config =
    if List.isEmpty config.Servers then
        failwith "No servers"
    else
        // Distribute load...
        config.Servers |> List.head

(**
#### After
*)

open FSharpPlus.Data

type Config = {
    Servers: NonEmptyList<string>
}

let getFirstServer config =
    NonEmptyList.head config.Servers  // Always safe!

let loadBalance config =
    // No need to check - type guarantees at least one server
    let primary = NonEmptyList.head config.Servers
    // Distribute load...
    primary

(**
### From Multiple Results to Validation

#### Before
*)

type ValidationError = string list

let validateUser name email age =
    let errors = ResizeArray()
    
    if String.IsNullOrWhiteSpace name then
        errors.Add "Name required"
    
    if not (String.contains "@" email) then
        errors.Add "Invalid email"
    
    if age < 18 then
        errors.Add "Must be 18+"
    
    if errors.Count > 0 then
        Error (List.ofSeq errors)
    else
        Ok { Name = name; Email = email; Age = age }

(**
#### After
*)

let validateUser name email age =
    let validateName n =
        if String.IsNullOrWhiteSpace n
        then Validation.error ["Name required"]
        else Validation.ok n
    
    let validateEmail e =
        if String.contains "@" e
        then Validation.ok e
        else Validation.error ["Invalid email"]
    
    let validateAge a =
        if a >= 18
        then Validation.ok a
        else Validation.error ["Must be 18+"]
    
    (fun n e a -> { Name = n; Email = e; Age = a })
    <!> validateName name
    <*> validateEmail email
    <*> validateAge age

(**
### From Builder Pattern to DList

#### Before
*)

let buildReport items =
    let mutable result = []
    for item in items do
        result <- result @ [sprintf "Item: %s" item.Name]
        if item.HasDetails then
            result <- result @ [sprintf "  Details: %s" item.Details]
    result

(**
#### After
*)

let buildReport items =
    items
    |> List.fold (fun (acc: DList<_>) item ->
        let acc' = acc ++ DList.singleton (sprintf "Item: %s" item.Name)
        if item.HasDetails then
            acc' ++ DList.singleton (sprintf "  Details: %s" item.Details)
        else
            acc') DList.empty
    |> DList.toList

(**
---

## Common Migration Patterns

### Pattern: Option Chains

#### Before
*)

let processOrder orderId =
    match findOrder orderId with
    | None -> None
    | Some order ->
        match validateOrder order with
        | None -> None
        | Some valid ->
            match processPayment valid with
            | None -> None
            | Some payment -> Some payment

(**
#### After
*)

let processOrder orderId =
    findOrder orderId
    >>= validateOrder
    >>= processPayment

(**
### Pattern: Result Chains

#### Before
*)

let pipeline input =
    match step1 input with
    | Error e -> Error e
    | Ok r1 ->
        match step2 r1 with
        | Error e -> Error e
        | Ok r2 ->
            match step3 r2 with
            | Error e -> Error e
            | Ok r3 -> Ok r3

(**
#### After
*)

let pipeline input =
    input
    |> step1
    >>= step2
    >>= step3

(**
### Pattern: List Comprehensions

#### Before
*)

let combinations =
    [for x in [1..3] do
     for y in [10..10..30] do
     if x + y > 15 then
         yield x + y]

(**
#### After
*)

let combinations = monad.plus {
    let! x = [1..3]
    let! y = [10..10..30]
    where (x + y > 15)
    return x + y
}

(**
### Pattern: Async Workflows

#### Before
*)

let fetchUserData userId = async {
    let! user = fetchUser userId
    let! profile = fetchProfile userId
    let! settings = fetchSettings userId
    return (user, profile, settings)
}

(**
#### After (Parallel)
*)

// These run in parallel with applicative
let fetchUserData userId = applicative {
    let! user = fetchUser userId
    let! profile = fetchProfile userId
    let! settings = fetchSettings userId
    return (user, profile, settings)
}

(**
---

## Incremental Adoption Guidelines

### Start Here
✅ Replace string operations with FSharpPlus String module  
✅ Use Option.protect instead of try-catch  
✅ Use tryParse instead of manual parsing  

### Then Move To
✅ Replace List.map with generic map  
✅ Use >>= for option/result chains  
✅ Use generic fold and filter  

### Finally Consider
✅ NonEmptyList for non-empty collections  
✅ Validation for form validation  
✅ Computation expressions (monad { })  
✅ DList for efficient list building  

### Advanced (Optional)
✅ Lenses for nested record updates  
✅ Monad transformers for combining effects  
✅ Custom type instances for domain types  

---

## Handling Team Concerns

### "This looks weird"
**Response**: Start with extension functions only. They're just better names for 
existing concepts. Introduce generic functions after the team is comfortable.

### "Will this be maintained?"
**Response**: FSharpPlus is well-established with active maintenance. It builds 
on stable F# features and doesn't require language changes.

### "Is it too abstract?"
**Response**: You choose your abstraction level. Use extensions only if that's 
all you need. The library grows with you.

### "Performance concerns?"
**Response**: Most FSharpPlus features compile to the same code as hand-written 
alternatives. Generic functions use inline and have no runtime overhead.

---

## Migration Checklist

Before migrating a module:

- [ ] Read existing code to understand patterns
- [ ] Identify repetitive Option/Result handling
- [ ] Look for manual parsing with try-catch
- [ ] Find string operations using System.String directly
- [ ] Check for nested pattern matching on Options/Results
- [ ] Look for repeated List.map, Array.map, etc.

After migration:

- [ ] Ensure all tests still pass
- [ ] Review with team member unfamiliar with changes
- [ ] Update documentation with new patterns
- [ ] Add comments explaining non-obvious uses
- [ ] Consider creating team guidelines

---

## Code Review Guidelines

When reviewing FSharpPlus code:

### ✅ Good Signs
- Cleaner, more concise code
- Fewer nested pattern matches
- Consistent use of operators
- Clear intent with appropriate types
- Well-documented unusual patterns

### ⚠️ Warning Signs
- Too many operators (>>=, <*>, etc.) in one expression
- Generic functions where specific is clearer
- Abstractions team doesn't understand
- Using advanced features unnecessarily
- No comments on complex transformations

---

## Example: Complete Migration

### Before: Order Processing System
*)

module OrderProcessing_Before =
    
    type Order = { Id: int; Items: string list; Total: decimal }
    type ValidationError = string
    
    let validateOrder order =
        if List.isEmpty order.Items then
            Error "Order must have items"
        elif order.Total <= 0m then
            Error "Total must be positive"
        else
            Ok order
    
    let processPayment order =
        try
            // Payment processing...
            Ok order
        with ex ->
            Error ex.Message
    
    let sendConfirmation order =
        try
            // Send email...
            Ok order
        with ex ->
            Error ex.Message
    
    let processOrder order =
        match validateOrder order with
        | Error e -> Error e
        | Ok validated ->
            match processPayment validated with
            | Error e -> Error e
            | Ok paid ->
                match sendConfirmation paid with
                | Error e -> Error e
                | Ok confirmed -> Ok confirmed

(**
### After: With FSharpPlus
*)

module OrderProcessing_After =
    
    open FSharpPlus
    open FSharpPlus.Data
    
    type Order = { Id: int; Items: NonEmptyList<string>; Total: decimal }
    type ValidationError = string
    
    let validateOrder order =
        if order.Total <= 0m then
            Error "Total must be positive"
        else
            Ok order
        // No need to check Items - NonEmptyList guarantees it!
    
    let processPayment order =
        Result.protect (fun o ->
            // Payment processing...
            o) order
    
    let sendConfirmation order =
        Result.protect (fun o ->
            // Send email...
            o) order
    
    let processOrder order =
        order
        |> validateOrder
        >>= processPayment
        >>= sendConfirmation
    
    // Or with computation expression
    let processOrder' order = monad {
        let! validated = validateOrder order
        let! paid = processPayment validated
        let! confirmed = sendConfirmation paid
        return confirmed
    }

(**
---

## Next Steps

- [Getting Started](getting-started.html) - Quick introduction
- [Learning Path](learning-path.html) - Structured learning
- [Common Patterns](common-patterns.html) - Real-world examples
- [When to Use What](when-to-use.html) - Decision guide

Remember: Migration should be gradual and pragmatic. Don't force it where it 
doesn't help. The goal is better code, not using every feature.
*)
