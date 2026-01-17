(*** hide ***)
// This block of code is omitted in the generated HTML documentation. Use 
// it to define helpers that you do not want to show in the documentation.
#r @"../../src/FSharpPlus/bin/Release/net8.0/FSharpPlus.dll"

(**
Common Patterns and Use Cases
==============================

This guide shows practical patterns you'll encounter in real-world F# development 
and how FSharpPlus makes them easier to handle.

*)

(**
```f#
#r "nuget: FSharpPlus"
```
*)

open FSharpPlus
open FSharpPlus.Data

(**

## Pattern 1: Chaining Operations That Might Fail

**Problem**: You have a series of operations, each of which might fail, and you 
want to stop at the first failure.

### Without FSharpPlus
*)

let processDataOldWay input =
    match tryParseInt input with
    | None -> None
    | Some num ->
        match tryDivide 100 num with
        | None -> None
        | Some result ->
            match tryFormat result with
            | None -> None
            | Some formatted -> Some formatted

(**
### With FSharpPlus (Option)
*)

let processDataWithOption input =
    Option.tryParse input
    >>= tryDivide 100
    >>= tryFormat

// Or using computation expression
let processDataWithMonad input = monad {
    let! num = Option.tryParse input
    let! result = tryDivide 100 num
    let! formatted = tryFormat result
    return formatted
}

(**
### With FSharpPlus (Result)
*)

let tryDivideResult x y =
    if y = 0 then Error "Division by zero"
    else Ok (x / y)

let processDataWithResult input =
    Result.tryParse input
    >>= tryDivideResult 100
    >>= tryFormatResult

(**

## Pattern 2: Validation with Multiple Errors

**Problem**: When validating user input, you want to collect ALL validation errors, 
not just the first one.

### Without FSharpPlus
*)

let validateUserOldWay name age email =
    let errors = ResizeArray()
    
    if String.IsNullOrWhiteSpace name then
        errors.Add "Name is required"
    
    if age < 0 || age > 150 then
        errors.Add "Age must be between 0 and 150"
        
    if not (String.contains "@" email) then
        errors.Add "Email must contain @"
    
    if errors.Count > 0 then
        Error (List.ofSeq errors)
    else
        Ok { Name = name; Age = age; Email = email }

(**
### With FSharpPlus Validation
*)

type UserInput = { Name: string; Age: int; Email: string }

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

// Applicative style combines all validations
let validateUser name age email =
    (fun n a e -> { Name = n; Age = a; Email = e })
    <!> validateName name
    <*> validateAge age
    <*> validateEmail email

// Example usage
let validUser = validateUser "John" 30 "john@example.com"
// Success { Name = "John"; Age = 30; Email = "john@example.com" }

let invalidUser = validateUser "" 200 "invalid"
// Failure ["Name is required"; "Age must be between 0 and 150"; "Email must contain @"]

(**

## Pattern 3: Safe List/Array Operations

**Problem**: List.head and similar functions throw exceptions on empty collections.

### Without FSharpPlus
*)

let processListOldWay items =
    if List.isEmpty items then
        None
    else
        let head = List.head items
        let tail = List.tail items
        Some (head, tail)

(**
### With FSharpPlus
*)

let processListWithFSharp items =
    List.tryHead items
    |> Option.map (fun head -> (head, List.tail items))

// Or use NonEmptyList when you know it's not empty
let processNonEmpty items =
    let nel = NonEmptyList.ofList items
    match nel with
    | Some nel -> Some (NonEmptyList.head nel, NonEmptyList.tail nel)
    | None -> None

(**

## Pattern 4: Working with Nested Options/Results

**Problem**: You have nested Options or Results and want to flatten them.

### Without FSharpPlus
*)

let processNestedOldWay (maybeX: int option option) =
    match maybeX with
    | Some (Some x) -> Some (x * 2)
    | Some None -> None
    | None -> None

(**
### With FSharpPlus
*)

// join flattens nested structures
let processNested maybeX =
    maybeX
    |> join
    |> map ((*) 2)

// Or use >>= which maps and joins in one step
let processNestedBind maybeX =
    maybeX >>= id >>= (fun x -> Some (x * 2))

(**

## Pattern 5: Combining Multiple Async Operations

**Problem**: You have multiple async operations that can run in parallel.

### With Standard F#
*)

let fetchUserOldWay userId = async {
    let! user = fetchUser userId
    let! profile = fetchProfile userId
    let! settings = fetchSettings userId
    return (user, profile, settings)
}

(**
### With FSharpPlus Applicative (Parallel)
*)

// These run in parallel!
let fetchUserData userId = applicative {
    let! user = fetchUser userId
    let! profile = fetchProfile userId
    let! settings = fetchSettings userId
    return (user, profile, settings)
}

(**

## Pattern 6: List Comprehensions

**Problem**: Generate combinations from multiple lists.

### Without FSharpPlus
*)

let combinationsOldWay =
    [for x in [1; 2; 3] do
     for y in [10; 20; 30] do
     yield x + y]

(**
### With FSharpPlus
*)

let combinations = monad.plus {
    let! x = [1; 2; 3]
    let! y = [10; 20; 30]
    return x + y
}

// With filtering
let filteredCombinations = monad.plus {
    let! x = [1; 2; 3]
    let! y = [10; 20; 30]
    where (x + y > 15)
    return x + y
}

(**

## Pattern 7: Building Lists Efficiently

**Problem**: Concatenating lists repeatedly is inefficient (O(n) per concat).

### Without FSharpPlus
*)

let buildListOldWay items =
    items
    |> List.fold (fun acc item -> acc @ [item * 2]) []

(**
### With FSharpPlus DList
*)

let buildListEfficiently items =
    items
    |> List.fold (fun (acc: DList<_>) item -> 
        acc ++ DList.singleton (item * 2)) DList.empty
    |> DList.toList

(**

## Pattern 8: Updating Nested Records

**Problem**: Updating deeply nested immutable records is verbose.

### Without FSharpPlus
*)

type Address = { Street: string; City: string }
type Person = { Name: string; Address: Address }

let updateCityOldWay person newCity =
    { person with Address = { person.Address with City = newCity } }

(**
### With FSharpPlus Lenses
*)

open FSharpPlus.Lens

// Define lenses for your types
let _address = (fun p -> p.Address), (fun a p -> { p with Address = a })
let _city = (fun a -> a.City), (fun c a -> { a with City = c })

// Compose and use
let person = { Name = "John"; Address = { Street = "Main St"; City = "NYC" } }
let updated = setl (_address << _city) "LA" person

(**

## Pattern 9: Error Handling with Context

**Problem**: Errors need context about where they occurred.

### Without FSharpPlus
*)

let processStepOldWay step value =
    match value with
    | Ok v -> 
        try
            Ok (process v)
        with ex ->
            Error (sprintf "Error in step %d: %s" step ex.Message)
    | Error e -> Error e

(**
### With FSharpPlus
*)

let processStep step value =
    value
    |> Result.bind (fun v -> Result.protect process v)
    |> Result.mapError (fun e -> sprintf "Error in step %d: %s" step e.Message)

(**

## Pattern 10: Converting Between Types

**Problem**: Need to convert between similar types (Option<->Result, List<->Array, etc.)

### With FSharpPlus
*)

// Option to Result
let optToResult opt = opt <?> "Value was None"

// Result to Option
let resultToOpt result = Result.toOption result

// List to Array and back (using generic toSeq/ofSeq)
let listToArray lst = lst |> toSeq |> Array.ofSeq
let arrayToList arr = arr |> toSeq |> List.ofSeq

// Validation to Result
let validationResult validation = Validation.toResult validation

(**

## Pattern 11: Retry Logic

**Problem**: Retry an operation that might fail.

### With FSharpPlus
*)

let rec retry times operation =
    match times with
    | 0 -> Error "Max retries exceeded"
    | n ->
        match operation() with
        | Ok result -> Ok result
        | Error _ -> retry (n - 1) operation

// Or using monad
let retryWithMonad times operation = monad {
    let mutable attempts = times
    let mutable result = Error "Not attempted"
    while attempts > 0 && Result.isError result do
        result <- operation()
        attempts <- attempts - 1
    return! result
}

(**

## Pattern 12: Working with Dictionaries

**Problem**: Dictionary lookups return KeyNotFoundException.

### Without FSharpPlus
*)

let lookupOldWay key (dict: System.Collections.Generic.Dictionary<_,_>) =
    if dict.ContainsKey key then
        Some dict.[key]
    else
        None

(**
### With FSharpPlus
*)

let lookupSafe key (dict: System.Collections.Generic.Dictionary<_,_>) =
    Dictionary.tryGetValue key dict

(**

## Pattern 13: Pipeline Transformations

**Problem**: Complex data transformation pipelines.

### With FSharpPlus
*)

let processDataPipeline data =
    data
    |> map String.trim
    |> filter (String.IsNullOrWhiteSpace >> not)
    |> map String.toLower
    |> map (String.split [|','|])
    |> map (Array.filter (fun s -> String.length s > 0))
    |> map Array.toList

(**

## Pattern 14: Conditional Execution

**Problem**: Execute code only when a condition is met.

### With FSharpPlus
*)

// Using guard in monad
let conditionalProcess value = monad {
    do! guard (value > 0)
    return value * 2
}  // Returns Some 20 if value is 10, None if value <= 0

// Using filter
let processIfValid values =
    values
    |> filter (fun x -> x > 0)
    |> map ((*) 2)

(**

## Pattern 15: Collecting Results

**Problem**: You have a list of operations that might fail, and you want all 
successes or all failures.

### With FSharpPlus
*)

// Sequence - stop at first error
let sequenceResults results =
    sequence results  // List<Result<'a, 'e>> -> Result<List<'a>, 'e>

// Traverse - map then sequence
let validateAll items =
    traverse validateItem items

let items = [1; 2; 3; 4; 5]
let validated = traverse (fun x -> 
    if x > 0 then Ok x else Error (sprintf "%d is invalid" x)) items
// Ok [1; 2; 3; 4; 5]

(**

## Tips for Choosing Patterns

1. **Start Simple**: Use extensions before reaching for abstractions
2. **Consider Readers**: Make code readable for team members
3. **Performance**: Most patterns are zero-cost abstractions
4. **Consistency**: Pick patterns and stick with them in your codebase
5. **Documentation**: Comment why you chose a particular pattern

## Next Steps

- [Getting Started](getting-started.html) - Basics of FSharpPlus
- [Learning Path](learning-path.html) - Structured learning guide
- [When to Use What](when-to-use.html) - Decision guide
- [Tutorial](tutorial.html) - In-depth exploration

*)
