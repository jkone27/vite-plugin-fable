module Components.RootComponent

open Feliz
open Fable.Core

JsInterop.importSideEffects "./app.css"


[<ReactComponent>]
let El () =
    let count, setCount = React.useState 0
    React.fragment [
        Test.El({| name = "TestComponent" |})
        Html.h1 "Vite plugin rocks!"
        Html.button [
            prop.onClick (fun _ -> setCount (count + 1))
            prop.text $"Current state {count}"
        ]
    ]
