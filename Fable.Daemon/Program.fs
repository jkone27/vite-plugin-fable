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
open Fable.Compiler.Util
open Fable.Daemon

module Rpc =

    // only works for reflection
    // let singleObjSettings = new JsonRpcMethodAttribute(UseSingleObjectParameterDeserialization = true)

    let mapFunc name func (rpc: JsonRpc)  = 
        rpc.AddLocalRpcMethod(name,  new Func<'a, 'b>(func))
        rpc
        
    let mapUnitFunc name func (rpc: JsonRpc)  = 
        rpc.AddLocalRpcMethod(name,  new Func<'a>(func))
        rpc

type FableServer(sender : Stream, reader : Stream, logger : ILogger) as this =
    let jsonMessageFormatter = new SystemTextJsonFormatter ()

    do
        jsonMessageFormatter.JsonSerializerOptions <-
            let options =
                JsonSerializerOptions (PropertyNamingPolicy = JsonNamingPolicy.CamelCase)

            let jsonFSharpOptions =
                JsonFSharpOptions.Default().WithUnionTagName("case").WithUnionFieldsName ("fields")

            options.Converters.Add (JsonUnionConverter jsonFSharpOptions)
            options

    let cts = new CancellationTokenSource ()

    do
        match logger with
        | :? Debug.InMemoryLogger as logger ->
            let server = Debug.startWebserver logger cts.Token
            Async.Start (server, cts.Token)
        | _ -> ()

    let handler =
        new HeaderDelimitedMessageHandler (sender, reader, jsonMessageFormatter)

    let rpc = new JsonRpc (handler, this)
    
    let endpoints = new RpcEndpoints(logger)

    interface IDisposable with
        member _.Dispose () =
            let dEndpoints: IDisposable = endpoints
            if not (isNull dEndpoints) then
                dEndpoints.Dispose ()

            if not cts.IsCancellationRequested then
                cts.Cancel ()

            ()

    /// returns a hot task that resolves when the stream has terminated
    member this.WaitForClose = rpc.Completion

    member _.ProjectChanged (p : ProjectChangedPayload) : Task<ProjectChangedResult> =
        endpoints.ProjectChanged(p, cts.Token)

    member _.InitialCompile () : Task<FilesCompiledResult> =
        endpoints.InitialCompile(cts.Token)

    member _.CompileFiles (p : CompileFilesPayload) : Task<FileChangedResult> =
        endpoints.CompileFiles(p, cts.Token)

    member this.Start() = 
        rpc 
        |> Rpc.mapFunc "fable/project-changed" endpoints.ProjectChanged
        |> Rpc.mapFunc "fable/initial-compile" endpoints.InitialCompile
        |> Rpc.mapFunc "fable/compile" endpoints.CompileFiles
        |> _.StartListening()


let input = Console.OpenStandardInput ()
let output = Console.OpenStandardOutput ()

let logger : ILogger =
    let envVar = Environment.GetEnvironmentVariable "VITE_PLUGIN_FABLE_DEBUG"

    if not (String.IsNullOrWhiteSpace envVar) && not (envVar = "0") then
        Debug.InMemoryLogger ()
    else
        NullLogger.Instance

// Set Fable logger
Log.setLogger Verbosity.Verbose logger

let daemon =
    new FableServer (Console.OpenStandardOutput (), Console.OpenStandardInput (), logger)

daemon.Start()

AppDomain.CurrentDomain.ProcessExit.Add (fun _ -> (daemon :> IDisposable).Dispose ())
daemon.WaitForClose.GetAwaiter().GetResult ()
exit 0
