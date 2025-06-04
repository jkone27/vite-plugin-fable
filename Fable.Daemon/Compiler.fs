module Fable.Daemon.Compiler 

open System
open System.Diagnostics
open Microsoft.Extensions.Logging
open Fable
open FSharp.Compiler.CodeAnalysis
open FSharp.Compiler.SourceCodeServices
open FSharp.Compiler.Diagnostics
open Fable.Compiler.ProjectCracker
open Fable.Compiler.Util
open Fable.Compiler
open Fable.Daemon


let timeAsync f =
    async {
        let sw = Stopwatch.StartNew ()
        let! result = f
        sw.Stop ()
        return result, sw.Elapsed
    }

let tryTypeCheckProject
    (logger : ILogger)
    (model : Model)
    (payload : ProjectChangedPayload)
    : Async<Result<TypeCheckedProjectData, string>>
    =
    async {
        try
            /// Project file will be in the Vite normalized format
            let projectFile = Path.GetFullPath payload.Project
            logger.LogDebug ("start tryTypeCheckProject for {projectFile}", projectFile)

            let cliArgs, crackerOptions =
                match model.CrackerInput with
                | Some {
                           CliArgs = cliArgs
                           CrackerOptions = crackerOptions
                       } -> cliArgs, crackerOptions
                | None ->

                let cliArgs : CliArgs =
                    {
                        ProjectFile = projectFile
                        RootDir = Path.GetDirectoryName payload.Project
                        OutDir = None
                        IsWatch = false
                        Precompile = false
                        PrecompiledLib = None
                        PrintAst = false
                        FableLibraryPath = Some payload.FableLibrary
                        Configuration = payload.Configuration
                        NoRestore = true
                        NoCache = true
                        NoParallelTypeCheck = false
                        SourceMaps = false
                        SourceMapsRoot = None
                        Exclude = List.ofArray payload.Exclude
                        Replace = Map.empty
                        CompilerOptions =
                            {
                                TypedArrays = false
                                ClampByteArrays = false
                                Language = Language.JavaScript
                                Define = [ "FABLE_COMPILER" ; "FABLE_COMPILER_4" ; "FABLE_COMPILER_JAVASCRIPT" ]
                                DebugMode = false
                                OptimizeFSharpAst = false
                                Verbosity = Verbosity.Verbose
                                // We keep using `.fs` for the compiled FSharp file, even though the contents will be JavaScript.
                                FileExtension = ".fs"
                                TriggeredByDependency = false
                                NoReflection = payload.NoReflection
                            }
                        RunProcess = None
                        Verbosity = Verbosity.Verbose
                    }

                cliArgs, CrackerOptions (cliArgs, true)

            let crackerResponse = getFullProjectOpts model.CoolCatResolver crackerOptions
            logger.LogDebug ("CrackerResponse: {crackerResponse}", crackerResponse)
            let checker = InteractiveChecker.Create crackerResponse.ProjectOptions

            let sourceReader =
                Fable.Compiler.File.MakeSourceReader (
                    Array.map Fable.Compiler.File crackerResponse.ProjectOptions.SourceFiles
                )
                |> snd

            let! typeCheckResult, typeCheckTime =
                timeAsync (CodeServices.typeCheckProject sourceReader checker cliArgs crackerResponse)

            logger.LogDebug ("Typechecking {projectFile} took {elapsed}", projectFile, typeCheckTime)

            let dependentFiles =
                model.CoolCatResolver.MSBuildProjectFiles projectFile
                |> List.map (fun fi -> fi.FullName)
                |> List.toArray

            return
                Ok
                    {
                        TypeCheckProjectResult = typeCheckResult
                        CrackerInput =
                            Option.defaultValue
                                {
                                    CliArgs = cliArgs
                                    CrackerOptions = crackerOptions
                                }
                                model.CrackerInput
                        Checker = checker
                        CrackerResponse = crackerResponse
                        SourceReader = sourceReader
                        DependentFiles = dependentFiles
                    }
        with ex ->
            logger.LogCritical ("tryTypeCheckProject threw exception {ex}", ex)
            return Error ex.Message
    }



let private mapRange (m : FSharp.Compiler.Text.range) =
    {
        StartLine = m.StartLine
        StartColumn = m.StartColumn
        EndLine = m.EndLine
        EndColumn = m.EndColumn
    }

let mapDiagnostics (ds : FSharpDiagnostic array) =
    ds
    |> Array.map (fun d ->
        {
            ErrorNumberText = d.ErrorNumberText
            Message = d.Message
            Range = mapRange d.Range
            Severity = string d.Severity
            FileName = d.FileName
        }
    )

let tryCompileProject (logger : ILogger) (model : Model) : Async<Result<CompiledProjectData, string>> =
    async {
        try
            let cachedFableModuleFiles =
                model.CoolCatResolver.TryGetCachedFableModuleFiles model.CrackerResponse.ProjectOptions.ProjectFileName

            let files =
                let cachedFiles = cachedFableModuleFiles.Keys |> Set.ofSeq

                model.CrackerResponse.ProjectOptions.SourceFiles
                |> Array.filter (fun sf ->
                    not (sf.EndsWith (".fsi", StringComparison.Ordinal))
                    && not (cachedFiles.Contains sf)
                )

            match model.CrackerInput with
            | None ->
                logger.LogCritical "tryCompileProject is entered without CrackerInput"
                return raise (exn "tryCompileProject is entered without CrackerInput")
            | Some { CliArgs = cliArgs } ->

            let! initialCompileResponse =
                CodeServices.compileMultipleFilesToJavaScript
                    model.PathResolver
                    cliArgs
                    model.CrackerResponse
                    model.TypeCheckProjectResult
                    files

            if cachedFableModuleFiles.IsEmpty then
                let fableModuleFiles =
                    initialCompileResponse.CompiledFiles
                    |> Map.filter (fun key _value -> key.Contains "fable_modules")

                model.CoolCatResolver.WriteCachedFableModuleFiles
                    model.CrackerResponse.ProjectOptions.ProjectFileName
                    fableModuleFiles

            let compiledFiles =
                (initialCompileResponse.CompiledFiles, cachedFableModuleFiles)
                ||> Map.fold (fun state key value -> Map.add key value state)

            return Ok { CompiledFSharpFiles = compiledFiles }
        with ex ->
            logger.LogCritical ("tryCompileProject threw exception {ex}", ex)
            return Error ex.Message
    }



/// Find all the dependent files as efficient as possible.
let rec getDependentFiles
    (sourceReader : SourceReader)
    (projectOptions : FSharpProjectOptions)
    (checker : InteractiveChecker)
    (inputFiles : string list)
    (result : Set<string>)
    : Async<Set<string>>
    =
    async {
        match inputFiles with
        | [] ->
            // Filter out the signature files at the end.
            return
                result
                |> Set.filter (fun f -> not (f.EndsWith (".fsi", StringComparison.Ordinal)))
        | head :: tail ->

        // If the file is already part of the collection, it can safely be skipped.
        if result.Contains head then
            return! getDependentFiles sourceReader projectOptions checker tail result
        else

        let! nextFiles = checker.GetDependentFiles (head, projectOptions.SourceFiles, sourceReader)
        let nextResult = (result, nextFiles) ||> Array.fold (fun acc f -> Set.add f acc)

        return! getDependentFiles sourceReader projectOptions checker tail nextResult
    }

let tryCompileFiles
    (logger : ILogger)
    (model : Model)
    (fileNames : string list)
    : Async<Result<CompiledFileData, string>>
    =
    async {
        try
            let fileNames = List.map Path.normalizePath fileNames
            logger.LogDebug ("tryCompileFile {fileNames}", fileNames)

            match model.CrackerInput with
            | None ->
                logger.LogCritical "tryCompileFile is entered without CrackerInput"
                return raise (exn "tryCompileFile is entered without CrackerInput")
            | Some { CliArgs = cliArgs } ->

            // Choose the signature file in the pair if it exists.
            let mapLeadingFile (file : string) : string =
                if file.EndsWith (".fsi", StringComparison.Ordinal) then
                    file
                else
                    model.CrackerResponse.ProjectOptions.SourceFiles
                    |> Array.tryFind (fun f -> f = String.Concat (file, "i"))
                    |> Option.defaultValue file

            let sourceReader =
                Fable.Compiler.File.MakeSourceReader (
                    Array.map Fable.Compiler.File model.CrackerResponse.ProjectOptions.SourceFiles
                )
                |> snd

            let! filesToCompile =
                let input = List.map mapLeadingFile fileNames
                getDependentFiles sourceReader model.CrackerResponse.ProjectOptions model.Checker input Set.empty

            logger.LogDebug ("About to compile {allFiles}", filesToCompile)

            // Type-check the project up until the last file
            let lastFile =
                model.CrackerResponse.ProjectOptions.SourceFiles
                |> Array.tryFindBack filesToCompile.Contains
                |> Option.defaultValue (Array.last model.CrackerResponse.ProjectOptions.SourceFiles)

            let! checkProjectResult =
                model.Checker.ParseAndCheckProject (
                    cliArgs.ProjectFile,
                    model.CrackerResponse.ProjectOptions.SourceFiles,
                    sourceReader,
                    lastFile = lastFile
                )

            let! compiledFileResponse =
                Fable.Compiler.CodeServices.compileMultipleFilesToJavaScript
                    model.PathResolver
                    cliArgs
                    model.CrackerResponse
                    { model.TypeCheckProjectResult with
                        ProjectCheckResults = checkProjectResult
                    }
                    filesToCompile

            return
                Ok
                    {
                        CompiledFiles = compiledFileResponse.CompiledFiles
                        Diagnostics = compiledFileResponse.Diagnostics
                    }
        with ex ->
            logger.LogCritical ("tryCompileFile threw exception {ex}", ex)
            return Error ex.Message
    }