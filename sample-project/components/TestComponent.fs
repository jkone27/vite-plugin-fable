module Components.Test

open Feliz

[<ReactComponent>]
let El (props: {| name: string |}) =
    React.fragment [
        Html.div [
            prop.style [
                style.backgroundColor "yellow"
                style.animationName "spin"
                style.animationDuration 3
                style.animationIterationCount.initial
                style.animationTimingFunction.linear
            ]
            prop.children [
                Html.h1 $"My name is: {props.name}!"
            ]
        ]
        Html.style [ prop.text "@keyframes spin { from { transform: rotate(0deg); } to { transform: rotate(360deg); } }" ]
    ]
