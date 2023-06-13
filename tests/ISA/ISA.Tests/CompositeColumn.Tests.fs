﻿module CompositeColumn.Tests

open ISA

#if FABLE_COMPILER
open Fable.Mocha
#else
open Expecto
#endif

let private tests_validate = 
    testList "validate" [
        testCase "Valid header with empty cells" (fun () ->
            let header : CompositeHeader = CompositeHeader.Characteristic (OntologyAnnotation.empty)
            let header1 : CompositeHeader = CompositeHeader.Input IOType.Source
            let header2 : CompositeHeader = CompositeHeader.ProtocolType
            let cells : CompositeCell [] = [||]
            let c1 = CompositeColumn.create(header, cells).validate()
            let c2 = CompositeColumn.create(header1, cells).validate()
            let c3 = CompositeColumn.create(header2, cells).validate()
            Expect.isTrue c1 "header"
            Expect.isTrue c2 "header1"
            Expect.isTrue c3 "header2"
        )
        testCase "Valid term with term cells" (fun () ->
            let header : CompositeHeader = CompositeHeader.Characteristic (OntologyAnnotation.empty)
            let cells : CompositeCell [] = Array.init 2 (fun _ -> CompositeCell.emptyTerm)
            let column = CompositeColumn.create(header, cells).validate()
            let eval() = CompositeColumn.create(header, cells).validate(true)
            // Still shows bool as output but could raise an exception.
            eval() |> ignore
            Expect.isTrue column ""
        )
        testCase "Valid term with unit cells" (fun () ->
            let header : CompositeHeader = CompositeHeader.Characteristic (OntologyAnnotation.empty)
            let cells : CompositeCell [] = Array.init 2 (fun _ -> CompositeCell.emptyUnitized)
            let column = CompositeColumn.create(header, cells).validate()
            let eval() = CompositeColumn.create(header, cells).validate(true)
            // Still shows bool as output but could raise an exception.
            eval() |> ignore
            Expect.isTrue column ""
        )
        testCase "Invalid term with freetext cells" (fun () ->
            let header : CompositeHeader = CompositeHeader.Characteristic (OntologyAnnotation.empty)
            let cells : CompositeCell [] = Array.init 2 (fun _ -> CompositeCell.emptyFreeText)
            let column = CompositeColumn.create(header, cells).validate()
            let eval() = CompositeColumn.create(header, cells).validate(true) |> ignore
            Expect.isFalse column ""
            Expect.throws eval ""
        )
        testCase "Invalid featured with freetext cells" (fun () ->
            let header : CompositeHeader = CompositeHeader.ProtocolType
            let cells : CompositeCell [] = Array.init 2 (fun _ -> CompositeCell.emptyFreeText)
            let column = CompositeColumn.create(header, cells).validate()
            let eval() = CompositeColumn.create(header, cells).validate(true) |> ignore
            Expect.isFalse column ""
            Expect.throws eval ""
        )
        testCase "Invalid io column with term cells" (fun () ->
            let header : CompositeHeader = CompositeHeader.Input IOType.ImageFile
            let cells : CompositeCell [] = Array.init 2 (fun _ -> CompositeCell.emptyTerm)
            let column = CompositeColumn.create(header, cells).validate()
            let eval() = CompositeColumn.create(header, cells).validate(true) |> ignore
            Expect.isFalse column ""
            Expect.throws eval ""
        )
        testCase "Invalid io column with unit cells" (fun () ->
            let header : CompositeHeader = CompositeHeader.Input IOType.ImageFile
            let cells : CompositeCell [] = Array.init 2 (fun _ -> CompositeCell.emptyUnitized)
            let column = CompositeColumn.create(header, cells).validate()
            let eval() = CompositeColumn.create(header, cells).validate(true) |> ignore
            Expect.isFalse column ""
            Expect.throws eval ""
        )
        testCase "Invalid single column with term cells" (fun () ->
            let header : CompositeHeader = CompositeHeader.ProtocolREF
            let cells : CompositeCell [] = Array.init 2 (fun _ -> CompositeCell.emptyTerm)
            let column = CompositeColumn.create(header, cells).validate()
            let eval() = CompositeColumn.create(header, cells).validate(true) |> ignore
            Expect.isFalse column ""
            Expect.throws eval ""
        )
        testCase "Invalid single column with unit cells" (fun () ->
            let header : CompositeHeader = CompositeHeader.ProtocolREF
            let cells : CompositeCell [] = Array.init 2 (fun _ -> CompositeCell.emptyUnitized)
            let column = CompositeColumn.create(header, cells).validate()
            let eval() = CompositeColumn.create(header, cells).validate(true) |> ignore
            Expect.isFalse column ""
            Expect.throws eval ""
        )
    ]

let main = 
    testList "CompositeColumn" [
        tests_validate
    ]