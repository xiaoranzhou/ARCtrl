﻿module CompositeCell.Tests

open ISA

#if FABLE_COMPILER
open Fable.Mocha
#else
open Expecto
#endif

let private tests_cellConverter = 
    testList "CellConverter" [
        testCase "FreeText toFreeText" (fun () ->
            let currentCell : CompositeCell = CompositeCell.FreeText "path/to/input"
            let asNewCell : CompositeCell = currentCell.toFreetextCell()
            let expected = currentCell
            Expect.equal asNewCell expected ""
        )
        testCase "FreeText toTerm" (fun () ->
            let currentCell : CompositeCell = CompositeCell.FreeText "path/to/input"
            let asNewCell : CompositeCell = currentCell.toTermCell()
            let expected = CompositeCell.Term <| OntologyAnnotation.fromString "path/to/input"
            Expect.equal asNewCell expected ""
        )
        testCase "FreeText toUnitized" (fun () ->
            let currentCell : CompositeCell = CompositeCell.FreeText "path/to/input"
            let asNewCell : CompositeCell = currentCell.toUnitizedCell()
            let expected = CompositeCell.Unitized <| ("", OntologyAnnotation.fromString "path/to/input")
            Expect.equal asNewCell expected ""
        )
        testCase "Term toFreeText" (fun () ->
            let oa = OntologyAnnotation.fromString("instrument model", "MS", "MS:000000042")
            let currentCell : CompositeCell = CompositeCell.Term oa
            let asNewCell : CompositeCell = currentCell.toFreetextCell()
            let expected = CompositeCell.FreeText oa.NameText
            Expect.equal asNewCell expected ""
        )
        testCase "Term toTerm" (fun () ->
            let currentCell : CompositeCell = CompositeCell.Term <| OntologyAnnotation.fromString("instrument model", "MS", "MS:000000042")
            let asNewCell : CompositeCell = currentCell.toTermCell()
            let expected = currentCell
            Expect.equal asNewCell expected ""
        )
        testCase "Term toUnitized" (fun () ->
            let oa = OntologyAnnotation.fromString("instrument model", "MS", "MS:000000042")
            let currentCell : CompositeCell = CompositeCell.Term oa
            let asNewCell : CompositeCell = currentCell.toUnitizedCell()
            let expected = CompositeCell.Unitized <| ("", oa)
            Expect.equal asNewCell expected ""
        )
        testCase "Unitized toFreeText" (fun () ->
            let oa = OntologyAnnotation.fromString("degree celsius", "UO", "UO:000000042")
            let currentCell : CompositeCell = CompositeCell.Unitized <| ("42", oa)
            let asNewCell : CompositeCell = currentCell.toFreetextCell()
            let expected = CompositeCell.FreeText oa.NameText
            Expect.equal asNewCell expected ""
        )
        testCase "Unitized toTerm" (fun () ->
            let oa = OntologyAnnotation.fromString("degree celsius", "UO", "UO:000000042")
            let currentCell : CompositeCell = CompositeCell.Unitized <| ("42", oa)
            let asNewCell : CompositeCell = currentCell.toTermCell()
            let expected = CompositeCell.Term <| oa
            Expect.equal asNewCell expected ""
        )
        testCase "Unitized toUnitized" (fun () ->
            let oa = OntologyAnnotation.fromString("degree celsius", "UO", "UO:000000042")
            let currentCell : CompositeCell = CompositeCell.Unitized <| ("42", oa)
            let asNewCell : CompositeCell = currentCell.toUnitizedCell()
            let expected = currentCell
            Expect.equal asNewCell expected ""
        )
    ]

let private tests_create = 
    testList "createX" [
        testCase "createFreeText" (fun () ->
            let newCell : CompositeCell = CompositeCell.createFreeText("Any important value")
            let expected = CompositeCell.FreeText "Any important value"
            Expect.equal newCell expected ""
        )
        testCase "createTerm" (fun () ->
            let oa = OntologyAnnotation.fromString("degree celsius", "UO", "UO:000000042")
            let newCell : CompositeCell = CompositeCell.createTerm(oa)
            let expected = Term oa
            Expect.equal newCell expected ""
        )
        testCase "createTermFromString" (fun () ->
            let newCell : CompositeCell = CompositeCell.createTermFromString("degree celsius", "UO", "UO:000000042")
            let expected = Term <| OntologyAnnotation.fromString("degree celsius", "UO", "UO:000000042")
            Expect.equal newCell expected ""
        )
        testCase "createUnitized_1" (fun () ->
            let newCell : CompositeCell = CompositeCell.createUnitized("42")
            let expected = Unitized ("42", OntologyAnnotation.empty)
            Expect.equal newCell expected ""
        )
        testCase "createUnitized_2" (fun () ->
            let oa = OntologyAnnotation.fromString("degree celsius", "UO", "UO:000000042")
            let newCell : CompositeCell = CompositeCell.createUnitized("42", oa)
            let expected = CompositeCell.Unitized("42", oa)
            Expect.equal newCell expected ""
        )
        testCase "createUnitizedFromString_1" (fun () ->
            let newCell : CompositeCell = CompositeCell.createUnitizedFromString("42")
            let expected = CompositeCell.Unitized("42",OntologyAnnotation.empty)
            Expect.equal newCell expected ""
        )
        testCase "createUnitizedFromString_2" (fun () ->
            let oa = OntologyAnnotation.fromString("degree celsius", "UO", "UO:000000042")
            let newCell : CompositeCell = CompositeCell.createUnitizedFromString("42", "degree celsius", "UO", "UO:000000042")
            let expected = CompositeCell.Unitized("42", oa)
            Expect.equal newCell expected ""
        )
    ]

let main = 
    testList "CompositeCell" [
        tests_cellConverter
        tests_create
    ]