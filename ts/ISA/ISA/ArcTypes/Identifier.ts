import { match, isMatch } from "../../../fable_modules/fable-library-ts/RegExp.js";
import { newGuid } from "../../../fable_modules/fable-library-ts/Guid.js";
import { combineMany } from "../../../FileSystem/Path.js";

export function checkValidCharacters(identifier: string): void {
    if (!isMatch(/^[a-zA-Z0-9_\- ]+$/gu, identifier)) {
        throw new Error("New identifier contains forbidden characters! Allowed characters are: letters, digits, underscore (_), dash (-) and whitespace ( ).");
    }
}

export function createMissingIdentifier(): string {
    let copyOfStruct: string;
    return "MISSING_IDENTIFIER_" + ((copyOfStruct = newGuid(), copyOfStruct));
}

export function isMissingIdentifier(str: string): boolean {
    return str.indexOf("MISSING_IDENTIFIER_") === 0;
}

export function removeMissingIdentifier(str: string): string {
    if (str.indexOf("MISSING_IDENTIFIER_") === 0) {
        return "";
    }
    else {
        return str;
    }
}

/**
 * On read-in the FileName can be any combination of "assays" (assay folder name), assayIdentifier and "isa.assay.xlsx" (the actual file name).
 * 
 * This functions extracts assayIdentifier from any of these combinations and returns it.
 */
export function Assay_identifierFromFileName(fileName: string): string {
    const m: any = match(/^(assays(\/|\\))?(?<identifier>[a-zA-Z0-9_\- ]+)((\/|\\)isa.assay.xlsx)?$/gu, fileName);
    const matchValue: boolean = m != null;
    if (matchValue) {
        return (m.groups && m.groups.identifier) || "";
    }
    else {
        throw new Error(`Cannot parse identifier from FileName \`${fileName}\``);
    }
}

/**
 * On writing a xlsx file we unify our output to a relative path to ARC root. So: `assays/assayIdentifier/isa.assay.xlsx`.
 */
export function Assay_fileNameFromIdentifier(identifier: string): string {
    checkValidCharacters(identifier);
    return combineMany(["assays", identifier, "isa.assay.xlsx"]);
}

/**
 * On read-in the FileName can be any combination of "studies" (study folder name), studyIdentifier and "isa.study.xlsx" (the actual file name).
 * 
 * This functions extracts studyIdentifier from any of these combinations and returns it.
 */
export function Study_identifierFromFileName(fileName: string): string {
    const m: any = match(/^(studies(\/|\\))?(?<identifier>[a-zA-Z0-9_\- ]+)((\/|\\)isa.study.xlsx)?$/gu, fileName);
    const matchValue: boolean = m != null;
    if (matchValue) {
        return (m.groups && m.groups.identifier) || "";
    }
    else {
        throw new Error(`Cannot parse identifier from FileName \`${fileName}\``);
    }
}

/**
 * On writing a xlsx file we unify our output to a relative path to ARC root. So: `studies/studyIdentifier/isa.study.xlsx`.
 */
export function Study_fileNameFromIdentifier(identifier: string): string {
    checkValidCharacters(identifier);
    return combineMany(["studies", identifier, "isa.study.xlsx"]);
}
