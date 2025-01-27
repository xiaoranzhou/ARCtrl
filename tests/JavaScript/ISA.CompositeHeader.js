import { equal, deepEqual, notEqual } from 'assert';
import { CompositeHeader, IOType } from "./ARCtrl/ISA/ISA/ArcTypes/CompositeHeader.js"
import { OntologyAnnotation } from './ARCtrl/ISA/ISA/JsonTypes/OntologyAnnotation.js';

function tests_IOType() {
    describe('IOType', function () {
        it('cases', function () {
            let cases = IOType.Cases
            //console.log(cases)
            equal(cases.length, 7);
        });
        it('Create non Freetext', function () {
            for (let mycase of IOType.Cases) {
                let tag = mycase[0]
                let iotype = new IOType(tag, [])
                switch (tag) {
                    case 0:
                        equal(iotype.asInput, "Input [Source Name]");
                        break;
                    case 1:
                        equal(iotype.asInput, "Input [Sample Name]");
                        break;
                    case 2:
                        equal(iotype.asInput, "Input [Raw Data File]");
                        break;
                    case 3:
                        equal(iotype.asInput, "Input [Derived Data File]");
                        break;
                    case 4:
                        equal(iotype.asInput, "Input [Image File]");
                        break;
                    case 5:
                        equal(iotype.asInput, "Input [Material]");
                        break;
                    case 6:
                        equal(iotype.asInput, "Input [undefined]");
                        break;
                }
            }
        });
        it('Create FreeText', function () {
            let freetext = new IOType(6, ["My FreeTextValue"])
            let asinput = freetext.asInput
            equal(asinput, "Input [My FreeTextValue]")
        })
    });
}

describe('CompositeHeader', function () {
    tests_IOType();
    it("Input", function () {
        let iotype = new IOType(6, ["My FreeTextValue"])
        let header = new CompositeHeader(11, [iotype])
        let actual = header.toString()
        equal(actual, "Input [My FreeTextValue]")
    });
    it("FreeText", function () {
        let header = new CompositeHeader(13, ["My FreeTextValue"])
        let actual = header.toString()
        equal(actual, "My FreeTextValue")
    });
    it("Term", function () {
        let oa = OntologyAnnotation.fromString("My OA Name")
        let header = new CompositeHeader(0, [oa])
        let actual = header.toString()
        //console.log(CompositeHeader.Cases)
        equal(actual, "Component [My OA Name]")
    });
    it('jsGetColumnMetaType', function () {
        let cases = CompositeHeader.Cases
        let oa = OntologyAnnotation.fromString("My OA Name")
        let iotype = new IOType(0, [])
        let stringExample = "My Example"
        for (let mycase of cases) {
            let tag = mycase[0]
            let code = CompositeHeader.jsGetColumnMetaType(tag)
            switch (code) {
                case 0:
                    let header1 = new CompositeHeader(tag, [])
                    equal((header1.IsSingleColumn || header1.IsFeaturedColumn), true);
                    break;
                case 1:
                    let header2 = new CompositeHeader(tag, [oa])
                    equal(header2.IsTermColumn, true);
                    break;
                case 2:
                    let header3 = new CompositeHeader(tag, [iotype])
                    equal(header3.IsIOType, true);
                    break;
                case 3:
                    let header4 = new CompositeHeader(tag, [stringExample])
                    equal(header4.isFreeText, true);
                    break;
            }
        }
    });
});