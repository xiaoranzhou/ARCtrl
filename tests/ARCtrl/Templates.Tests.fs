﻿module ARCtrl.Templates.Tests

#if FABLE_COMPILER
open Fable.Mocha
open Thoth.Json
#else
open Expecto
open Thoth.Json.Net
#endif

open ARCtrl.Templates.Json
open ARCtrl.ISA

let private tests_Organisation = testList "Organisation" [
    testList "encode" [
        testCase "DataPLANT" <| fun _ ->
            let o = Organisation.DataPLANT
            let actual = Organisation.encode o |> Encode.toString 4
            let expected = "\"DataPLANT\""
            Expect.equal actual expected ""
        testCase "Other" <| fun _ ->
            let o = Organisation.Other "My Custom Org"
            let actual = Organisation.encode o |> Encode.toString 4
            // if this ever fails with:
            // --> String does not match at position 1. Expected char: '\013', but got '\010'.
            // You might have to change your vs community settings:
            // Edit -> Advanced -> "Set End of Line Sequence" -> LF
            // That worked for me ~Kevin F, 09.2023
            let expected = "[
    \"Other\",
    \"My Custom Org\"
]"
            Expect.equal actual expected "check comment if this fails with: \"Expected char: '\013', but got '\010'\""
    ]
    testList "decode" [
        testCase "DataPLANT" <| fun _ ->
            let json = "\"DataPLANT\""
            let actual = Decode.fromString Organisation.decode json
            let expected =  Ok (Organisation.DataPLANT)
            Expect.equal actual expected ""
        testCase "Other" <| fun _ ->
            let json = "[
    \"Other\",
    \"My Custom Org\"
]"
            let actual = Decode.fromString Organisation.decode json
            let expected =  Ok (Other "My Custom Org")
            Expect.equal actual expected ""
    ]
    testList "roundabout" [
        testCase "DataPLANT" <| fun _ ->
            let o = Organisation.DataPLANT
            let json = Organisation.encode o |> Encode.toString 4
            let actual = Decode.fromString Organisation.decode json
            let expected = Ok (o)
            Expect.equal actual expected ""
        testCase "Other" <| fun _ ->
            let o = Organisation.Other "My Custom Org"
            let json = Organisation.encode o |> Encode.toString 4
            let actual = Decode.fromString Organisation.decode json
            let expected = Ok (o)
            Expect.equal actual expected ""
    ]
]

let private tests_CompositeCell = testList "CompositeCell" [
        testList "roundabout" [
            testCase "Freetext" <| fun _ ->
                let o = CompositeCell.createFreeText "My Freetext"
                let json = CompositeCell.encode o |> Encode.toString 4
                let actual = Decode.fromString CompositeCell.decode json
                let expected = Ok o
                Expect.equal actual expected ""
            testCase "Term" <| fun _ ->
                let o = CompositeCell.createTermFromString "My Term"
                let json = CompositeCell.encode o |> Encode.toString 4
                let actual = Decode.fromString CompositeCell.decode json
                let expected = Ok o
                Expect.equal actual expected ""
            testCase "Unitized" <| fun _ ->
                let o = CompositeCell.createUnitizedFromString ("12","My Term")
                let json = CompositeCell.encode o |> Encode.toString 4
                let actual = Decode.fromString CompositeCell.decode json
                let expected = Ok o
                Expect.equal actual expected ""
        ]
    ]

let private tests_CompositeHeader = testList "CompositeHeader" [
        testList "roundabout" [
            testCase "Input/Output" <| fun _ ->
                let o = CompositeHeader.Input IOType.Source
                let json = CompositeHeader.encode o |> Encode.toString 4
                let actual = Decode.fromString CompositeHeader.decode json
                let expected = Ok o
                Expect.equal actual expected ""
            testCase "Single" <| fun _ ->
                let o = CompositeHeader.ProtocolREF
                let json = CompositeHeader.encode o |> Encode.toString 4
                let actual = Decode.fromString CompositeHeader.decode json
                let expected = Ok o
                Expect.equal actual expected ""
            testCase "Term" <| fun _ ->
                let o = CompositeHeader.Characteristic (OntologyAnnotation.fromString "My Chara")
                let json = CompositeHeader.encode o |> Encode.toString 4
                let actual = Decode.fromString CompositeHeader.decode json
                let expected = Ok o
                Expect.equal actual expected ""
        ]
    ]

let tests_ArcTable = testList "ArcTable" [  
        testList "roundabout" [
            testCase "complete" <| fun _ ->
                let o = ArcTable.init("My Table")
                o.AddColumn(CompositeHeader.Input IOType.Source, [|for i in 0 .. 9 do yield CompositeCell.createFreeText($"Source {i}")|])
                o.AddColumn(CompositeHeader.Output IOType.RawDataFile, [|for i in 0 .. 9 do yield CompositeCell.createFreeText($"Output {i}")|])
                let json = Encode.toString 4 (ArcTable.encode o)
                let actual = Decode.fromString ArcTable.decode json
                let expected = Ok o
                Expect.equal actual expected ""
        ]
    ]

let tests_Template = testList "Template" [
        testList "roundabout" [
            testCase "complete" <| fun _ ->
                let table = ArcTable.init("My Table")
                table.AddColumn(CompositeHeader.Input IOType.Source, [|for i in 0 .. 9 do yield CompositeCell.createFreeText($"Source {i}")|])
                table.AddColumn(CompositeHeader.Output IOType.RawDataFile, [|for i in 0 .. 9 do yield CompositeCell.createFreeText($"Output {i}")|])
                let o = Template.init("MyTemplate")
                o.Table <- table
                o.Authors <- [|ARCtrl.ISA.Person.create(FirstName="John", LastName="Doe"); ARCtrl.ISA.Person.create(FirstName="Jane", LastName="Doe");|]
                o.EndpointRepositories <- [|ARCtrl.ISA.OntologyAnnotation.fromString "Test"; ARCtrl.ISA.OntologyAnnotation.fromString "Testing second"|]
                let json = Encode.toString 4 (Template.encode o)
                let actual = Decode.fromString Template.decode json
                let expected = o
                Expect.isOk actual "Ok"
                let actualValue = actual |> Result.toOption |> Option.get
                Expect.isTrue (actualValue.StructurallyEquivalent(expected)) "structurallyEquivalent"
        ]
    ]

let private tests_json = testList "Json" [
    tests_Organisation
    tests_CompositeCell
    tests_CompositeHeader
    tests_ArcTable
]

let main = testList "Templates" [
    tests_json
]