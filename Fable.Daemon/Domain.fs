namespace Fable.Daemon


open Fable
open FSharp.Compiler.SourceCodeServices
open Fable.Compiler.ProjectCracker
open Fable.Compiler.Util
open Fable.Compiler
open Fable.Daemon
open FSharp.Compiler.Diagnostics

type Msg =
    | ProjectChanged of payload : ProjectChangedPayload * AsyncReplyChannel<ProjectChangedResult>
    | CompileFullProject of AsyncReplyChannel<FilesCompiledResult>
    | CompileFiles of fileNames : string list * AsyncReplyChannel<FileChangedResult>
    | Disconnect

/// Input for every getFullProjectOpts
/// Should be reused for subsequent type checks.
type CrackerInput =
    {
        CliArgs : CliArgs
        /// Reuse the cracker options in future design time builds
        CrackerOptions : CrackerOptions
    }

type Model =
    {
        CoolCatResolver : CoolCatResolver
        Checker : InteractiveChecker
        CrackerInput : CrackerInput option
        CrackerResponse : CrackerResponse
        SourceReader : SourceReader
        PathResolver : PathResolver
        TypeCheckProjectResult : TypeCheckProjectResult
    }

type TypeCheckedProjectData =
    {
        TypeCheckProjectResult : TypeCheckProjectResult
        CrackerInput : CrackerInput
        Checker : InteractiveChecker
        CrackerResponse : CrackerResponse
        SourceReader : SourceReader
        /// An array of files that influence the design time build
        /// If any of these change, the plugin should respond accordingly.
        DependentFiles : FullPath array
    }

type CompiledProjectData =
    {
        CompiledFSharpFiles : Map<string, string>
    }

type CompiledFileData =
    {
        CompiledFiles : Map<string, string>
        Diagnostics : FSharpDiagnostic array
    }
