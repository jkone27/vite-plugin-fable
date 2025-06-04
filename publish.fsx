#r "nuget: Fake.DotNet.Cli"
#r "nuget: Fake.IO.FileSystem"
open Fake.DotNet
open Fake.IO
open System.IO
open System

// Publish Fable.Daemon in Release config, DISABLE binlog to avoid MSBuild binlog version error
let publishResult =
    DotNet.publish (fun p ->
        { p with
            Configuration = DotNet.BuildConfiguration.Release
            MSBuildParams =
                { p.MSBuildParams with
                    DisableInternalBinLog = true // disables binlog generation
                }
        }) "./Fable.Daemon/Fable.Daemon.fsproj"

printfn "[Publish] completed"

// Ensure bin directory exists
let binDir = Path.Combine(__SOURCE_DIRECTORY__, "bin")

if not (Directory.Exists binDir) then 
    Directory.CreateDirectory binDir |> ignore
    printfn "[CreateDir] created ./bin"

// Copy published files to ./bin
Path.Combine(__SOURCE_DIRECTORY__, "artifacts/publish/Fable.Daemon/release")
|> DirectoryInfo
|> DirectoryInfo.getFiles
|> Seq.iter (fun file ->
    let dest = Path.Combine(binDir, file.Name)
    File.Copy(file.FullName, dest, true)
    printfn "[Copied] %s to %s" file.FullName dest)