(*** hide ***)
// This block of code is omitted in the generated HTML documentation. Use 
// it to define helpers that you do not want to show in the documentation.

(**
Arrow
=====

Arrow<'T, 'U> represents a process that takes as input something of type 'T and outputs something of type 'U.

___



Minimal complete definition
---------------------------


 * ``arr f`` and  ``first f``

*)
(**
    static member Arr(f: 'T -> 'U) : 'Arrow<'T, 'U>
    static member First (f: 'Arrow<'T, 'U>) : 'Arrow<('T * 'V),('U * 'V)>
*)
(**



Other operations
----------------

 * ``second f``
*)
(**
    static member Second (f: 'Arrow<'T, 'U>) : 'Arrow<('V * 'T),('V * 'U)>
*)
(**
 * ``(***) f g``
*)
(**
    static member ``***`` (f : 'Arrow<'T1,'U1>) (g : 'Arrow<'T2,'U2>) : 'Arrow<('T1 * 'T2),('U1 * 'U2)>
*)
(**
 * ``(&&&) f g``
*)
(**
    static member  (&&&) (f : 'Arrow<'T,'U1>) (g : 'Arrow<'T,'U2>) : 'Arrow<'T,('U1 * 'U2)>
*)
(**



Rules
-----
*)
(**
    arr id = id
    arr (f >>> g) = arr f >>> arr g
    first (arr f) = arr (first f)
    first (f >>> g) = first f >>> first g
    first f >>> arr fst = arr fst >>> f
    first f >>> arr (id *** g) = arr (id *** g) >>> first f
    first (first f) >>> arr assoc = arr assoc >>> first f

    where assoc ((a,b),c) = (a,(b,c))
*)
(**

Examples
--------

Here are some examples showing how Arrow operations work:

*)

#r "nuget: FSharpPlus"
open FSharpPlus

// Basic arrow operations with functions
let addOne = (+) 1
let multiplyByTwo = (*) 2

// arr: lift a function into an arrow
let arrowAddOne = arr addOne
let result1 = arrowAddOne 5 // 6

// first: apply arrow to first element of a tuple
let firstAddOne = first (arr addOne)
let result2 = firstAddOne (5, "hello") // (6, "hello")

// second: apply arrow to second element of a tuple  
let secondMultiply = second (arr multiplyByTwo)
let result3 = secondMultiply ("hello", 5) // ("hello", 10)

// (***): apply two arrows to both elements of a tuple
let bothOps = arr addOne *** arr multiplyByTwo
let result4 = bothOps (5, 3) // (6, 6)

// (&&&): apply two arrows to the same input, producing a tuple
let fanout = arr addOne &&& arr multiplyByTwo
let result5 = fanout 5 // (6, 10)

// Composing arrows
let composed = arr addOne >>> arr multiplyByTwo
let result6 = composed 5 // 12 (first add 1, then multiply by 2)

(**

Working with Kleisli arrows:

*)

// Kleisli arrows for Option monad
let safeSquareRoot x = if x < 0.0 then None else Some (sqrt x)
let addTen x = Some (x + 10.0)

// Create Kleisli arrows
let kleisliSqrt = Kleisli safeSquareRoot
let kleisliAdd = Kleisli addTen

// Compose Kleisli arrows: first add 10, then take square root
let composedKleisli = kleisliAdd >>> kleisliSqrt
let result7 = Kleisli.run composedKleisli 6.0 // Some 4.0 (sqrt(16))

(**


Concrete implementations
------------------------

From .Net/F#
 
 -  ``'T->'U``
 -  ``Func<'T,'U>``

 
From F#+

 -  [``Kleisli<'T, 'Monad<'U>>``](type-kleisli.html)

 [Suggest another](https://github.com/fsprojects/FSharpPlus/issues/new) concrete implementation
*)
