namespace Fable.Daemon

open System
open System.Diagnostics
open System.IO
open System.Threading
open System.Text.Json
open System.Text.Json.Serialization
open System.Threading.Tasks
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Logging.Abstractions
open StreamJsonRpc
open Fable
open FSharp.Compiler.CodeAnalysis
open FSharp.Compiler.SourceCodeServices
open FSharp.Compiler.Diagnostics
open Fable.Compiler.ProjectCracker
open Fable.Compiler.Util
open Fable.Compiler
open Fable.Daemon
open Fable.Daemon.Compiler

type RpcEndpoints(logger: ILogger) =

    let mailbox =
        MailboxProcessor.Start (fun inbox ->
            let rec loop (model : Model) =
                async {
                    let! msg = inbox.Receive ()

                    match msg with
                    | ProjectChanged (payload, replyChannel) ->
                        let! result = tryTypeCheckProject logger model payload

                        match result with
                        | Error error ->
                            replyChannel.Reply (ProjectChangedResult.Error error)
                            return! loop model
                        | Ok result ->

                        replyChannel.Reply (
                            ProjectChangedResult.Success (
                                result.CrackerResponse.ProjectOptions.SourceFiles,
                                mapDiagnostics result.TypeCheckProjectResult.ProjectCheckResults.Diagnostics,
                                result.DependentFiles
                            )
                        )

                        return!
                            loop
                                { model with
                                    CrackerInput = Some result.CrackerInput
                                    Checker = result.Checker
                                    CrackerResponse = result.CrackerResponse
                                    SourceReader = result.SourceReader
                                    TypeCheckProjectResult = result.TypeCheckProjectResult
                                }

                    | CompileFullProject replyChannel ->
                        let! result = tryCompileProject logger model

                        match result with
                        | Error error ->
                            replyChannel.Reply (FilesCompiledResult.Error error)
                            return! loop model
                        | Ok result ->
                            replyChannel.Reply (FilesCompiledResult.Success result.CompiledFSharpFiles)

                            return! loop model

                    | CompileFiles (fileNames, replyChannel) ->
                        let! result = tryCompileFiles logger model fileNames

                        match result with
                        | Error error -> replyChannel.Reply (FileChangedResult.Error error)
                        | Ok result ->
                            replyChannel.Reply (
                                FileChangedResult.Success (result.CompiledFiles, mapDiagnostics result.Diagnostics)
                            )

                        return! loop model
                    | Disconnect -> return ()
                }

            loop
                {
                    CoolCatResolver = CoolCatResolver logger
                    Checker = Unchecked.defaultof<InteractiveChecker>
                    CrackerResponse = Unchecked.defaultof<CrackerResponse>
                    SourceReader = Unchecked.defaultof<SourceReader>
                    PathResolver =
                        { new PathResolver with
                            member _.TryPrecompiledOutPath (_sourceDir, _relativePath) = None
                            member _.GetOrAddDeduplicateTargetDir (importDir, addTargetDir) = importDir
                        }
                    TypeCheckProjectResult = Unchecked.defaultof<TypeCheckProjectResult>
                    CrackerInput = None
                }
        )

    // log or something.
    let subscription = mailbox.Error.Subscribe (fun evt -> ())


    /// [<JsonRpcMethod("fable/project-changed", UseSingleObjectParameterDeserialization = true)>]
    member this.ProjectChanged (p : ProjectChangedPayload, ct: CancellationToken) : Task<ProjectChangedResult> =
        task {
            logger.LogDebug ("enter \"fable/project-changed\" {p}", p)
            let! response = mailbox.PostAndAsyncReply (fun replyChannel -> Msg.ProjectChanged (p, replyChannel))
            logger.LogDebug ("exit \"fable/project-changed\" {response}", response)
            return response
        }

    /// [<JsonRpcMethod("fable/initial-compile", UseSingleObjectParameterDeserialization = true)>]
    member this.InitialCompile (ct: CancellationToken) : Task<FilesCompiledResult> =
        task {
            logger.LogDebug "enter \"fable/initial-compile\""
            let! response = mailbox.PostAndAsyncReply Msg.CompileFullProject

            let logResponse =
                match response with
                | FilesCompiledResult.Error e -> box e
                | FilesCompiledResult.Success result -> result.Keys |> String.concat "\n" |> sprintf "\n%s" |> box

            logger.LogDebug ("exit \"fable/initial-compile\" with {logResponse}", logResponse)
            return response
        }

    /// [<JsonRpcMethod("fable/compile", UseSingleObjectParameterDeserialization = true)>]
    member this.CompileFiles (p : CompileFilesPayload, ct: CancellationToken) : Task<FileChangedResult> =
        task {
            logger.LogDebug ("enter \"fable/compile\" with {p}", p)

            let! response =
                mailbox.PostAndAsyncReply (fun replyChannel ->
                    Msg.CompileFiles (List.ofArray p.FileNames, replyChannel)
                )

            let logResponse =
                match response with
                | FileChangedResult.Error e -> box e
                | FileChangedResult.Success (result, diagnostics) ->
                    let keys = result.Keys |> String.concat "\n" |> sprintf "\n%s"
                    box (keys, diagnostics)

            logger.LogDebug ("exit \"fable/compile\" with {p}", logResponse)
            return response
        }

    interface IDisposable with
        member _.Dispose () =
            if not (isNull subscription) then
                subscription.Dispose ()
            ()
