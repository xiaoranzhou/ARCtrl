﻿module ReleaseNotesTasks

open Fake.Extensions.Release
open BlackFox.Fake
open Fake.Core

/// This might not be necessary, mostly useful for apps which want to display current version as it creates an accessible F# version script from RELEASE_NOTES.md
let createAssemblyVersion = BuildTask.create "createvfs" [] {
    AssemblyVersion.create ProjectInfo.project
}

// https://github.com/Freymaurer/Fake.Extensions.Release#releaseupdate
let updateReleaseNotes = BuildTask.createFn "ReleaseNotes" [] (fun config ->
    ReleaseNotes.update(ProjectInfo.gitOwner, ProjectInfo.project, config)

    let semVer = 
        Fake.Core.ReleaseNotes.load "RELEASE_NOTES.md"
        |> fun x -> x.SemVer.AsString
    Trace.trace "Start updating package.json version"
    // Update Version in src/Nfdi4Plants.Fornax.Template/package.json
    let p = "build/release_package.json"
    let t = System.IO.File.ReadAllText p
    let tNew = System.Text.RegularExpressions.Regex.Replace(t, """\"version\": \".*\",""", sprintf "\"version\": \"%s\"," semVer )
    System.IO.File.WriteAllText(p, tNew)
    Trace.trace "Finish updating package.json version"
)


// https://github.com/Freymaurer/Fake.Extensions.Release#githubdraft
let githubDraft = BuildTask.createFn "GithubDraft" [] (fun config ->

    let body = "We are ready to go for the first release!"

    Github.draft(
        ProjectInfo.gitOwner,
        ProjectInfo.project,
        (Some body),
        None,
        config
    )
)