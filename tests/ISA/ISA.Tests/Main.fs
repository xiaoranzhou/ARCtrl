﻿module ISADotnet.Tests

open ISA

#if FABLE_COMPILER
open Fable.Mocha
#else
open Expecto

[<Tests>]
#endif
let all = testSequenced <| testList "All" [
    Update.Tests.main
    DataModel.Tests.main
    CompositeHeader.Tests.main
    CompositeCell.Tests.main
    CompositeColumn.Tests.main
    ArcTable.Tests.main
]

let [<EntryPoint>] main argv = 
    #if FABLE_COMPILER
    Mocha.runTests all
    #else
    Tests.runTestsWithCLIArgs [] argv all
    #endif